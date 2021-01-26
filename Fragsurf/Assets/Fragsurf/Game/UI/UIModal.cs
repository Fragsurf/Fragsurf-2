
namespace Fragsurf.UI
{
    public interface IUIModal
    {

        CursorType CursorType { get; }
        string ModalName { get; }
        bool IsOpen { get; }
        bool IsHovered { get; }
        bool HasFocusedInputField { get; }
        bool ClosesOnEscape { get; }
        void Open();
        void Close();

    }
}
