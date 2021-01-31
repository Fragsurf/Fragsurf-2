
namespace Fragsurf.Shared.Entity
{
    public interface IInteractable
    {
        void OnInteract(NetEntity interactee);
        void MouseEnter(int clientIndex);
        void MouseExit(int clientIndex);
    }
}

