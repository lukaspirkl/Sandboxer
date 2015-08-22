using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sandboxer
{
    public class SandboxHost : ISandboxer
    {
        private bool pluginInFolder;
        private string pluginPath;
        private List<SandboxInfo> sandboxesInfo = new List<SandboxInfo>();

        internal SandboxHost(string pluginPath, bool pluginInFolder)
        {
            this.pluginPath = pluginPath;
            this.pluginInFolder = pluginInFolder;
        }

        public IEnumerable<SandboxeeInfo> AvailablePlugins { get { return sandboxesInfo.Select(x => x.SandboxeeInfo); } }

        internal void Scan()
        {
            var directoryInfo = new DirectoryInfo(pluginPath);
            foreach (var fileInfo in directoryInfo.GetFiles("*.dll"))
            {
                var sandboxInfo = new SandboxInfo();
                sandboxInfo.FileCreated = fileInfo.CreationTimeUtc;


                sandboxInfo.AppDomain = AppDomain.CreateDomain(fileInfo.Name, null, new AppDomainSetup()
                {
                    ApplicationBase = pluginPath
                });
                var guest = (SandboxGuest)sandboxInfo.AppDomain.CreateInstanceAndUnwrap(typeof(SandboxGuest).Assembly.FullName, typeof(SandboxGuest).FullName);
                sandboxInfo.SandboxeeInfo = guest.TryLoadAssembly(fileInfo.FullName);

                if(sandboxInfo.SandboxeeInfo == null)
                {
                    AppDomain.Unload(sandboxInfo.AppDomain);
                    continue;
                }

                sandboxesInfo.Add(sandboxInfo);
            }
        }

        private class SandboxInfo
        {
            public SandboxeeInfo SandboxeeInfo { get; set; }
            public AppDomain AppDomain { get; set; }
            public DateTime FileCreated { get; set; }
        }
    }

    public class SandboxGuest : MarshalByRefObject
    {
        public SandboxeeInfo TryLoadAssembly(string fileName)
        {
            Assembly asm = null;
            try
            {
                asm = Assembly.LoadFile(fileName);
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
