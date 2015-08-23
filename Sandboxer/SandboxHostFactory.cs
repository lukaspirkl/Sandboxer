using System;
using System.Collections.Generic;

namespace Sandboxer
{
    public class SandboxHostFactory
    {
        public SandboxHostFactory(string pluginPath)
        {
            PluginPath = pluginPath;
        }

        public string PluginPath { get; private set; }

        public ISandboxHost Create()
        {
            var sandboxHost = new SandboxHost(PluginPath);
            sandboxHost.ReloadAll();
            return sandboxHost;
        }
    }

}