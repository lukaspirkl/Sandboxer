using System;
using System.Collections.Generic;

namespace Sandboxer
{
    public interface ISandboxHost : IDisposable
    {
        IEnumerable<SandboxeeInfo> AvailableSandboxees { get; }

        event EventHandler<SandboxeeEventArgs> Loaded;

        event EventHandler<SandboxeeEventArgs> Unloaded;

        IEnumerable<T> GetInstances<T>();
    }

}