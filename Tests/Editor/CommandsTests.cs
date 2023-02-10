﻿using Commands;
using HECSFramework.Core;
using NUnit.Framework;
using Systems;

internal class CommandsTests
{
    [Test]
    public void TestLocalCommand()
    {
        EntityManager.RecreateInstance();

        var entity = new Entity("Check");
        var sys = new StressTestReactsSystem();
        entity.AddHecsSystem(sys);
        entity.Init();

        entity.Command(new StressTestLocalCommand { Param = true });
        entity.RemoveHecsSystem<StressTestReactsSystem>();
        entity.Command(new StressTestLocalCommand { Param = false });

        Assert.IsTrue(sys.LocalReact && sys.LocalReactRemoved);
    }


    [Test]
    public void TestGlobalCommand()
    {
        EntityManager.RecreateInstance();

        var entity = new Entity("Check");
        var sys = new StressTestReactsSystem();
        entity.AddHecsSystem(sys);
        entity.Init();

        EntityManager.Command(new StressTestGlobalCommand { Param = true });
        entity.RemoveHecsSystem<StressTestReactsSystem>();
        entity.Command(new StressTestGlobalCommand { Param = false });

        Assert.IsTrue(sys.GlobalReact && sys.GlobalReactRemoved);
    }
}
