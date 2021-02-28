using Fragsurf.Shared;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Shared
{
    [Inject(InjectRealm.Shared)]
    public class SpectateController : FSSharedScript
    {

        private Human _targetHuman;

        public Human TargetHuman
        {
            get => _targetHuman;
            set => Spectate(value);
        }

        protected override void _Tick()
        {
            if(_targetHuman == null || !_targetHuman.IsValid())
            {
                Spectate(Human.Local);
            }
        }

        protected override void _Update()
        {
            _targetHuman?.CameraController?.Update();
        }

        public void Spectate(Human hu)
        {
            if(_targetHuman != null)
            {
                _targetHuman.IsFirstPerson = false;
                _targetHuman.CameraController.Deactivate();
                _targetHuman = null;
            }

            if(hu == null)
            {
                return;
            }

            _targetHuman = hu;
            _targetHuman.IsFirstPerson = true;
            _targetHuman.CameraController.Activate(GameCamera.Camera);
        }

    }
}

