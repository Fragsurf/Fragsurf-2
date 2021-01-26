using System.Collections.Generic;

namespace Fragsurf.Shared.LagComp
{
    public class GameState
    {
        public double Time;
        public int SnapshotNumber;
        public List<EntityState> EntityStates = new List<EntityState>();
    }
}