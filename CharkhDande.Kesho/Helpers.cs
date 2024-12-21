using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharkhDande.Kesho;
 
public static class Helpers
{
    public static IServiceCollection AddKesho(this IServiceCollection services)
    {
        return services.AddSingleton<WorkflowRegistry>()
            .AddSingleton<KafkaEventSource>();
    }
}
