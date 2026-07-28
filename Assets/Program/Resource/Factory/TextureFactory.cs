using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

public class TextureFactory : ResourceFactory<TextureResource>
{
    /// <inheritdoc/>
    public TextureFactory() : base()
    {
        RegisterFormatHandler(new BMPFormatHandler());
        RegisterFormatHandler(new TXRFormatHandler());
    }

    public override ulong Load(string path, ResourceParameters parameters = null)
    {
        // Calculate our hash name
        ulong name = HashThis.StringTo64(path);

        // Does this resource already exist in the cache? If so, return it without doing anything else.
        if (resourceCache.ContainsKey(name) && resourceCache[name] != null)
            return name;

        // Lets create our absolute path now
        string absolutePath = Path.Combine(ResourceManager.ResourceRoot, path);

        if (!File.Exists(absolutePath))
            throw new Exception($"Failed to import '{path}' located '{absolutePath}'!\nFile does not exist");

        // Try to find a loader for this resource
        FormatHandler<TextureResource> handler = GetFormatHandler(Path.GetExtension(path)) ?? throw new Exception($"Couldn't find format handler for '{path}'!");

        // Create the stream we will use to load the file
        using (FileInputStream fis = new FileInputStream(path))
        {
            LoadInternal(fis, handler, name, absolutePath);
        }

        return name;
    }

    public override ulong Load(ResourceBlob blob, ResourceParameters parameters = null)
    {
        ulong name = HashThis.StringTo64(blob.VirtualOrigin);

        if (resourceCache.ContainsKey(name) && resourceCache[name] != null)
            return name;

        FormatHandler<TextureResource> handler = GetFormatHandler(Path.GetExtension(blob.VirtualOrigin)) ?? throw new Exception($"Couldn't find format handler for '{blob.VirtualOrigin}'!");

        using (FileInputStream fis = new FileInputStream(blob.Buffer))
        {
            LoadInternal(fis, handler, name, blob.VirtualOrigin);
        }

        return name;
    }

    void LoadInternal(FileInputStream fis, FormatHandler<TextureResource> handler, ulong name, string origin)
    {
        // Validate using the handler
        if (!handler.Validate(fis))
            throw new Exception($"Failed to import '{origin}' using handler '{handler.Metadata.name}'!");

        // Create the texture resource...
        TextureResource resource = new TextureResource
        {
            ResourceState  = ResourceState.Unloaded,
            ResourceOrigin = origin,
            ReferenceCount = 0
        };

        // Now the actual load
        if (!handler.Load(fis, resource))
            throw new Exception($"Failed to import '{origin}' using handler '{handler.Metadata.name}'!");

        // Store resource
        resourceCache[name] = resource;
    }

    public override void Purge()
    {
        // Get the list of resources to purge...
        List<ulong> resourcesToPurge =
             resourceCache
            .Where((kvp => kvp.Value.ReferenceCount <= 0 && kvp.Value.ResourceState == ResourceState.Unloaded))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (ulong resource in resourcesToPurge)
        {
            // Remove object from cache list...
            resourceCache[resource] = null;
            resourceCache.Remove(resource, out _);
        }
            
    }
}