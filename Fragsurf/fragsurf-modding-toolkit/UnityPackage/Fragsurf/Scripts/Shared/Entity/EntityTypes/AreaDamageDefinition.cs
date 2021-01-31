using System;
using Fragsurf.Shared.Player;

namespace Fragsurf.Shared.Entity
{
    [Serializable]
    public struct AreaDamageDefinition 
    {
        public AreaDamageDefinition(HitboxArea area, int percent)
        {
            Area = area;
            DamagePercent = percent;
        }
        public HitboxArea Area;
        public int DamagePercent;
    }
}
