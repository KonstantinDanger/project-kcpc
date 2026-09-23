using System;
using System.Collections.Generic;

public class ServiceLocator
{
    private static ServiceLocator _instance;
    public static ServiceLocator Container => _instance ??= new ServiceLocator();

    private readonly Dictionary<Type, object> _services = new();
    private readonly Dictionary<Type, object> _cachedServices = new();

    /// <summary>
    /// "cached" means that the service will be added to the cached map, that will be cleared on dispose
    /// </summary>
    public void RegisterSingle<T>(T service, bool cached = false)
    {
        Type type = typeof(T);

        if (cached)
        {
            if (_cachedServices.ContainsKey(type))
                throw new Exception($"The service with type \"{type.Name}\" already exists in cached service locator");

            _cachedServices[type] = service;
        }
        else
        {
            if (_services.ContainsKey(type))
                throw new Exception($"The service with type \"{type.Name}\" already exists in service locator");

            _services[type] = service;
        }
    }

    /// <summary>
    /// "resolveCached" means that the service will be resolved from cached map of services
    /// </summary>
    public T Resolve<T>(bool resolveCached = false)
    {
        Type type = typeof(T);

        if (resolveCached)
        {
            if (!_cachedServices.ContainsKey(type))
                throw new Exception($"The service with type \"{type.Name}\" does not exist in cached service locator, but you are trying to resolve it");

            return (T)_cachedServices[type];
        }

        if (!_services.ContainsKey(type))
            throw new Exception($"The service with type \"{type.Name}\" does not exist in service locator, but you are trying to resolve it");

        return (T)_services[type];
    }

    /// <summary>
    /// Clears cached services
    /// </summary>
    public void Dispose() => _cachedServices.Clear();

    public bool Unregister<T>(bool unregisterCached = false)
    {
        Type type = typeof(T);

        if (unregisterCached)
        {
            if (!_cachedServices.ContainsKey(type))
                return false;

            _cachedServices.Remove(type);
            return true;
        }
        else
        {
            if (!_services.ContainsKey(type))
                return false;

            _services.Remove(type);
            return true;
        }
    }
}
