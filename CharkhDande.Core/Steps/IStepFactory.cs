using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CharkhDande.Core.Steps;

public interface IStepFactory
{
    void AddDeserializer<T>(IStepDeserializer<T> parser) where T : IStep;
    IStep Deserialize(StepSerializeObject obj);
    IStepDeserializer<T> GetDeserializer<T>(string type) where T : IStep;
}
public interface IStepDeserializer<T> where T : IStep
{
    T Deserialize(StepSerializeObject obj);
}
public class StepFactory : IStepFactory
{
    private readonly Dictionary<string, Type> _typeMappings = new();
    private readonly Dictionary<string, object> _deserializer = new();
    private readonly Dictionary<Type, MethodInfo> _cachedDeserializerMethods = new();

    private MethodInfo GetCachedDeserializerMethod(Type type)
    {
        if (!_cachedDeserializerMethods.TryGetValue(type, out var method))
        {
            method = typeof(StepFactory)
                .GetMethod(nameof(GetDeserializer), BindingFlags.Public | BindingFlags.Instance)!
                .MakeGenericMethod(type);
            _cachedDeserializerMethods[type] = method;
        }
        return method;
    }
    public void AddDeserializer<T>(IStepDeserializer<T> deserializer) where T : IStep
    {
        var type = typeof(T).FullName;
        _deserializer[type] = deserializer;
        _typeMappings[type] = typeof(T);
    }

    public IStepDeserializer<T> GetDeserializer<T>(string type) where T : IStep
    {
        if (_deserializer.TryGetValue(type, out var parser) && parser is IStepDeserializer<T> typedDeserializer)
        {
            return typedDeserializer;
        }

        throw new KeyNotFoundException($"Parser for type '{type}' not found.");
    }

    public IStep Deserialize(StepSerializeObject obj)
    {
        if (_typeMappings.TryGetValue(obj.Type, out var stepType))
        {
            var method = GetCachedDeserializerMethod(stepType);

            var res = method.Invoke(this, [obj.Type]);

            var desMeth = res.GetType()
                  .GetMethod(nameof(IStepDeserializer<IStep>.Deserialize), BindingFlags.Public | BindingFlags.Instance);

            var step = desMeth.Invoke(res, [obj]);

            if (step is IStep sss)
            {
                return sss;
            }
        }

        throw new KeyNotFoundException($"No deserializer found for type '{obj.Type}'.");
    }
}
