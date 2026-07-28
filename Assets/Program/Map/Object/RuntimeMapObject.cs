using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public struct RuntimeMapObject : IComponentData
{
    /// <summary>The maximum distance the entity should be active at</summary>
    public float CullDistanceSq;

    /// <summary>If the entity should be visible</summary>
    public bool Visible;
}
