using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using System.IO;

/// <summary>
/// Generic implementation of a resource manager
/// </summary>
public static class ResourceManager
{
    // Factories
    static readonly TextureFactory textureFactory = new();
    static readonly AudioFactory audioFactory = new();
    static readonly ModelFactory modelFactory = new();

    static readonly Dictionary<Type, object> factories = new Dictionary<Type, object>
    {
        { typeof(TextureResource), textureFactory },
        { typeof(AudioResource),   audioFactory   },
        { typeof(ModelResource),   modelFactory   }
    };

    /// <summary>Stores a set of file sources, i.e. directories which can be looked at for resources</summary>
    public static List<string> ResourceRoots { get; private set; } = null;

    /// <summary>
    /// Initializes the resource manager
    /// </summary>
    public static void Initialize()
    {
        ResourceRoots = new List<string>();
    }

    /// <summary>
    /// Adds a new resource root to the resource manager.<br/>
    /// The <b>LAST</b> item added is considered the priority root.
    /// </summary>
    public static void AssignResourceRoot(string resourceRoot)
    {
        if (string.IsNullOrEmpty(resourceRoot))
            return;

        ResourceRoots.Add(resourceRoot);
    }

    public static bool Find(string find, out string foundPath)
    {
        for (int i = ResourceRoots.Count - 1; i >= 0; --i)
        {
            if (!File.Exists(Path.Combine(ResourceRoots[i], find)))
                continue;

            foundPath = Path.Combine(ResourceRoots[i], find);
            return true;
        }

        Logger.Custom("RESR", 0x8040F0, $"Failed to find file: '{find}'");

        foundPath = null;
        return false;
    }

    public static List<FileInfo> Enumerate(string searchPattern)
    {
        List<FileInfo> fileList = new List<FileInfo>();

        for (int i = 0; i < ResourceRoots.Count; ++i)
            fileList.AddRange(new DirectoryInfo(ResourceRoots[i]).EnumerateFiles(searchPattern, SearchOption.AllDirectories));

        return fileList;
    }



    /// <summary>
    /// Purges any dead assets from each of the factories
    /// </summary>
    public static void Purge()
    {
        textureFactory.Purge();
        audioFactory.Purge();
        modelFactory.Purge();
    }

    /// <summary>
    /// Dumps information about the resource manager
    /// </summary>
    public static void Dump()
    {
        using StringWriter sw = new StringWriter();

        sw.WriteLine("ResourceManager::Dump {");

        // Roots
        sw.WriteLine($"\tRoots = {{");
        foreach (string resourceRoot in ResourceRoots)
            sw.WriteLine($"\t\t'{resourceRoot}',");
        sw.WriteLine($"\t}},");

        // Textures
        sw.WriteLine($"\tLoaded Texture Info = {{");
        sw.WriteLine($"\t\tCount = {textureFactory.CacheSize}");
        sw.WriteLine($"\t\tResources = {{");
        foreach (string resourceName in textureFactory.GetResourceNames())
            sw.WriteLine($"\t\t\t'{resourceName}',");
        sw.WriteLine($"\t\t}}");
        sw.WriteLine($"\t}},");

        // Models
        sw.WriteLine($"\tLoaded Model Info = {{");
        sw.WriteLine($"\t\tCount = {modelFactory.CacheSize}");
        sw.WriteLine($"\t\tResources ={{");
        foreach (string resourceName in modelFactory.GetResourceNames())
            sw.WriteLine($"\t\t\t'{resourceName}',");
        sw.WriteLine($"\t\t}}");
        sw.WriteLine($"\t}},");

        // Sounds
        sw.WriteLine($"\tLoaded Sound Info = {{");
        sw.WriteLine($"\t\tCount = {audioFactory.CacheSize}");
        sw.WriteLine($"\t\tResources = {{");
        foreach (string resourceName in audioFactory.GetResourceNames())
            sw.WriteLine($"\t\t\t'{resourceName}',");
        sw.WriteLine($"\t\t}}");
        sw.WriteLine($"\t}},");

        sw.WriteLine($"}}");

        Logger.Info(sw.ToString());
    }

    /// <summary>
    /// Load a file.
    /// </summary>
    /// <typeparam name="T">The resource type to load</typeparam>
    /// <param name="path">The relative path we expect to find the resource at</param>
    public static ulong Load<T>(string path, ResourceParameters parameters = null) where T : IBaseResource
    {
        if (factories.TryGetValue(typeof(T), out object factory))
            return ((ResourceFactory<T>)factory).Load(path, parameters);

        throw new Exception("Invalid resource type!");
    }

    /// <summary>
    /// Load a buffer.
    /// </summary>
    /// <typeparam name="T">The resource type to load</typeparam>
    /// <param name="path">The relative path we expect to find the resource at</param>
    public static ulong Load<T>(ResourceBlob blob, ResourceParameters parameters = null) where T : IBaseResource
    {
        if (factories.TryGetValue(typeof(T), out object factory))
            return ((ResourceFactory<T>)factory).Load(blob, parameters);

        throw new Exception("Invalid resource type!");
    }

    /// <summary>
    /// Load a file in async
    /// </summary>
    /// <typeparam name="T">The resource type to load</typeparam>
    /// <param name="path">The relative path we expect to find the resource at</param>
    public static async void LoadAsync<T>(string path, ResourceParameters parameters = null, Action<ulong> onComplete = null) where T : IBaseResource
    {
        Task<ulong> LoadingTask;

        // Do all our format wrangling on a background thread
        try
        {
            LoadingTask = new(() => Load<T>(path, parameters));
            LoadingTask.Start();

            await LoadingTask;
        } catch
        {
            throw;
        }

        // We can now execute the users provided callback
        onComplete?.Invoke(LoadingTask.Result);
    }

    /// <summary>
    /// Get a file
    /// </summary>
    /// <typeparam name="T">The resource type to load</typeparam>
    /// <param name="name">The returned name from loading a file</param>
    public static T Get<T>(ulong name) where T : IBaseResource
    {
        if (factories.TryGetValue(typeof(T), out object factory))
            return ((ResourceFactory<T>)factory).Get(name);

        throw new Exception("Invalid resource type!");
    }
}
