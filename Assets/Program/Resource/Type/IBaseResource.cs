using UnityEngine;

public interface IBaseResource
{
    ResourceState ResourceState { get; set; }

    string ResourceOrigin { get; set; }

    int ReferenceCount { get; set; }
}
