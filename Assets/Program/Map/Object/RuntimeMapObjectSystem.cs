using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Rendering;

[BurstCompile]
public partial struct RuntimeMapObjectSystem : ISystem
{
    // Data
    float3 lastCameraPositionOnUpdate;

    /// <summary>
    /// ECS Implementation.<br/>
    /// Denies running the system if entities of RuntimeMapObject type do not exist.
    /// </summary>
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RuntimeMapObject>();
    }

    /// <summary>
    /// ECS Implementation.<br/>
    /// Handles capturing and updating object state
    /// </summary>
    public void OnUpdate(ref SystemState state)
    {
        // Grab the entity manager
        EntityManager entityManager = state.EntityManager;

        // We want to use an entity command buffer to handle commands, as we cannot modify entities we are iterating over
        EntityCommandBuffer commandBuffer = new(Allocator.Temp);

        foreach (var (mapObject, entity) in SystemAPI.Query<RefRO<RuntimeMapObject>>().WithEntityAccess())
        {
            // Grab the root linked group
            DynamicBuffer<LinkedEntityGroup> objectGroup = entityManager.GetBuffer<LinkedEntityGroup>(entity);

            // Update visibility state
            SetObjectVisible(entityManager, commandBuffer, objectGroup, mapObject.ValueRO.Visible);
        }

        commandBuffer.Playback(entityManager);
        commandBuffer.Dispose();
    }

    /// <summary>
    /// Sets entities children MaterialMeshIndexes visibility
    /// </summary>
    void SetObjectVisible(EntityManager em, EntityCommandBuffer ecb, DynamicBuffer<LinkedEntityGroup> rootObjectGroup, bool enabled)
    {
        // First child is always the parent. We can assume so, and skip it.
        // ^ No we fucking can't, dumb cunt? What about LIGHTs, FX etc... UGH
        for (int i = 0; i < rootObjectGroup.Length; ++i)
        {
            if (em.HasComponent<MaterialMeshInfo>(rootObjectGroup[i].Value))
                ecb.SetComponentEnabled<MaterialMeshInfo>(rootObjectGroup[i].Value, enabled);
        }       
    }
}
