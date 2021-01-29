
namespace Fragsurf.Shared.Entity
{
    public interface IDamageable
    {
        void Damage(DamageInfo dmgInfo);
        bool Dead { get; }
    }
}

