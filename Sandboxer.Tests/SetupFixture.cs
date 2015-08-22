using NUnit.Framework;

namespace Sandboxer.Tests
{
    [SetUpFixture]
    public class SetupFixture
    {
        [SetUp]
        public void Setup()
        {
            var creator = new AssemblyCreator();

            creator.CreateAssembly("MyPlugin.dll", @"

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

            creator.CreateAssembly("NoPlugin.dll", @"

            using System.Reflection;

            [assembly: AssemblyTitle(""Some common assembly"")]

            namespace NoPlugin
            {
                public class SomeUsualClass
                {
                }
            }

            ");
        }


    }
}