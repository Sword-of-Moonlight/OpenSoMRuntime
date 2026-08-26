using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

using UnityEngine;

public class AudioFactory : ResourceFactory<AudioResource>
{
    /// <inheritdoc/>
    public AudioFactory() : base()
    {
        RegisterFormatHandler(new WAVFormatHandler());
        RegisterFormatHandler(new SNDFormatHandler());
    }

    public override ulong Load(string path, ResourceParameters parameters = null)
    {
        // Calculate our hash name
        ulong name = HashThis.StringTo64(path);

        // Does this resource already exist in the cache? If so, return it without doing anything else.
        if (resourceCache.ContainsKey(name))
        {
            if (resourceCache[name].ResourceState != ResourceState.Unloaded)
                return name;
            else
                resourceCache.Remove(name, out _);
        }          

        // Lets create our absolute path now
        if (!File.Exists(path))
            throw new Exception($"Failed to import '{path}'!\nFile does not exist");

        // Try to find a loader for this resource
        FormatHandler<AudioResource> handler = GetFormatHandler(Path.GetExtension(path)) ?? throw new Exception($"Couldn't find format handler for '{path}'!");

        // Create the stream we will use to load the file
        using FileInputStream finStream = new(path);

        // Now we can validate it
        if (!handler.Validate(finStream))
            throw new Exception($"Failed to import '{path}' using handler '{handler.Metadata.name}'!");

        // Create the new resource 
        AudioResource resource = new()
        {
            ResourceState  = ResourceState.Unloaded,
            ResourceOrigin = path,
            ReferenceCount = 0
        };

        if (!handler.Load(finStream, resource))
            throw new Exception($"Failed to import '{path}' using handler '{handler.Metadata.name}'!\nUnknown error.");

        // Store the resource in our cache
        resourceCache[name] = resource;

        return name;
    }

    public override void Purge()
    {
        // Get the list of resources to purge...
        List<ulong> resourcesToPurge =
             resourceCache
            .Where((kvp => kvp.Value.ReferenceCount == 0 && kvp.Value.ResourceState == ResourceState.Unloaded))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (ulong resource in resourcesToPurge)
        {
            resourceCache[resource] = null;
            resourceCache.Remove(resource, out _);
        }     
    }
}