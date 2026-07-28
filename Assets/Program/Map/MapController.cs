using UnityEngine;
using UnityEngine.Rendering;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;

public class MapController : MonoBehaviour
{
    [Header("References (External)")]
    [SerializeField] SoMMapData mapData;

    [Header("References (Internal)")]
    [SerializeField] Light directionalLightA;
    [SerializeField] Light directionalLightB;
    [SerializeField] Light directionalLightC;
    [SerializeField] BaseSky skyObject;

    [Header("Configuration")]
    [SerializeField] bool enableDirectionalLightShadows;

    // Properties
    public bool IsMapExited { get; private set; } = true;

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

        // Any data from the save file to override the loaded map with?..

        // Setup Segment
        SetupMapEnviroment();

        SetupMapPlayer();
        SetupMapCamera();

        SetupMapTiles();
        SetupMapObjects();

        SetupMapMusic();

        // After all of this nonsense, purge the resource manager to free now unused assets...
        ResourceManager.Purge();
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
            // ^ Slightly off. I can't get an exact match to what is baked (ambient is seemingly always too strong)
            target.color = colour;

            // Shadow settings
            target.shadows = enableDirectionalLightShadows switch
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

        // Create map tile archetype
        EntityArchetype tileEntityArchetype = entityManager.CreateArchetype(
            typeof(PerInstanceCullingTag),
            typeof(WorldToLocal_Tag),
            typeof(DepthSorted_Tag),
            typeof(Static),

            typeof(LocalToWorld),
            typeof(RenderBounds),
            typeof(RenderMeshArray),
            typeof(RenderFilterSettings),
            typeof(MaterialMeshInfo),

            typeof(WorldRenderBounds),

            typeof(PhysicsCollider),
            typeof(PhysicsWorldIndex)
        );

        // Mesh filter settings will stay the same for each tile, so set them up now
        RenderFilterSettings tileEntityRenderFilterSettings = new RenderFilterSettings
        {
            MotionMode         = MotionVectorGenerationMode.ForceNoMotion,
            Layer              = 0,
            ReceiveShadows     = enableDirectionalLightShadows,
            ShadowCastingMode  = enableDirectionalLightShadows ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off,
            RenderingLayerMask = 1,
            StaticShadowCaster = false
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

            float4x4 transformMatrix = tileMeshTransform.ToMatrix();

            // We need one entity per sub mesh sadly...
            Mesh unityMesh = mapData.RenderMeshes[mapTile.meshID];
            
            for (int j = 0; j < unityMesh.subMeshCount; ++j)
            {
                // Create Entity...
                Entity tileEntity = entityManager.CreateEntity(tileEntityArchetype);

                entityManager.SetComponentData(tileEntity, new LocalToWorld { Value = transformMatrix });         
                entityManager.SetComponentData(tileEntity, new RenderBounds { Value = unityMesh.GetSubMesh(j).bounds.ToAABB() });
                entityManager.SetSharedComponentManaged(tileEntity, tileMeshData);
                entityManager.SetSharedComponent(tileEntity, tileEntityRenderFilterSettings);
                entityManager.SetComponentData(tileEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(mapTile.materialIDs[j], mapTile.meshID, (ushort)j));
                
                if (mapTile.colliderID >= 0)
                    entityManager.SetComponentData(tileEntity, new PhysicsCollider { Value = mapData.CollisionMeshes[mapTile.colliderID] });
            }
        }
    }

    /// <summary>
    /// Sets up the objects
    /// </summary>
    void SetupMapObjects()
    {
        // Get the entity manager
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Create the object root archetype
        EntityArchetype objectRootArchetype = entityManager.CreateArchetype(
            typeof(LocalTransform),
            typeof(LocalToWorld),
            typeof(LinkedEntityGroup),
            typeof(RuntimeMapObject)
            );

        // Create the object mesh archetype
        EntityArchetype objectMeshArchetype = entityManager.CreateArchetype(
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

        // Mesh filter settings will stay the same for each tile, so set them up now
        RenderFilterSettings objectRenderFilterSettings = new RenderFilterSettings
        {
            MotionMode          = MotionVectorGenerationMode.ForceNoMotion,
            Layer               = 0,
            ReceiveShadows      = enableDirectionalLightShadows,
            ShadowCastingMode   = enableDirectionalLightShadows ? ShadowCastingMode.On : ShadowCastingMode.Off,
            RenderingLayerMask  = 1,
            StaticShadowCaster  = false
        };

        // Spawn entities for each object
        for (int i = 0; i < mapData.WorldObjects.Length; ++i)
        {
            // Get an object from our world object data
            SoMMapData.MPXObject worldObject = mapData.WorldObjects[i];

            // Skip empty delcarations
            if (worldObject.declarationID == -1)
                continue;

            // We can now get object data from the registry, and it's model data.
            if (!GameManager.Instance.ObjectData.GetObjectData(worldObject.declarationID, out SoMObjectProfile objProf, out SoMObjectParameter objParam, out ModelResource objModel))
                continue;

            // We must now get the mesh from our model resource
            Mesh unityMesh = objModel.Get();

            // We can now construct the render mesh array and object initial transform
            RenderMeshArray objRenderMeshArray = new RenderMeshArray(objModel.Materials, new Mesh[] { unityMesh });
            LocalTransform objLocalTransform   = LocalTransform.FromPositionRotationScale(
                new float3(worldObject.position.x, worldObject.position.y, worldObject.position.z),
                quaternion.Euler(new float3(-worldObject.rotation.x, -worldObject.rotation.y, -worldObject.rotation.z), math.RotationOrder.ZXY),
                objParam.scale * worldObject.scale
            );

            // We must now create the root entity that we will store our meshes
            Entity objectRootEntity = entityManager.CreateEntity(objectRootArchetype);
            entityManager.SetComponentData(objectRootEntity, objLocalTransform);

            // Entity linking group allows us to tie the root to the meshes contained
            entityManager.GetBuffer<LinkedEntityGroup>(objectRootEntity).Add(objectRootEntity);

            // Now we may create each submesh for the object...
            for (int j = 0; j < unityMesh.subMeshCount; ++j)
            {
                Entity meshEntity = entityManager.CreateEntity(objectMeshArchetype);

                // Link the hierarchy
                entityManager.SetComponentData(meshEntity, new Parent { Value = objectRootEntity });
                entityManager.SetComponentData(meshEntity, LocalTransform.Identity);

                // Setup rendering components
                entityManager.SetComponentData(meshEntity, new RenderBounds { Value = unityMesh.GetSubMesh(j).bounds.ToAABB() });
                entityManager.SetSharedComponentManaged(meshEntity, objRenderMeshArray);
                entityManager.SetSharedComponent(meshEntity, objectRenderFilterSettings);
                entityManager.SetComponentData(meshEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(objModel.MeshMaterialMapping[j], 0, (ushort)j));

                // Add the child to the root's LinkedEntityGroup so it gets culled together
                entityManager.GetBuffer<LinkedEntityGroup>(objectRootEntity).Add(meshEntity);
            }

            // We finally set our object data on the root
            entityManager.SetComponentData(objectRootEntity,
                new RuntimeMapObject
                {
                    // We must calculate in a special cull value which includes the AABB
                    CullDistanceSq = (mapData.CameraZFar * mapData.CameraZFar) + Vector3.SqrMagnitude(unityMesh.bounds.center - unityMesh.bounds.max),
                    
                    // The visible flag is set by the user and must be accounted for
                    Visible        = worldObject.visible == 1
                });
        }
    }

    /// <summary>
    /// Sets up map music
    /// </summary>
    void SetupMapMusic() =>
        MusicManager.Instance.Play(mapData.MusicFileName, true);

    /// <summary>
    /// Call to exit the map.
    /// </summary>
    public void ExitMap()
    {
        IsMapExited = true;
    }
}
