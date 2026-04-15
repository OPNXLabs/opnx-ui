using System.Windows;

namespace OPNX.UI.WPF.Interactivity.DragDrop
{
    public interface IDragSourceAdvisor
    {
        UIElement? SourceUI { get; set; }
        DragDropEffects SupportedEffects { get; }
        DataObject? DragData { get; set; }
        void FinishDrag(UIElement draggedElt, DragDropEffects finalEffects);
        bool IsDraggable(UIElement? dragElt);
        UIElement? GetTopContainer();
    }
}


