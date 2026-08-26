using Unity.Entities;

public struct MapRuntimeItem : IComponentData
{
    /// <summary>Reference id is used to link the entity to map data</summary>
    public ReferenceID refId;
}
