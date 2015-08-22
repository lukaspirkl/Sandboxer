using System;
using System.IO;
using System.Linq;
using Shouldly;
using System.CodeDom.Compiler;

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

            var sandboxerDllPath = typeof(SandboxHost).Assembly.Location;
            File.Copy(sandboxerDllPath, Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileName(sandboxerDllPath)), true);

            var parameters = new CompilerParameters()
            {
                GenerateExecutable = false,
                OutputAssembly = fileName
            };
            
            parameters.ReferencedAssemblies.Add(typeof(SandboxHost).Assembly.Location);

            var r = CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromSource(parameters, code);

            r.Errors.HasErrors.ShouldBe(false, () => string.Join(Environment.NewLine, r.Errors.OfType<CompilerError>().Select(x => x.ErrorText)));
        }
    }
}