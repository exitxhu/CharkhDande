using CharkhDande.Core;
using CharkhDande.Kesho;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace CharkhDande.Tests;

public class WorkflowSerializationTests
{
    [Fact]
    public async Task DeserializingWorkFlow()
    {
        var json = @"{
  ""id"": ""4e3e4bb6-c452-4089-881c-10130be98ad5"",
  ""Steps"": [
    {
      ""Id"": ""first"",
      ""State"": ""WAITING"",
      ""Routes"": [
        {
          ""Id"": ""GoTask2R"",
          ""Action"": [],
          ""Condition"": [],
          ""NextStepId"": {
            ""id"": ""second""
          }
        }
      ],
      ""Type"": ""ConditionalStep"",
      ""MetaData"": {
        ""Consitions#1"": {
          ""Key"": ""docIdEven""
        },
        ""Actions#1"": {
          ""Key"": ""sendEmailOnCondition""
        },
        ""Actions#2"": {
          ""Key"": ""jobTriggerKey""
        },
        ""Actions#3"": {
          ""Key"": ""dispatchEventKey""
        }
      }
    },
    {
      ""Id"": ""second"",
      ""State"": ""WAITING"",
      ""Routes"": [
        {
          ""Id"": ""GoTask3R"",
          ""Action"": [],
          ""Condition"": [
            {
              ""Key"": ""docIdEven""
            }
          ],
          ""NextStepId"": {
            ""id"": ""third""
          }
        }
      ],
      ""Type"": ""ConditionalStep"",
      ""MetaData"": {
        ""Actions#1"": {
          ""Key"": ""taskRun""
        }
      }
    },
    {
      ""Id"": ""third"",
      ""State"": ""WAITING"",
      ""Routes"": [],
      ""Type"": ""ConditionalStep"",
      ""MetaData"": {
        ""Actions#1"": {
          ""Key"": ""taskRun""
        }
      }
    },
    {
      ""Id"": ""monitor 1"",
      ""State"": ""FINISHED"",
      ""Routes"": [
        {
          ""Id"": ""GoTask1R"",
          ""Action"": [],
          ""Condition"": [],
          ""NextStepId"": {
            ""id"": ""eventStep""
          }
        }
      ],
      ""Type"": ""MonitorStep"",
      ""MetaData"": {
        ""OnSuccessActions#1"": {
          ""Key"": ""jobAction""
        },
        ""OnTimeoutActions#1"": {
          ""Key"": ""jobTimeOutAction""
        },
        ""Timeout#"": ""00:00:10"",
        ""PollingInterval#"": ""00:00:01""
      }
    },
    {
      ""Id"": ""eventStep"",
      ""State"": ""RUNNING"",
      ""Routes"": [],
      ""Type"": ""CharkhDande.Kesho.EventListenerStep"",
      ""MetaData"": {
        ""EventKey#"": ""accept_topic""
      }
    }
  ],
  ""Context"": {
    ""Properties"": {
      ""wfid"": ""4e3e4bb6-c452-4089-881c-10130be98ad5"",
      ""jobCompleted"": true,
      ""X"": false,
      ""Y"": true,
      ""Z"": true,
      ""age"": 10,
      ""doc_id"": 1,
      ""eventStep:accept_topic"": ""something""
    },
    ""History"": null
  }
}";
        
        var services = new ServiceCollection();
        
        var sp = services
            .AddCharkhDande(a => a.AddKesho(services))
            .AddSingleton<IWorkflowResolver, WFRepo>()
            .BuildServiceProvider();

        var wfactory = sp.GetRequiredService<WorkflowFactory>();

        var wf = wfactory.Reconstruct(json);

        wf.Should().NotBeNull();
    }
}