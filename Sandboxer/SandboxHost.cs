using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sandboxer
{
    public sealed class SandboxHost : ISandboxHost
    {
        private string pluginPath;
        private List<SandboxInfo> sandboxesInfo = new List<SandboxInfo>();

        internal SandboxHost(string pluginPath)
        {
            this.pluginPath = pluginPath;

            var fileSystemWatcher = new FileSystemWatcher(pluginPath);
            fileSystemWatcher.Deleted += (s, e) => 
            {
                var fileInfo = new FileInfo(e.FullPath);
                var found = sandboxesInfo.FirstOrDefault(x => x.SandboxeeInfo.FilePath == fileInfo.FullName);
                if(found != null)
                {
                    sandboxesInfo.Remove(found);
                    AppDomain.Unload(found.AppDomain);
                    OnUnloaded(found.SandboxeeInfo);
                }
            };
            fileSystemWatcher.Created += (s, e) =>
            {
                var fileInfo = new FileInfo(e.FullPath);
                var sandboxInfo = TryLoadSandboxee(fileInfo);
                if (sandboxInfo != null)
                {
                    sandboxesInfo.Add(sandboxInfo);
                    OnLoaded(sandboxInfo.SandboxeeInfo);
                }
            };
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        public IEnumerable<SandboxeeInfo> AvailableSandboxees { get { return sandboxesInfo.Select(x => x.SandboxeeInfo); } }

        public event EventHandler<SandboxeeEventArgs> Loaded;

        private void OnLoaded(SandboxeeInfo info)
        {
            var handler = Loaded;
            if (handler != null)
            {
                handler(this, new SandboxeeEventArgs(info));
            }
        }

        public event EventHandler<SandboxeeEventArgs> Unloaded;

        private void OnUnloaded(SandboxeeInfo info)
        {
            var handler = Unloaded;
            if (handler != null)
            {
                handler(this, new SandboxeeEventArgs(info));
            }
        }

        internal void ReloadAll()
        {
            foreach (var sandboxInfo in sandboxesInfo)
            {
                OnUnloaded(sandboxInfo.SandboxeeInfo);
                AppDomain.Unload(sandboxInfo.AppDomain);
            }
            sandboxesInfo.Clear();

            var directoryInfo = new DirectoryInfo(pluginPath);
            foreach (var fileInfo in directoryInfo.GetFiles("*.dll"))
            {
                var sandboxInfo = TryLoadSandboxee(fileInfo);
                if (sandboxInfo != null)
                {
                    sandboxesInfo.Add(sandboxInfo);
                    OnLoaded(sandboxInfo.SandboxeeInfo);
                }
            }
        }

        private SandboxInfo TryLoadSandboxee(FileInfo fileInfo)
        {
            var sandboxInfo = new SandboxInfo();
            sandboxInfo.FileCreated = fileInfo.CreationTimeUtc;

            sandboxInfo.AppDomain = AppDomain.CreateDomain(fileInfo.Name, null, new AppDomainSetup()
            {
                ApplicationBase = pluginPath,
                ShadowCopyDirectories = pluginPath,
                ShadowCopyFiles = "true"
            });

            var guest = (SandboxGuest)sandboxInfo.AppDomain.CreateInstanceAndUnwrap(typeof(SandboxGuest).Assembly.FullName, typeof(SandboxGuest).FullName);
            sandboxInfo.SandboxeeInfo = guest.TryLoadAssembly(fileInfo.FullName);

            if (sandboxInfo.SandboxeeInfo == null)
            {
                AppDomain.Unload(sandboxInfo.AppDomain);
                return null;
            }

            return sandboxInfo;
        }

        private class SandboxInfo
        {
            public SandboxeeInfo SandboxeeInfo { get; set; }
            public AppDomain AppDomain { get; set; }
            public DateTime FileCreated { get; set; }
        }

        public void Dispose()
        {
            foreach (var sandboxInfo in sandboxesInfo)
            {
                AppDomain.Unload(sandboxInfo.AppDomain);
            }
        }
    }
}
