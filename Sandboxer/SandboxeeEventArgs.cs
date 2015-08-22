using System;

namespace Sandboxer
{
    [Serializable]
    public class SandboxeeEventArgs : EventArgs
    {
        public SandboxeeEventArgs(SandboxeeInfo info)
        {
            this.SandboxeeInfo = info;
        }

        public SandboxeeInfo SandboxeeInfo { get; private set; }
    }
}