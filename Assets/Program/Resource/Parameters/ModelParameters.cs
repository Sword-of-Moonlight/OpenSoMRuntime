using UnityEngine;

public class ModelParameters : ResourceParameters
{
    /// <summary>
    /// Default Model Parameters as ResourceParameters type
    /// </summary>
    public static readonly ResourceParameters Default = new ModelParameters();

    /// <summary>
    /// The type of model to create (static or animated)
    /// </summary>
    public ModelParameterType ModelType { get; set; } = ModelParameterType.Static;

    /// <summary>
    /// If material creation should be done automatically
    /// </summary>
    public bool CreateDefaultMaterials  { get; set; } = false;

    /// <summary>
    /// Root path to load texture data from
    /// </summary>
    public string TextureRootPath { get; set; } = string.Empty;
}

public enum ModelParameterType
{
    Static  = 0,
    Animated = 1
}
