using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Sandboxer
{
    public sealed class SandboxHost : ISandboxHost
    {
        private string pluginPath;
        private List<SandboxInfo> sandboxesInfo = new List<SandboxInfo>();
        private object sandboxesLock = new object();
        private FileSystemWatcher fileSystemWatcher;

        internal SandboxHost(string pluginPath)
        {
            this.pluginPath = pluginPath;

            //TODO: This is potentionaly not thread safe - domain can be unloaded meanwhile some work is happening
            fileSystemWatcher = new FileSystemWatcher(pluginPath);
            fileSystemWatcher.Deleted += FileSystemWatchEventHandler;
            fileSystemWatcher.Created += FileSystemWatchEventHandler;
            fileSystemWatcher.Changed += FileSystemWatchEventHandler;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void FileSystemWatchEventHandler(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Deleted)
            {
                lock (sandboxesLock)
                {
                    var fileInfo = new FileInfo(e.FullPath);
                    var found = sandboxesInfo.FirstOrDefault(x => x.SandboxeeInfo.FilePath == fileInfo.FullName);
                    if (found != null)
                    {
                        Task.Run(() => { OnUnloaded(found.SandboxeeInfo); });
                        sandboxesInfo.Remove(found);
                        AppDomain.Unload(found.AppDomain);
                    }
                }
            }
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                lock (sandboxesLock)
                {
                    var fileInfo = new FileInfo(e.FullPath);
                    var sandboxInfo = TryLoadSandboxee(fileInfo);
                    if (sandboxInfo != null)
                    {
                        sandboxesInfo.Add(sandboxInfo);
                        Task.Run(() => { OnLoaded(sandboxInfo.SandboxeeInfo); });
                    }
                }
            }
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
            lock (sandboxesLock)
            {
                foreach (var sandboxInfo in sandboxesInfo)
                {
                    Task.Run(() => { OnUnloaded(sandboxInfo.SandboxeeInfo); });
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
                        Task.Run(() => { OnLoaded(sandboxInfo.SandboxeeInfo); });
                    }
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

            sandboxInfo.Guest = (SandboxGuest)sandboxInfo.AppDomain.CreateInstanceAndUnwrap(typeof(SandboxGuest).Assembly.FullName, typeof(SandboxGuest).FullName);
            sandboxInfo.SandboxeeInfo = sandboxInfo.Guest.TryLoadAssembly(fileInfo.FullName);

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
            public SandboxGuest Guest { get; set; }
        }

        public void Dispose()
        {
            lock (sandboxesLock)
            {
                foreach (var sandboxInfo in sandboxesInfo)
                {
                    AppDomain.Unload(sandboxInfo.AppDomain);
                }
            }
            fileSystemWatcher.Dispose();
        }

        public IEnumerable<T> GetInstances<T>()
        {
            lock (sandboxesLock)
            {
                foreach (var sandboxInfo in sandboxesInfo)
                {
                    foreach (var createInstanceInfo in sandboxInfo.Guest.GetCreateInstanceInfo(typeof(T).FullName))
                    {
                        yield return (T)sandboxInfo.AppDomain.CreateInstanceAndUnwrap(createInstanceInfo.AssemblyFullName, createInstanceInfo.TypeFullName);
                    }
                }
            }
        }
    }

}
