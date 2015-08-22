using System;
using System.IO;
using System.Linq;
using Shouldly;
using System.CodeDom.Compiler;
using Sandboxer.Tests.Interfaces;

namespace Sandboxer.Tests
{
    public class AssemblyCreator
    {
        private readonly string dir;

        public AssemblyCreator()
        {
            dir = Path.Combine(Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath), "Plugins");
        }

        public string PluginsDir { get { return dir; } }

        public void CreateAssembly(string fileName, string code)
        {
            fileName = Path.Combine(dir, fileName);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }
            
            var parameters = new CompilerParameters()
            {
                GenerateExecutable = false,
                OutputAssembly = fileName
            };

            CopyAssemblyWithType(typeof(SandboxHost), Path.GetDirectoryName(fileName));
            parameters.ReferencedAssemblies.Add(typeof(SandboxHost).Assembly.Location);

            CopyAssemblyWithType(typeof(IWordGenerator), Path.GetDirectoryName(fileName));
            parameters.ReferencedAssemblies.Add(typeof(IWordGenerator).Assembly.Location);

            var r = CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromSource(parameters, code);

            r.Errors.HasErrors.ShouldBe(false, () => string.Join(Environment.NewLine, r.Errors.OfType<CompilerError>().Select(x => x.ErrorText)));
        }

        private void CopyAssemblyWithType(Type type, string destination)
        {
            var dllPath = type.Assembly.Location;
            File.Copy(dllPath, Path.Combine(destination, Path.GetFileName(dllPath)), true);
        }
    }
}