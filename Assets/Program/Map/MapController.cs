using UnityEngine;
using UnityEngine.Rendering;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using System;

public class MapController : MonoBehaviour
{
    [Header("References (External)")]
    [SerializeField] SoMMapData mapData;

    [Header("References (Internal)")]
    [SerializeField] Light directionalLightA;
    [SerializeField] Light directionalLightB;
    [SerializeField] Light directionalLightC;
    [SerializeField] BaseSky skyObject;

    // Properties
    public bool IsMapExited { get; private set; } = true;

    // ECS Archetypes    
    RenderFilterSettings ObjectRenderSettings;
    EntityArchetype ArchetypeObjectRoot, ArchetypeObjectMesh;
    EntityArchetype ArchetypeTileRoot, ArchetypeTileMesh;
    EntityArchetype ArchetypeChild;

    bool archetypesAreInitialized = false;

    /// <summary>Singleton Instance.</summary>
    public static MapController Instance { get; private set; }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void Awake()
    {
        // Singleton Implementation.
        if (Instance != null)
            throw new DuplicateSingletonException();

        Instance = this;

        // ECS archetype construction...
        InitializeArchetypes();
    }

    /// <summary>
    /// Loads a map and sets it up for play. Can only be called when the exited flag is true!
    /// </summary>
    public void LoadMap(int mapID)
    {
        if (!IsMapExited)
            return;

        // Load the map data...
        mapData.Load(mapID);

        // Also load up the events for this map...
        if (ResourceManager.Find($"DATA\\MAP\\{mapID:D2}.evt", out string foundEvtFile))
            EventManager.Instance.LoadEventsFromFile(foundEvtFile);
        else
            throw new Exception("Failed to load EVT data");

        // Any data from the save file to override the loaded map with?..

        // Setup Segment
        SetupMapEnviroment();

        SetupMapPlayer();
        SetupMapCamera();

        SetupMapTiles();
        SetupMapObjects();

        SetupMapMusic();

        // After map load we execute the "Open Map" System Event
        EventManager.Instance.ExecuteEvent(0);

        // After all of this nonsense, purge the resource manager to free now unused assets...
        ResourceManager.Purge();
    }

    /// <summary>
    /// Call to exit the map.
    /// </summary>
    public void ExitMap()
    {
        IsMapExited = true;
    }

    /// <summary>
    /// Initializes all ECS archetypes for map loading
    /// </summary>
    void InitializeArchetypes()
    {
        if (archetypesAreInitialized)
            return;

        // Get the entity manager
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Tile Archetypes
        ArchetypeTileRoot = entityManager.CreateArchetype(
            typeof(LocalTransform),
            typeof(LocalToWorld),
            typeof(LinkedEntityGroup)
            );

        ArchetypeTileMesh = entityManager.CreateArchetype(
            typeof(Parent),
            typeof(Static),
            typeof(LocalTransform),
            typeof(LocalToWorld),
            typeof(PerInstanceCullingTag),
            typeof(WorldToLocal_Tag),
            typeof(DepthSorted_Tag),
            typeof(RenderBounds),
            typeof(RenderMeshArray),
            typeof(RenderFilterSettings),
            typeof(MaterialMeshInfo),
            typeof(WorldRenderBounds),
            typeof(PhysicsCollider),
            typeof(PhysicsWorldIndex)
            );

        // FX Archetypes
        // TO-DO

        // Object Archetypes
        ObjectRenderSettings = new RenderFilterSettings
        {
            MotionMode         = MotionVectorGenerationMode.ForceNoMotion,
            Layer              = 0,
            ReceiveShadows     = GameManager.Instance.RenderStyle.EnableRealTimeShadows,
            ShadowCastingMode  = GameManager.Instance.RenderStyle.EnableRealTimeShadows ? ShadowCastingMode.On : ShadowCastingMode.Off,
            RenderingLayerMask = 1,
            StaticShadowCaster = false
        };

        ArchetypeObjectRoot = entityManager.CreateArchetype(
            typeof(LocalTransform),
            typeof(LocalToWorld),
            typeof(LinkedEntityGroup),
            typeof(RuntimeMapObject)
            );

        ArchetypeObjectMesh = entityManager.CreateArchetype(
            typeof(Parent),
            typeof(LocalTransform),
            typeof(LocalToWorld),
            typeof(PerInstanceCullingTag),
            typeof(WorldToLocal_Tag),
            typeof(DepthSorted_Tag),
            typeof(RenderBounds),
            typeof(RenderMeshArray),
            typeof(RenderFilterSettings),
            typeof(MaterialMeshInfo),
            typeof(WorldRenderBounds)
            );

        // Item Archetypes
        // TO-DO

        // Enemy Archetypes
        // TO-DO

        // NPC Archetypes
        // TO-DO

        // Generic reusable archetypes
        ArchetypeChild = entityManager.CreateArchetype(
            typeof(Parent),
            typeof(LocalTransform),
            typeof(LocalToWorld)
            );

        archetypesAreInitialized = true;
    }

    /// <summary>
    /// Sets up the map enviroment, including fog, lighting and sky
    /// </summary>
    void SetupMapEnviroment()
    {
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogEndDistance   = mapData.CameraZFar;
        RenderSettings.fogStartDistance = mapData.CameraZFar * mapData.EnviromentFogDistance;
        RenderSettings.fogColor         = mapData.EnviromentFogColour;
        RenderSettings.fog              = true;

        RenderSettings.ambientMode  = AmbientMode.Flat;
        RenderSettings.ambientLight = mapData.EnviromentAmbientColour;

        // Directional Lights
        SetupMapDirectionalLight(directionalLightA, mapData.EnviromentDirLightAColour, mapData.EnviromentDirLightADirection);
        SetupMapDirectionalLight(directionalLightB, mapData.EnviromentDirLightBColour, mapData.EnviromentDirLightBDirection);
        SetupMapDirectionalLight(directionalLightC, mapData.EnviromentDirLightCColour, mapData.EnviromentDirLightCDirection);

        // Sky
        SetupMapSky();
    }

    /// <summary>
    /// Sets up a single directional light source
    /// </summary>
    void SetupMapDirectionalLight(Light target, Color32 colour, Vector3 direction)
    {
        // Check for valid colour - if it's 0,0,0 - the light is off...
        if (colour.r == 0 && colour.g == 0 && colour.b == 0)
            target.gameObject.SetActive(false);
        else
        {
            // Transform - must be modified to work with Unity.
            // ^ Tears of the Moon is a good game for matching these.
            target.transform.eulerAngles = direction * Mathf.Rad2Deg;

            // Colour
            target.color = colour;

            // Shadow settings
            target.shadows = GameManager.Instance.RenderStyle.EnableRealTimeShadows switch
            {
                false => LightShadows.None,
                true  => LightShadows.Hard
            };
            
            target.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Sets up the sky type
    /// </summary>
    void SetupMapSky()
    {
        if (mapData.WorldSkyType > 0)
        {
            // We only support classic sky type for now, but the base class is implemented for future expansion
            GameObject skyGameObject     = new ("Sky");
            MeshRenderer skyMeshRenderer = skyGameObject.AddComponent<MeshRenderer>();
            MeshFilter skyMeshFilter     = skyGameObject.AddComponent<MeshFilter>();
            skyObject                    = skyGameObject.AddComponent<ClassicSky>();

            // Weird, weird logic to force the object into the map scene...
            skyGameObject.transform.parent = this.transform;
            skyGameObject.transform.parent = null;

            // Set up classic sky
            ClassicSky classicSky        = (skyObject as ClassicSky);
            classicSky.Initialize(skyMeshRenderer, skyMeshFilter);
            classicSky.LoadClassicSky((int)mapData.WorldSkyType);
        }
    }

    /// <summary>
    /// Sets up player camera with fov and z near/far
    /// </summary>
    void SetupMapCamera()
    {
        // Camera should always clear to the fog colour...
        Camera playerCamera = GameManager.Instance.PlayerController.Camera;

        playerCamera.clearFlags = CameraClearFlags.SolidColor | CameraClearFlags.Depth;
        playerCamera.backgroundColor = mapData.EnviromentFogColour;

        // FoV never really changes in SoM, but here we are...
        playerCamera.fieldOfView     = mapData.CameraFoV;
        playerCamera.nearClipPlane   = mapData.CameraZNear;

        // TO-DO: This needs moving to a tweaks config
        if (GameManager.Instance.BoostCameraZFarBehindFog)
            playerCamera.farClipPlane = mapData.CameraZFar * 2F;
        else
            playerCamera.farClipPlane = mapData.CameraZFar;
    }

    /// <summary>
    /// Sets up the initial player spawn (probably ought to be overridable for events?)
    /// </summary>
    void SetupMapPlayer()
    {
        // X and Z can be got from map data as is
        float playerX = mapData.PlayerDefaultStartPosition.x;
        float playerZ = mapData.PlayerDefaultStartPosition.z;

        // Y must be got from tile elevation.
        float playerY = mapData.GetElevationFromPosition(playerX, playerZ) + mapData.PlayerDefaultStartPosition.y;

        // Set player position
        GameManager.Instance.PlayerController.Teleport(
            new Vector3(playerX, playerY, playerZ), 
            new Vector3(mapData.PlayerDefaultStartDirection, 0F, 0F)
        );
    }

    /// <summary>
    /// Sets up the tilemap
    /// </summary>
    void SetupMapTiles()
    {
        // Get the entity manager
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Mesh filter settings will stay the same for each tile, so set them up now
        RenderFilterSettings tileEntityRenderFilterSettings = new RenderFilterSettings
        {
            MotionMode         = MotionVectorGenerationMode.ForceNoMotion,
            Layer              = 0,
            ReceiveShadows     = GameManager.Instance.RenderStyle.EnableRealTimeShadows,
            ShadowCastingMode  = GameManager.Instance.RenderStyle.EnableRealTimeShadows ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off,
            RenderingLayerMask = 1,
            StaticShadowCaster = true & GameManager.Instance.RenderStyle.EnableRealTimeShadows
        };

        // We're using one global render mesh array with all unique tiles contained
        RenderMeshArray tileMeshData = new RenderMeshArray(mapData.RenderMaterials, mapData.RenderMeshes);

        // Spawn entities for tiles
        for (int i = 0; i < mapData.WorldTiles.Length; ++i)
        {
            // Get map tile...
            MapTile mapTile = mapData.WorldTiles[i];

            // Skip invalid tiles...
            if (!mapTile.used)
                continue;

            // Calculate tile position
            int tilePositionX = (int)(i % mapData.WorldWidth);
            int tilePositionZ = (int)(i / mapData.WorldWidth);

            // Create LocalTransform
            LocalTransform tileMeshTransform = LocalTransform.FromPositionRotation(
                new float3(2F * tilePositionX, mapTile.elevation, 2F * tilePositionZ),
                quaternion.RotateY(mapTile.rotation)
            );

            // We need one entity per sub mesh sadly...
            Mesh unityMesh = mapData.RenderMeshes[mapTile.meshID];

            // Create the root tile entity
            Entity tileRootEntity = entityManager.CreateEntity(ArchetypeTileRoot);
            entityManager.SetComponentData(tileRootEntity, tileMeshTransform);

            // Entity linking group allows us to tie the root to the meshes contained
            entityManager.GetBuffer<LinkedEntityGroup>(tileRootEntity).Add(tileRootEntity);

            // Now we may create each submesh for the tile
            for (int j = 0; j < unityMesh.subMeshCount; ++j)
            {
                Entity meshEntity = entityManager.CreateEntity(ArchetypeTileMesh);

                // Link the hierarchy
                entityManager.SetComponentData(meshEntity, new Parent { Value = tileRootEntity });
                entityManager.SetComponentData(meshEntity, LocalTransform.Identity);

                // Setup rendering components 
                entityManager.SetComponentData(meshEntity, new RenderBounds { Value = unityMesh.GetSubMesh(j).bounds.ToAABB() });
                entityManager.SetSharedComponentManaged(meshEntity, tileMeshData);
                entityManager.SetSharedComponent(meshEntity, tileEntityRenderFilterSettings);
                entityManager.SetComponentData(meshEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(mapTile.materialIDs[j], mapTile.meshID, (ushort)j));

                // Setup collision components 
                if (mapTile.colliderID >= 0)
                    entityManager.SetComponentData(meshEntity, new PhysicsCollider { Value = mapData.CollisionMeshes[mapTile.colliderID] });

                // Add the child to the root's LinkedEntityGroup so it gets culled together
                entityManager.GetBuffer<LinkedEntityGroup>(tileRootEntity).Add(meshEntity);
            }
        }
    }

    /// <summary>
    /// Sets up the objects
    /// </summary>
    void SetupMapObjects()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        for (int i = 0; i < mapData.WorldObjects.Length; ++i)
        {
            // Get an object from our world object data
            MPXObject mpxObject = mapData.WorldObjects[i];

            // We want to avoid invalid declarations...
            if (mpxObject.declarationID == -1)
                continue;

            SpawnObject(entityManager, mpxObject, i);
        }
    }

    /// <summary>
    /// Spawns a single map object
    /// </summary>
    void SpawnObject(EntityManager entityManager, MPXObject mpxObject, int index)
    {
        // Get object data
        if (!GameManager.Instance.ObjectData.GetObjectData(mpxObject.declarationID, out SoMObjectProfile profile, out SoMObjectParameter parameter, out ModelResource model))
            return;

        // Set up mesh data for the object
        Mesh mesh = model.Get();
        RenderMeshArray renderMeshArray = new RenderMeshArray(model.Materials, new Mesh[] { mesh });

        // Build the object transform
        LocalTransform localTransform = LocalTransform.FromPositionRotationScale(
            mpxObject.position, 
            quaternion.Euler(-mpxObject.rotation, math.RotationOrder.ZXY),
            parameter.scale * mpxObject.scale
        );

        // Create root entity
        Entity objectRootEntity = entityManager.CreateEntity(ArchetypeObjectRoot);
        entityManager.SetComponentData(objectRootEntity, localTransform);
        entityManager.GetBuffer<LinkedEntityGroup>(objectRootEntity).Add(objectRootEntity);

        // Store runtime object data on the root entity
        entityManager.SetComponentData(objectRootEntity,
            new RuntimeMapObject
            {
                // The visible flag is set by the user and must be accounted for
                Visible = mpxObject.visible == 1
            });

        // Depending on the object type, additional entities may be required.
        switch (profile.objectClass)
        {
            case SoMObjectClass.Light:
                // Don't bother with range 0 lights, I'm not convinced they do anything...
                if (mpxObject.flags.lightFlags.range <= 0)
                    break;

                // We must create a game object for the light
                GameObject lightObject = new GameObject($"OBJ {index:D4} LIGHT");
                Light light = lightObject.AddComponent<Light>();

                // position itself needs setting...
                float3 controlPointPosition = (model.ControlPoints[0] * localTransform.Scale);
                lightObject.transform.position = localTransform.Position + controlPointPosition;

                // Configure as a Point Light
                light.type = LightType.Point;
                light.color = new Color32(
                    (byte)((mpxObject.flags.lightFlags.colour >> 00) & 0xFF),
                    (byte)((mpxObject.flags.lightFlags.colour >> 08) & 0xFF),
                    (byte)((mpxObject.flags.lightFlags.colour >> 16) & 0xFF),
                    255
                    );

                light.range   = (2F * mpxObject.flags.lightFlags.range);
                light.shadows = LightShadows.None;

                // Create child entity for light syncing
                Entity lightEntity = entityManager.CreateEntity(ArchetypeChild);

                entityManager.AddComponentObject(lightEntity, light);
                entityManager.SetComponentData(lightEntity, LocalTransform.FromPosition(controlPointPosition));
                entityManager.SetComponentData(lightEntity, new Parent { Value = objectRootEntity });

                // Add the child to the root's LinkedEntityGroup so it gets culled together
                entityManager.GetBuffer<LinkedEntityGroup>(objectRootEntity).Add(lightEntity);
                break;
        }

        // We now must create an entity for each sub mesh...
        for (int i = 0; i < mesh.subMeshCount; ++i)
        {
            Entity meshEntity = entityManager.CreateEntity(ArchetypeObjectMesh);

            // Link the hierarchy
            entityManager.SetComponentData(meshEntity, new Parent { Value = objectRootEntity });
            entityManager.SetComponentData(meshEntity, LocalTransform.Identity);

            // Setup rendering components
            entityManager.SetComponentData(meshEntity, new RenderBounds { Value = mesh.GetSubMesh(i).bounds.ToAABB() });
            entityManager.SetSharedComponentManaged(meshEntity, renderMeshArray);
            entityManager.SetSharedComponent(meshEntity, ObjectRenderSettings);
            entityManager.SetComponentData(meshEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(model.MeshMaterialMapping[i], 0, (ushort)i));

            // Add the child to the root's LinkedEntityGroup so it gets culled together
            entityManager.GetBuffer<LinkedEntityGroup>(objectRootEntity).Add(meshEntity);
        }
    }

    /// <summary>
    /// Sets up map music
    /// </summary>
    void SetupMapMusic() =>
        MusicManager.Instance.Play(mapData.MusicFileName, true);
}
