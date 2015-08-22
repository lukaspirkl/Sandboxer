using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandboxer.Tests
{
    public class SandboxHostTests
    {
        ISandboxHost sut;
        AssemblyCreator assemblyCreator = new AssemblyCreator();

        [Test]
        public void OnlySandboxeeAssembliesShouldBeFound()
        {
            sut.AvailableSandboxees.Count().ShouldBe(1);
            sut.AvailableSandboxees.First().Name.ShouldBe("My Plugin");
        }

        [Test]
        public void SandboxeeAssembliesShouldNotBeLoadedToThisAppDomain()
        {
            AppDomain.CurrentDomain.GetAssemblies().ShouldNotContain(x => x.GetCustomAttribute(typeof(SandboxeeAttribute)) != null);
        }

        [Test]
        public void LoadedEventIsRaisedWhenSandboxeeIsAdded()
        {
            var loadedPluginName = string.Empty;
            sut.Loaded += (s, a) =>
            {
                loadedPluginName = a.SandboxeeInfo.Name;
            };

            new AssemblyCreator().CreateAssembly("NewPlugin.dll", @"

            using System.Reflection;
            using Sandboxer;

            [assembly: AssemblyTitle(""New Plugin"")]
            [assembly: Sandboxee]

            ");

            Thread.Sleep(TimeSpan.FromSeconds(1));

            loadedPluginName.ShouldBe("New Plugin");
        }

        [Test]
        public void UnloadEventIsRaisedWhenSandboxeeIsRemoved()
        {
            var unloadedPluginName = string.Empty;
            sut.Unloaded += (s, a) =>
            {
                unloadedPluginName = a.SandboxeeInfo.Name;
            };

            File.Delete(Path.Combine(assemblyCreator.PluginsDir, "MyPlugin.dll"));

            Thread.Sleep(TimeSpan.FromSeconds(1));

            unloadedPluginName.ShouldBe("My Plugin");
        }

        [SetUp]
        public void SetUp()
        {
            assemblyCreator.CreateAssembly("MyPlugin.dll", @"

            using System.Reflection;
            using Sandboxer;

            [assembly: AssemblyTitle(""My Plugin"")]
            [assembly: Sandboxee]

            namespace MyPlugin
            {
                public class Initializer : ISandboxeeInitializer
                {
                    public void Initialize()
                    {
                    }
                }
            }

            ");

            assemblyCreator.CreateAssembly("NoPlugin.dll", @"

            using System.Reflection;

            [assembly: AssemblyTitle(""Some common assembly"")]

            namespace NoPlugin
            {
                public class SomeUsualClass
                {
                }
            }

            ");

            var factory = new SandboxHostFactory()
            {
                PluginPath = assemblyCreator.PluginsDir
            };

            sut = factory.Create();
        }

        [TearDown]
        public void TearDown()
        {
            if (sut != null)
            {
                sut.Dispose();
            }

            if (Directory.Exists(assemblyCreator.PluginsDir))
            {
                Directory.Delete(assemblyCreator.PluginsDir, true);
            }
        }
    }
}
