using System;
using System.Collections.Generic;

namespace Sandboxer
{
    public class SandboxerFactory
    {
        public bool PluginInFolder { get; set; }

        public string PluginPath { get; set; }

        public ISandboxer Create()
        {
            var sandboxHost = new SandboxHost(PluginPath, PluginInFolder);
            sandboxHost.Scan();
            return sandboxHost;
        }
    }

}