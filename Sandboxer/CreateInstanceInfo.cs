using System;

namespace Sandboxer
{
    [Serializable]
    public class CreateInstanceInfo
    {
        public string AssemblyFullName { get; set; }
        public string TypeFullName { get; set; }
    }
}