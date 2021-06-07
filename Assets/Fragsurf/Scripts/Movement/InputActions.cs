
namespace Fragsurf.Movement
{
    [System.Flags]
    public enum InputActions
    {
        None = 0,
        Jump = 1 << 1,
        Duck = 1 << 2,
        Speed = 1 << 3,
        MoveLeft = 1 << 4,
        MoveRight = 1 << 5,
        MoveForward = 1 << 6,
        MoveBack = 1 << 7,
        MoveUp = 1 << 8,
        HandAction = 1 << 9,
        HandAction2 = 1 << 10,
        Interact = 1 << 11,
        Slot1 = 1 << 12,
        Slot2 = 1 << 13,
        Slot3 = 1 << 14,
        Slot4 = 1 << 15,
        Slot5 = 1 << 16,
        Drop = 1 << 17,
        Reload = 1 << 18,
        NextItem = 1 << 19,
        PrevItem = 1 << 20,
        Brake = 1 << 21,
        Flashlight = 1 << 22,
        Slide = 1 << 23
    }
}
