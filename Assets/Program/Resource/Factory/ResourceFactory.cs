using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.IO;

/// <summary>
/// A resource factory is responsible for handling import, export and storage of resources in a specific format
/// </summary>
public abstract class ResourceFactory<T> where T : IBaseResource
{
    protected List<FormatHandler<T>> registeredFormatHandlers;
    protected ConcurrentDictionary<ulong, T> resourceCache;

    /// <summary>
    /// Returns the size of the resource cache
    /// </summary>
    public int CacheSize => resourceCache.Count;

    /// <summary>
    /// Default constructor.<br/>
    /// Will create default containers.
    /// </summary>
    protected ResourceFactory()
    {
        registeredFormatHandlers = new List<FormatHandler<T>>();
        resourceCache = new ConcurrentDictionary<ulong, T>();
    }

    /// <summary>
    /// Enumurates all registered format handlers which match the filter provided.
    /// </summary>
    /// <param name="filter">The filter to use.</param>
    /// <returns></returns>
    public List<FormatHandler<T>> EnumerateFormatHandlers(FormatFilter filter = FormatFilter.AllSafe)
    {
        // We accumulate our format handlers that match the filter in this list
        List<FormatHandler<T>> handlers = new();
            
        // Scan through our registered format handlers and find ones that match our filter...
        foreach(FormatHandler<T> handler in registeredFormatHandlers)
        {
            // Check import capabilities vs filter. Inclusive.
            if (!handler.Capabilities.allowImport && ((filter & FormatFilter.Importable) > 0))
                continue;

            // Check export capabilities vs filter. Inclusive.
            if (!handler.Capabilities.allowExport && ((filter & FormatFilter.Exportable) > 0))
                continue;

            // Check deprecated vs filter. Exclusive.
            if (handler.Capabilities.deprecated   && ((filter & FormatFilter.Deprecated) == 0))
                continue;

            // Check experimental vs filter. Exclusive.
            if (handler.Capabilities.experimental && ((filter & FormatFilter.Experimental) == 0))
                continue;

            // Now the handler has passed all checks, it can be appended to the list
            handlers.Add(handler);
        }

        return handlers;
    }

    /// <summary>
    /// Registers a format handler.
    /// </summary>
    /// <param name="formatHandler">The format handler to register</param>
    public void RegisterFormatHandler(FormatHandler<T> formatHandler) =>
        registeredFormatHandlers.Add(formatHandler);

    /// <summary>
    /// Tries to find a handler for a format with the extension given.
    /// </summary>
    /// <param name="extension">The common extension of the format e.g. '.tga', '.wav'</param>
    /// <returns>The handler if one can be found, or null if it can not.</returns>
    public FormatHandler<T> GetFormatHandler(string extension)
    {
        // Scan through our list of registered handlers looking for the extension
        foreach (FormatHandler<T> handler in registeredFormatHandlers)
        {
            foreach(string handlerExtension in handler.Metadata.extensions)
            {
                if (handlerExtension.Equals(extension, StringComparison.InvariantCultureIgnoreCase))
                    return handler;
            }
        }

        // Failed.
        return null;
    }

    /// <summary>
    /// Gets a resource from the resource cache
    /// </summary>
    /// <param name="name">The name of the resource</param>
    public T Get(ulong name) =>
        resourceCache[name];

    /// <summary>
    /// Loads a resource into the factory from the given path
    /// </summary>
    public virtual ulong Load(string path, ResourceParameters parameters = null) =>
        throw new NotImplementedException();

    /// <summary>
    /// Loads a resource into the factory from the given blob
    /// </summary>
    public virtual ulong Load(ResourceBlob blob, ResourceParameters parameters = null) =>
        throw new NotImplementedException();

    /// <summary>
    /// Purges the resource cache
    /// </summary>
    public virtual void Purge() =>
        throw new NotImplementedException();

    /// <summary>
    /// Gets a list of all resource origins
    /// </summary>
    public List<string> GetResourceNames()
    {
        List<string> resourceNames = new List<string>();

        foreach (ulong resourceKey in resourceCache.Keys)
            resourceNames.Add($"[{resourceCache[resourceKey].ResourceState}] {Path.GetFileName(resourceCache[resourceKey].ResourceOrigin)}");
            
        return resourceNames;
    }
}