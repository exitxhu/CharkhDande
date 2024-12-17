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
        Configurator.RegisterActions();

    }


    [Fact]
    public void ConditionRegistryTest()
    {
        Configurator.RegisterConditions();
    }

}
