using System.Collections.Generic;

namespace Sandboxer
{
    public interface ISandboxer
    {
        IEnumerable<SandboxeeInfo> AvailablePlugins { get; }
    }
}