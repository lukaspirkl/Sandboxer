using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sandboxer
{
    public class SandboxGuest : MarshalByRefObject
    {
        Assembly assembly = null;
        List<Assembly> referencedAssemblies = new List<Assembly>();

        public SandboxeeInfo TryLoadAssembly(string fileName)
        {
            try
            {
                assembly = Assembly.LoadFrom(fileName);
                foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                {
                    referencedAssemblies.Add(Assembly.Load(referencedAssembly));
                }
            }
            catch
            {
                return null;
            }

            var atr = assembly.GetCustomAttribute<SandboxeeAttribute>();
            if (atr == null)
            {
                return null;
            }

            var titleAtr = assembly.GetCustomAttribute<AssemblyTitleAttribute>();

            return new SandboxeeInfo()
            {
                FilePath = fileName,
                Name = titleAtr == null ? Path.GetFileNameWithoutExtension(fileName) : titleAtr.Title
            };
        }

        public CreateInstanceInfo[] GetCreateInstanceInfo(string interfaceFullName)
        {
            if (assembly == null)
            {
                return new CreateInstanceInfo[0];
            }

            var interfaceType = referencedAssemblies.SelectMany(x => x.GetTypes()).Union(assembly.GetTypes()).FirstOrDefault(x => x.FullName == interfaceFullName);
            if (interfaceType == null)
            {
                return new CreateInstanceInfo[0];
            }

            return assembly.GetTypes().Where(x => x.GetInterfaces().Contains(interfaceType)).Select(x => new CreateInstanceInfo()
            {
                AssemblyFullName = x.Assembly.FullName,
                TypeFullName = x.FullName
            }).ToArray();
        }
    }

}