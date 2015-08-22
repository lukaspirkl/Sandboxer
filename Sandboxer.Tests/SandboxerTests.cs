using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sandboxer.Tests
{
    public class SandboxerTests
    {
        [Test]
        public void OnlySandboxeeAssembliesShouldBeFound()
        {
            var sut = CreateSut();
            sut.AvailablePlugins.Count().ShouldBe(1);
        }

        [Test]
        public void SandboxeeAssembliesShouldNotBeLoadedToThisAssembly()
        {
            CreateSut();
            AppDomain.CurrentDomain.GetAssemblies().ShouldNotContain(x => x.GetCustomAttribute(typeof(SandboxeeAttribute)) != null);
        }

        private ISandboxer CreateSut()
        {
            var creator = new AssemblyCreator();

            var factory = new SandboxerFactory()
            {
                PluginPath = creator.PluginsDir
            };

            return factory.Create();
        }
    }
}
