using System.Windows;
using System.Windows.Media.Animation;

namespace OPNX.UI.WPF.Interactivity.DragDrop
{
    public class DropTargetAdvisor : IDropTargetAdvisor
    {
        public DropTargetAdvisor()
        {
            ApplyMouseOffset = false;
        }

        /// <summary>
        /// 드랍 이벤트 핸들.
        /// </summary>
        public event EventHandler<DropTargetAdvisorDropCompletedEventArgs>? DropCompleted = null;

        protected virtual FrameworkElement? ExtractElement(IDataObject obj)
        {
            return null;
        }

        #region IDropTargetAdvisor Members

        /// <summary>
        /// Gets or sets TargetUI.
        /// </summary>
        public UIElement? TargetUI { get; set; }

        /// <summary>
        /// Gets a value indicating whether ApplyMouseOffset.
        /// </summary>
        public bool ApplyMouseOffset
        {
            get;
            private set;
        }

        /// <summary>
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// </returns>
        public bool IsValidDataObject(IDataObject obj)
        {
            if (obj != null && this.TargetUI != null)
                return true;


            //var targetCell = this.TargetUI as Cell;
            //if (targetCell != null)
            //{
            //    var multiViewPanel = targetCell.Parent as MultiViewPanel;
            //    if (multiViewPanel != null)
            //    {
            //        //if (multiViewPanel.ControlledCell != null)
            //        //{
            //        //    return false;
            //        //}
            //    }
            //}

            //if (obj.GetDataPresent(supportedFormatForLocation.Name))
            //{
            //    return true;
            //}

            //if (obj.GetDataPresent(supportedFormatForCell.Name))
            //{
            //    return true;
            //}

            //if (obj.GetDataPresent(supportedFormatForCamera.Name))
            //{
            //    return true;
            //}

            //if (obj.GetDataPresent(supportedFormatForCameraControlPlayback.Name))
            //{
            //    return true;
            //}
            //if (obj.GetDataPresent(supportedFormatForImage.Name))
            //{
            //    return true;
            //}

            //if (obj.GetDataPresent(supportedFormatForView.Name))
            //{
            //    return true;
            //}

            return false;
        }

        public void OnDropCompleted(IDataObject obj, Point dropPoint, DragDropEffects effect)
        {
            this.DropCompleted?.Invoke(this, new DropTargetAdvisorDropCompletedEventArgs(obj, this.TargetUI, effect));

            //if (this.DropCompleted != null)
            //{
            //    this.DropCompleted(this, new DropTargetAdvisorDropCompletedEventArgs(obj, this.TargetUI, effect));

            // Source가 Cell인 경우
            //if (obj.GetDataPresent(supportedFormatForCell.Name))
            //{
            //    this.eDropCompleted(this.TargetUI, new CellDropTargetAdvisorDropCompletedEventArgs(obj.GetData(supportedFormatForCell.Name), this.TargetUI as Cell, effect));
            //}

            // Source가 Location인 경우
            //if (obj.GetDataPresent(supportedFormatForLocation.Name))
            //{
            //    this.eDropCompleted(this.TargetUI, new CellDropTargetAdvisorDropCompletedEventArgs(obj.GetData(supportedFormatForLocation.Name), this.TargetUI as Cell));
            //}

            // Source가 카메라인 경우
            // 카메라를 CameraControlPlayback으로 떨어뜨려야 한다
            //if (obj.GetDataPresent(supportedFormatForCameraControlPlayback.Name))
            //{
            //    this.eDropCompleted(this.TargetUI, new CellDropTargetAdvisorDropCompletedEventArgs(obj.GetData(supportedFormatForCameraControlPlayback.Name), this.TargetUI as Cell, effect));
            //}

            // Source가 이미지인 경우
            //if (obj.GetDataPresent(supportedFormatForImage.Name))
            //{
            //    this.eDropCompleted(this.TargetUI, new CellDropTargetAdvisorDropCompletedEventArgs(obj.GetData(supportedFormatForImage.Name), this.TargetUI as Cell));
            //}

            // Source가 레이아웃인 경우
            //if (obj.GetDataPresent(supportedFormatForView.Name))
            //{
            //    this.eDropCompleted(this.TargetUI, new CellDropTargetAdvisorDropCompletedEventArgs(obj.GetData(supportedFormatForView.Name), this.TargetUI as Cell));
            //}
            //}
        }

        public virtual UIElement GetVisualFeedback(IDataObject obj)
        {
            //var element = this.ExtractElement(obj);
            System.Windows.Shapes.Rectangle rect = new()
            {
                Opacity = 0.5,
                IsHitTestVisible = false
            };

            var anim = new DoubleAnimation(0.75, new Duration(TimeSpan.FromMilliseconds(500)))
            { From = 0.25, AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            rect.BeginAnimation(UIElement.OpacityProperty, anim);

            return rect;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public UIElement GetTopContainer()
        {
            return Application.Current?.MainWindow?.Content as UIElement
                   ?? throw new InvalidOperationException("MainWindow.Content is not a UIElement.");
        }
        #endregion
    }

    /// <summary>
    /// DropTargetAdvisorDropCompletedEventArgs Class.
    /// </summary>
    public class DropTargetAdvisorDropCompletedEventArgs(object source, UIElement? targetUIElement, DragDropEffects effect) : EventArgs
    {
        /// <summary>
        /// Gets or sets Source.
        /// </summary>
        public object Source { get; protected set; } = source;

        /// <summary>
        /// Gets or sets TargetCell.
        /// </summary>
        public UIElement? TargetUIElement { get; protected set; } = targetUIElement;

        public DragDropEffects DragDropEffect { get; set; } = effect;
    }
}


