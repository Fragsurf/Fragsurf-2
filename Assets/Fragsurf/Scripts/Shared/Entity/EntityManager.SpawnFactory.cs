
namespace Fragsurf.Shared.Entity
{
    public partial class EntityManager
    {
 
        public Equippable SpawnEquippable()
        {
            var ent = new Equippable(Game);
            AddEntity(ent);
            return ent;
        }
        
        public TestEntity SpawnTestEntity()
        {
            var testEnt = new TestEntity(Game);
            AddEntity(testEnt);
            return testEnt;
        }

    }
}
