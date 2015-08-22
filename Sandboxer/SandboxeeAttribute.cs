using System;

namespace Sandboxer
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class SandboxeeAttribute : Attribute
    {
    }
}