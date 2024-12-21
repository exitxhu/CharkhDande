using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace CharkhDande.Tests;

public class WorkflowSerializationTests
{
    [Fact]
    public async Task DeserializingWorkFlow()
    {
        var json = @"{
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
        }
      }
    },
    {
      ""Id"": ""eventStep"",
      ""State"": ""RUNNING"",
      ""Routes"": [],
      ""Type"": ""EventListenerStep"",
      ""MetaData"": null
    }
  ],
  ""Context"": {
    ""Properties"": {
      ""wfid"": ""7856ce21-e8fe-490d-9285-f3a59eb79d94"",
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
        var sp = new ServiceCollection().BuildServiceProvider();

        var wfactory = new WorkflowFactory(sp);

        var wf = wfactory.Reconstruct(json);

        wf.Should().NotBeNull();
    }
}