using CharkhDande.Core.Actions;
using CharkhDande.Core.Conditions;
using CharkhDande.Core.Routes;
using CharkhDande.Core.Steps;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CharkhDande.Core;
public static class Helpers
{
    public static IServiceCollection AddCharkhDande(this IServiceCollection services, Action<CharkhDandeConfig> config)
    {
        var conf = new CharkhDandeConfig();
        config(conf);

        conf.Assemblies.Add(typeof(Workflow).Assembly);

        DiscoverStepDeserializers(services, conf);
        DiscoverRouteDeserializers(services, conf);

        return services
            .AddSingleton<WorkflowFactory>()
            .AddSingleton<IActionRegistry, ActionRegistry>()
            .AddSingleton<IConditionRegistry, ConditionRegistry>()
            ;
    }
    private static void DiscoverRouteDeserializers(IServiceCollection services, CharkhDandeConfig conf)
    {
        foreach (var assembly in conf.Assemblies)
        {
            var parserTypes = assembly.GetTypes()
                 .Where(type => !type.IsAbstract && !type.IsInterface) // Exclude abstract classes and interfaces
                 .SelectMany(type => type.GetInterfaces()
                  .Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IRouteDeserializer<>))
                   .Select(interfaceType => new { ParserType = type, InterfaceType = interfaceType }));

            foreach (var parserType in parserTypes)
            {
                services.AddTransient(parserType.InterfaceType, parserType.ParserType);
            }
        }
        services.AddSingleton<IRouteFactory>(provider =>
        {
            var factory = new RouteFactory();
            var serviceProvider = provider;

            // Find all registered IRouteDeserializer<T> implementations
            foreach (var serviceDescriptor in services.Where(sd =>
                sd.ServiceType.IsGenericType &&
                sd.ServiceType.GetGenericTypeDefinition() == typeof(IRouteDeserializer<>)))
            {
                var typeArgument = serviceDescriptor.ServiceType.GetGenericArguments()[0]; // Extract T from IRouteDeserializer<T>
                var deserializerType = typeof(IRouteDeserializer<>).MakeGenericType(typeArgument);
                var deserializerInstance = serviceProvider.GetRequiredService(deserializerType);

                // Use reflection to call AddDeserializer<T> on RouteFactory
                var addDeserializerMethod = factory.GetType()
                    .GetMethod(nameof(factory.AddDeserializer))!
                    .MakeGenericMethod(typeArgument);
                addDeserializerMethod.Invoke(factory, [deserializerInstance]);
            }

            return factory;
        });
    }

    private static void DiscoverStepDeserializers(IServiceCollection services, CharkhDandeConfig conf)
    {
        foreach (var assembly in conf.Assemblies)
        {
            var parserTypes = assembly.GetTypes()
                 .Where(type => !type.IsAbstract && !type.IsInterface) // Exclude abstract classes and interfaces
                 .SelectMany(type => type.GetInterfaces()
                  .Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IStepDeserializer<>))
                   .Select(interfaceType => new { ParserType = type, InterfaceType = interfaceType }));

            foreach (var parserType in parserTypes)
            {
                services.AddTransient(parserType.InterfaceType, parserType.ParserType);
            }
        }
        services.AddSingleton<IStepFactory>(provider =>
        {
            var factory = new StepFactory(provider.GetRequiredService<IRouteFactory>());
            var serviceProvider = provider;

            // Find all registered IStepDeserializer<T> implementations
            foreach (var serviceDescriptor in services.Where(sd =>
                sd.ServiceType.IsGenericType &&
                sd.ServiceType.GetGenericTypeDefinition() == typeof(IStepDeserializer<>)))
            {
                var typeArgument = serviceDescriptor.ServiceType.GetGenericArguments()[0]; // Extract T from IStepDeserializer<T>
                var deserializerType = typeof(IStepDeserializer<>).MakeGenericType(typeArgument);
                var deserializerInstance = serviceProvider.GetRequiredService(deserializerType);

                // Use reflection to call AddDeserializer<T> on StepFactory
                var addDeserializerMethod = factory.GetType()
                    .GetMethod(nameof(factory.AddDeserializer))!
                    .MakeGenericMethod(typeArgument);
                addDeserializerMethod.Invoke(factory, [deserializerInstance]);
            }

            return factory;
        });
    }

    public static CharkhDandeConfig AddWorkflowResolver<T>(this CharkhDandeConfig config, ServiceCollection services) where T : class, IWorkflowResolver
    {
        services.AddTransient<IWorkflowResolver, T>();
        return config;
    }
}

public class CharkhDandeConfig
{
    public List<Assembly> Assemblies { get; } = [];

}
