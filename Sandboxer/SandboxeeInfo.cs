using System;

namespace Sandboxer
{
    [Serializable]
    public class SandboxeeInfo
    {
        public string FilePath { get; set; }

        public string Name { get; set; }
    }
}