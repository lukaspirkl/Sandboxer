using NUnit.Framework;
using Sandboxer.Tests.Interfaces;
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
            var finished = new ManualResetEvent(false);
            var loadedPluginName = string.Empty;
            sut.Loaded += (s, a) =>
            {
                loadedPluginName = a.SandboxeeInfo.Name;
                finished.Set();
            };

            new AssemblyCreator().CreateAssembly("NewPlugin.dll", @"

            using System.Reflection;
            using Sandboxer;

            [assembly: AssemblyTitle(""New Plugin"")]
            [assembly: Sandboxee]

            ");

            finished.WaitOne(TimeSpan.FromSeconds(1)).ShouldBe(true);

            loadedPluginName.ShouldBe("New Plugin");
        }

        [Test]
        public void UnloadEventIsRaisedWhenSandboxeeIsRemoved()
        {
            var finished = new ManualResetEvent(false);
            var unloadedPluginName = string.Empty;
            sut.Unloaded += (s, a) =>
            {
                unloadedPluginName = a.SandboxeeInfo.Name;
                finished.Set();
            };

            File.Delete(Path.Combine(assemblyCreator.PluginsDir, "MyPlugin.dll"));

            finished.WaitOne(TimeSpan.FromSeconds(1)).ShouldBe(true);

            unloadedPluginName.ShouldBe("My Plugin");
        }

        [Test]
        public void CallMethodImplementedInSandboxee()
        {
            var generators = sut.GetInstances<IWordGenerator>();

            generators.Count().ShouldBe(1);
            generators.First().GenerateWord().ShouldBe("Awesomeville");
        }

        [SetUp]
        public void SetUp()
        {
            assemblyCreator.CreateAssembly("MyPlugin.dll", @"

            using System;
            using System.Reflection;
            using Sandboxer;
            using Sandboxer.Tests.Interfaces;

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

                public class WordGenerator : MarshalByRefObject, IWordGenerator
                {
                    public string GenerateWord()
                    {
                        return ""Awesomeville"";
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
