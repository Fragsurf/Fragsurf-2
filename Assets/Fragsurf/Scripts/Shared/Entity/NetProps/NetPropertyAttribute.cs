using System;

namespace Fragsurf.Shared.Entity
{
    public class NetPropertyAttribute : Attribute
    {
        public NetPropertyAttribute(bool skipIfAuthority = false)
        {
            SkipIfAuthority = skipIfAuthority;
        }
        public readonly bool SkipIfAuthority;
    }
}