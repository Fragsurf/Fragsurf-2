using Fragsurf.BSP;
using Fragsurf.FSM.Actors;

namespace Fragsurf.BSP
{
    [EntityComponent("info_player_*")]
    public class BspSpawnPoint : BspEntityMonoBehaviour
    {

        public int TeamNumber = 0;

        protected override void OnStart()
        {
            switch (Entity.ClassName.ToLower())
            {
                case "info_player_terrorist":
                    TeamNumber = 1;
                    break;
                case "info_player_counterterrorist":
                    TeamNumber = 2;
                    break;
            }
            gameObject.AddComponent<FSMSpawnPoint>().TeamNumber = TeamNumber;
        }

    }
}

