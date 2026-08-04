using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public struct RuntimeMapObject : IComponentData
{
    /// <summary>If the entity should be visible</summary>
    public bool Visible;
}
