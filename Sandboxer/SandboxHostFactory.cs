using System;
using System.Collections.Generic;

namespace Sandboxer
{
    public class SandboxHostFactory
    {
        public string PluginPath { get; set; }

        public ISandboxHost Create()
        {
            var sandboxHost = new SandboxHost(PluginPath);
            sandboxHost.ReloadAll();
            return sandboxHost;
        }
    }

}