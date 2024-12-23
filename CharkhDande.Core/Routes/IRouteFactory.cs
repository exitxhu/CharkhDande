using CharkhDande.Core.Steps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CharkhDande.Core.Routes;

public interface IRouteFactory
{
    void AddDeserializer<T>(IRouteDeserializer<T> parser) where T : IRoute;
    IRoute Deserialize(RouteSerializableObject obj);
    IRouteDeserializer<T> GetDeserializer<T>(string type) where T : IRoute;
}

public interface IRouteDeserializer<T> where T : IRoute
{
    T Deserialize(RouteSerializableObject obj);
}

public class RouteFactory : IRouteFactory
{
    private readonly Dictionary<string, Type> _typeMappings = new();
    private readonly Dictionary<string, object> _deserializer = new();
    private readonly Dictionary<Type, MethodInfo> _cachedDeserializerMethods = new();

    private MethodInfo GetCachedDeserializerMethod(Type type)
    {
        if (!_cachedDeserializerMethods.TryGetValue(type, out var method))
        {
            method = typeof(RouteFactory)
                .GetMethod(nameof(GetDeserializer), BindingFlags.Public | BindingFlags.Instance)!
                .MakeGenericMethod(type);
            _cachedDeserializerMethods[type] = method;
        }
        return method;
    }
    public void AddDeserializer<T>(IRouteDeserializer<T> deserializer) where T : IRoute
    {
        var type = typeof(T).FullName;
        _deserializer[type] = deserializer;
        _typeMappings[type] = typeof(T);
    }

    public IRouteDeserializer<T> GetDeserializer<T>(string type) where T : IRoute
    {
        if (_deserializer.TryGetValue(type, out var parser) && parser is IRouteDeserializer<T> typedDeserializer)
        {
            return typedDeserializer;
        }

        throw new KeyNotFoundException($"Parser for type '{type}' not found.");
    }

    public IRoute Deserialize(RouteSerializableObject obj)
    {
        if (_typeMappings.TryGetValue(obj.Type, out var stepType))
        {
            var method = GetCachedDeserializerMethod(stepType);

            var res = method.Invoke(this, [obj.Type]);

            var desMeth = res.GetType()
                  .GetMethod(nameof(IRouteDeserializer<IRoute>.Deserialize), BindingFlags.Public | BindingFlags.Instance);

            var step = desMeth.Invoke(res, [obj]);

            if (step is IRoute sss)
            {
                return sss;
            }
        }

        throw new KeyNotFoundException($"No deserializer found for type '{obj.Type}'.");
    }
}
