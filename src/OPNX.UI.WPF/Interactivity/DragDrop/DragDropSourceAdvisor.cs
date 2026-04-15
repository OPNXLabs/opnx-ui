using System.Windows;

namespace OPNX.UI.WPF.Interactivity.DragDrop
{
    public class DragDropSourceAdvisor(DragDropEffects dropEffect) : IDragSourceAdvisor
    {
        #region Constructors
        public DragDropSourceAdvisor()
            : this(DragDropEffects.Copy)
        {
        }
        #endregion

        #region Properties
        public UIElement? SourceUI { get; set; }

        public DragDropEffects SupportedEffects { get; private set; } = dropEffect;

        public DataObject? DragData { get; set; }
        #endregion

        #region Public Methods
        public virtual void FinishDrag(UIElement draggedElt, DragDropEffects finalEffects)
        {
        }

        public UIElement? GetTopContainer()
        {
            return Application.Current.MainWindow.Content as UIElement;
        }

        public virtual bool IsDraggable(UIElement? dragElt)
        {
            return true;
        }
        #endregion
    }
}


