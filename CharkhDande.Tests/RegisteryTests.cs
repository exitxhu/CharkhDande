using CharkhDande.Core.Actions;
using CharkhDande.Core.Conditions;

using FluentAssertions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharkhDande.Tests;

public class RegisteryTests
{
    [Fact]
    public void ActionRegistryTest()
    {
        var actionReg = new ActionRegistry();
        Configurator.RegisterActions(actionReg);

        var action = actionReg.Resolve(Configurator.ActionTrigger);

        action.Should().NotBeNull();
    }


    [Fact]
    public void ConditionRegistryTest()
    {
        var conditionReg = new ConditionRegistry();
        Configurator.RegisterConditions(conditionReg);

        var condition = conditionReg.Resolve(Configurator.ConditionTrue);

        var t = condition.Invoke(default, default);

        t.Should().Be(true);

    }

}
