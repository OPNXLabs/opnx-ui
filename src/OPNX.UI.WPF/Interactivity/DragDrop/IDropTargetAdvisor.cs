using System.Windows;

namespace OPNX.UI.WPF.Interactivity.DragDrop
{
    public interface IDropTargetAdvisor
    {
        UIElement? TargetUI { get; set; }

        bool ApplyMouseOffset { get; }
        bool IsValidDataObject(IDataObject obj);
        void OnDropCompleted(IDataObject obj, Point dropPoint, DragDropEffects effect);
        UIElement GetVisualFeedback(IDataObject obj);
        UIElement GetTopContainer();
    }
}


