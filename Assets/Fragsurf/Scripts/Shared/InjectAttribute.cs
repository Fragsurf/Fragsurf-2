using System;

namespace Fragsurf.Shared
{
    [System.Flags]
    public enum InjectRealm
    {
        Client = 1 << 0,
        Server = 1 << 1,
        Shared = 1 << 2
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class InjectAttribute : Attribute
    {
        public InjectAttribute(InjectRealm realm)
        {
            Realm = realm;
        }
        public InjectAttribute(InjectRealm realm, Type gamemodeType)
        {
            Realm = realm;
            GamemodeType = gamemodeType;
        }
        public readonly InjectRealm Realm;
        public readonly Type GamemodeType;
    }
}

