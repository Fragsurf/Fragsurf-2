using System;

namespace Fragsurf
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ConCommandAttribute : Attribute
    {

        public readonly string Path;
        public readonly string Description;
        public readonly ConVarFlags Flags;

        public ConCommandAttribute(string path, string description = "Missing Description", ConVarFlags flags = ConVarFlags.None)
        {
            Path = path;
            Description = description;
            Flags = flags;
        }

    }
}

