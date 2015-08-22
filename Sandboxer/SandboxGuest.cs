using System;
using System.IO;
using System.Reflection;

namespace Sandboxer
{
    public class SandboxGuest : MarshalByRefObject
    {
        public SandboxeeInfo TryLoadAssembly(string fileName)
        {
            Assembly asm = null;
            try
            {
                asm = Assembly.LoadFrom(fileName);
            }
            catch
            {
                return null;
            }

            var atr = asm.GetCustomAttribute<SandboxeeAttribute>();
            if (atr == null)
            {
                return null;
            }

            var titleAtr = asm.GetCustomAttribute<AssemblyTitleAttribute>();

            return new SandboxeeInfo()
            {
                FilePath = fileName,
                Name = titleAtr == null ? Path.GetFileNameWithoutExtension(fileName) : titleAtr.Title
            };
        }
    }
}