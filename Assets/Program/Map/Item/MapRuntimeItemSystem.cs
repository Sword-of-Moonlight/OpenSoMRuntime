using UnityEngine;
using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct MapRuntimeItemSystem : ISystem
{
    /// <summary>
    /// ECS Implementation.<br/>
    /// Denies running the system if entities of MapRuntimeItem type do not exist.
    /// </summary>
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MapRuntimeItem>();
    }

    /// <summary>
    /// ECS Implementation.<br/>
    /// Handles capturing and updating object state
    /// </summary>
    public void OnUpdate(ref SystemState state)
    {

    }
}
