using System;

namespace Fragsurf
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConVarAttribute : Attribute
    {

        public readonly string Path;
        public readonly string Description;
        public readonly ConVarFlags Flags;

        public ConVarAttribute(string path, string description = "Missing Description", ConVarFlags flags = ConVarFlags.None)
        {
            Path = path;
            Description = description;
            Flags = flags;
        }

    }
}

