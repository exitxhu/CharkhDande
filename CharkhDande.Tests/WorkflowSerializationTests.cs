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
  ""id"": ""5775f115-86bc-495e-890b-2fdccee536b9"",
  ""Steps"": [
    {
      ""Id"": ""first"",
      ""State"": ""WAITING"",
      ""Routes"": [
        {
          ""Id"": ""GoTask2R"",
          ""Actions"": [],
          ""Conditions"": [],
          ""Type"": ""ConditionalRoute"",
          ""NextStepId"": {
            ""id"": ""second""
          }
        }
      ],
      ""Type"": ""ConditionalStep"",
      ""IsFirstStep"": false,
      ""MetaData"": {
        ""Conditions#1"": {
          ""Value"": ""{\""Key\"":\""docIdEven\""}"",
          ""Type"": ""ConditionSerializableObject""
        },
        ""Actions#1"": {
          ""Value"": ""{\""Key\"":\""sendEmailOnCondition\""}"",
          ""Type"": ""ActionSerializableObject""
        },
        ""Actions#2"": {
          ""Value"": ""{\""Key\"":\""jobTriggerKey\""}"",
          ""Type"": ""ActionSerializableObject""
        },
        ""Actions#3"": {
          ""Value"": ""{\""Key\"":\""dispatchEventKey\""}"",
          ""Type"": ""ActionSerializableObject""
        }
      }
    },
    {
      ""Id"": ""second"",
      ""State"": ""WAITING"",
      ""Routes"": [
        {
          ""Id"": ""GoTask3R"",
          ""Actions"": [],
          ""Conditions"": [
            {
              ""Key"": ""docIdEven""
            }
          ],
          ""Type"": ""ConditionalRoute"",
          ""NextStepId"": {
            ""id"": ""third""
          }
        }
      ],
      ""Type"": ""ConditionalStep"",
      ""IsFirstStep"": false,
      ""MetaData"": {
        ""Actions#1"": {
          ""Value"": ""{\""Key\"":\""taskRun\""}"",
          ""Type"": ""ActionSerializableObject""
        }
      }
    },
    {
      ""Id"": ""third"",
      ""State"": ""WAITING"",
      ""Routes"": [],
      ""Type"": ""ConditionalStep"",
      ""IsFirstStep"": false,
      ""MetaData"": {
        ""Actions#1"": {
          ""Value"": ""{\""Key\"":\""taskRun\""}"",
          ""Type"": ""ActionSerializableObject""
        }
      }
    },
    {
      ""Id"": ""monitor 1"",
      ""State"": ""FINISHED"",
      ""Routes"": [
        {
          ""Id"": ""GoTask1R"",
          ""Actions"": [],
          ""Conditions"": [],
          ""Type"": ""ConditionalRoute"",
          ""NextStepId"": {
            ""id"": ""eventStep""
          }
        }
      ],
      ""Type"": ""MonitorStep"",
      ""IsFirstStep"": true,
      ""MetaData"": {
        ""OnSuccessActions#1"": {
          ""Value"": ""{\""Key\"":\""jobAction\""}"",
          ""Type"": ""ActionSerializableObject""
        },
        ""OnTimeoutActions#1"": {
          ""Value"": ""{\""Key\"":\""jobTimeOutAction\""}"",
          ""Type"": ""ActionSerializableObject""
        },
        ""Timeout#"": {
          ""Value"": ""00:00:10"",
          ""Type"": ""System.TimeSpan""
        },
        ""PollingInterval#"": {
          ""Value"": ""00:00:01"",
          ""Type"": ""System.TimeSpan""
        }
      }
    },
    {
      ""Id"": ""eventStep"",
      ""State"": ""RUNNING"",
      ""Routes"": [],
      ""Type"": ""CharkhDande.Kesho.EventListenerStep"",
      ""IsFirstStep"": false,
      ""MetaData"": {
        ""EventKey#"": {
          ""Value"": ""accept_topic"",
          ""Type"": ""System.String""
        }
      }
    }
  ],
  ""Context"": {
    ""Properties"": {
      ""wfid"": ""5775f115-86bc-495e-890b-2fdccee536b9"",
      ""jobCompleted"": true,
      ""X"": false,
      ""Y"": true,
      ""Z"": true,
      ""age"": 10,
      ""doc_id"": 1,
      ""eventStep:accept_topic"": ""something""
    },
    ""History"": null
  },
  ""CurrentStep"": {
    ""Id"": ""eventStep"",
    ""State"": ""RUNNING"",
    ""Routes"": [],
    ""Type"": ""CharkhDande.Kesho.EventListenerStep"",
    ""IsFirstStep"": false,
    ""MetaData"": {
      ""EventKey#"": {
        ""Value"": ""accept_topic"",
        ""Type"": ""System.String""
      }
    }
  }
}";

        var services = new ServiceCollection();

        var sp = services
            .AddCharkhDande(a => a.AddKesho(services))
            .AddSingleton<IWorkflowResolver, WFRepo>()
            .BuildServiceProvider();

        var wfactory = sp.GetRequiredService<WorkflowFactory>();

        var wf = wfactory.Reconstruct(json);

        var js = wf.ExportWorkFlow();

        wf.Should().NotBeNull();

        js.Should().Be(json);
    }
}