using CharkhDande.Core;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharkhDande.Kesho;

public static class Helpers
{
    public static CharkhDandeConfig AddKesho(this CharkhDandeConfig config, IServiceCollection services)
    {
        config.Assemblies.Add(typeof(WorkflowRegistry).Assembly);

        services.AddSingleton<WorkflowRegistry>()
        .AddSingleton<KafkaEventSource>();

        return config;
    }
}
