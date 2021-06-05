using Fragsurf.Shared.Entity;

namespace Fragsurf.BSP
{
    public abstract class BspBaseFilter : BspEntityMonoBehaviour
    {

        public abstract bool Passes(NetEntity ent);

    }
}

