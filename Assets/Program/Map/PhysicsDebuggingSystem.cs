using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;

public partial struct PhysicsDebuggingSystem : ISystem
{
    /// <summary>
    /// ECS Implementation.<br/>
    /// </summary>
    public void OnCreate(ref SystemState state)
    {
        /*
        // Ensure no existing singleton exists
        if (!SystemAPI.HasSingleton<PhysicsDebugDisplayData>())
        {
            // CreateSingleton properly registers the archetype singleton for PhysicsDebugDisplaySystem
            state.EntityManager.CreateSingleton(new PhysicsDebugDisplayData
            {
                DrawColliders      = 1, // Draw Collider Geometry
                DrawColliderEdges  = 1, // Draw Collider Wireframe
                DrawMassProperties = 0
            });
        }
        */
    }
}
