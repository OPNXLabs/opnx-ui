using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace OPNX.UI.WPF.Interactivity.DragDrop
{
    public static class DragDropManager
    {
        public static readonly DependencyProperty DragSourceAdvisorProperty =
            DependencyProperty.RegisterAttached("DragSourceAdvisor",
                                                typeof(IDragSourceAdvisor),
                                                typeof(DragDropManager),
                                                new FrameworkPropertyMetadata(OnDragSourceAdvisorChanged));

        public static readonly DependencyProperty DropTargetAdvisorProperty =
            DependencyProperty.RegisterAttached("DropTargetAdvisor",
                                                typeof(IDropTargetAdvisor),
                                                typeof(DragDropManager),
                                                new FrameworkPropertyMetadata(OnDropTargetAdvisorChanged));

        private static readonly string _dragOffsetFormat = "DnD.DragOffset";

        private static Point _adornerPosition;

        private static UIElement? _draggedElt;
        private static Point _dragStartPoint;
        private static bool _isMouseDown;
        private static Point _offsetPoint;
        private static DropPreviewAdorner? _overlayElt;

        private static IDragSourceAdvisor? _currentDragSourceAdvisor { get; set; }

        private static IDropTargetAdvisor? _currentDropTargetAdvisor { get; set; }

        public static void SetDragSourceAdvisor(DependencyObject depObj, IDragSourceAdvisor? advisor)
        {
            depObj.SetValue(DragSourceAdvisorProperty, null);
            depObj.SetValue(DragSourceAdvisorProperty, advisor);
        }

        public static void SetDropTargetAdvisor(DependencyObject depObj, IDropTargetAdvisor? advisor)
        {
            depObj.SetValue(DropTargetAdvisorProperty, advisor);
        }

        public static IDragSourceAdvisor? GetDragSourceAdvisor(DependencyObject? depObj)
        {
            return depObj?.GetValue(DragSourceAdvisorProperty) as IDragSourceAdvisor;
        }

        public static IDropTargetAdvisor? GetDropTargetAdvisor(DependencyObject? depObj)
        {
            return depObj?.GetValue(DropTargetAdvisorProperty) as IDropTargetAdvisor;
        }

        // 드래그 소스 어드바이저 생성시 처리.
        private static void OnDragSourceAdvisorChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is UIElement sourceElt)
            {
                //Debug.Assert(sourceElt != null, "depObj가 UIElement가 아닙니다.");

                if (args.NewValue != null && args.OldValue == null)
                {
                    sourceElt.PreviewMouseLeftButtonDown += DragSource_PreviewMouseLeftButtonDown;
                    sourceElt.PreviewMouseMove += DragSource_PreviewMouseMove;
                    sourceElt.PreviewMouseUp += DragSource_PreviewMouseUp;

                    // Set the Drag source UI
                    var advisor = args.NewValue as IDragSourceAdvisor;
                    //Debug.Assert(advisor != null, "args.NewValus가 IDragSourceAdvisor가 아닙니다.");

                    advisor?.SourceUI = sourceElt;
                }
                else if (args.NewValue == null && args.OldValue != null)
                {
                    sourceElt.PreviewMouseLeftButtonDown -= DragSource_PreviewMouseLeftButtonDown;
                    sourceElt.PreviewMouseMove -= DragSource_PreviewMouseMove;
                    sourceElt.PreviewMouseUp -= DragSource_PreviewMouseUp;
                }
            }
        }

        // 드랍할 타겟 어드바이저 생성시 처리.
        private static void OnDropTargetAdvisorChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is UIElement targetElt)
            {
                //Debug.Assert(targetElt != null, "드랍될 목적 엘리먼트가 UI 엘리먼트가 아닙니다.");
                if (args.NewValue != null && args.OldValue == null)
                {
                    targetElt.PreviewDragEnter += DropTarget_PreviewDragEnter;
                    targetElt.PreviewDragOver += DropTarget_PreviewDragOver;
                    targetElt.PreviewDragLeave += DropTarget_PreviewDragLeave;
                    targetElt.PreviewDrop += DropTarget_PreviewDrop;
                    targetElt.AllowDrop = true;

                    // Set the Drag source UI
                    var advisor = args.NewValue as IDropTargetAdvisor;
                    //Debug.Assert(advisor != null, "새로운 어드바이저가 IDropTargetAdvisor가 아닙니다.");
                    advisor?.TargetUI = targetElt;
                }
                else if (args.NewValue == null && args.OldValue != null)
                {
                    targetElt.PreviewDragEnter -= DropTarget_PreviewDragEnter;
                    targetElt.PreviewDragOver -= DropTarget_PreviewDragOver;
                    targetElt.PreviewDragLeave -= DropTarget_PreviewDragLeave;
                    targetElt.PreviewDrop -= DropTarget_PreviewDrop;
                    targetElt.AllowDrop = false;
                }
            }
        }

        /* ____________________________________________________________________
		 *		Drop Target events 
		 * ____________________________________________________________________
		 */

        private static void DropTarget_PreviewDrop(object sender, DragEventArgs e)
        {
            UpdateEffects(e);

            //Debug.Assert(sender is UIElement, "sender가 UIElement가 아닙니다.");
            var dropPoint = e.GetPosition(sender as UIElement);

            // Calculate displacement for (Left, Top)
            var offset = e.GetPosition(_overlayElt);

            dropPoint.X -= offset.X;
            dropPoint.Y -= offset.Y;

            RemovePreviewAdorner();
            _offsetPoint = new Point(0, 0);

            if (_currentDropTargetAdvisor!.IsValidDataObject(e.Data))
            {
                _currentDropTargetAdvisor.OnDropCompleted(e.Data, dropPoint, e.Effects);
            }
            e.Handled = true;
        }

        private static void DropTarget_PreviewDragLeave(object sender, DragEventArgs e)
        {
            UpdateEffects(e);

            RemovePreviewAdorner();
            e.Handled = true;
        }

        private static void DropTarget_PreviewDragOver(object sender, DragEventArgs e)
        {
            UpdateEffects(e);
            //Debug.Assert(sender is UIElement, "sender가 UIElement가 아닙니다.");

            // Update position of the preview Adorner
            _adornerPosition = e.GetPosition(sender as UIElement);
            PositionAdorner();

            e.Handled = true;
        }

        private static void DropTarget_PreviewDragEnter(object sender, DragEventArgs e)
        {
            // Get the current drop target advisor
            _currentDropTargetAdvisor = GetDropTargetAdvisor(sender as DependencyObject);

            UpdateEffects(e);

            // Setup the preview Adorner
            _offsetPoint = new Point();
            if (_currentDropTargetAdvisor!.ApplyMouseOffset && e.Data.GetData(_dragOffsetFormat) != null)
            {
                _offsetPoint = (Point)e.Data.GetData(_dragOffsetFormat);
            }
            CreatePreviewAdorner(sender as UIElement, e.Data);

            e.Handled = true;
        }

        private static void UpdateEffects(DragEventArgs e)
        {
            if (_currentDropTargetAdvisor?.IsValidDataObject(e.Data) == false)
            {
                e.Effects = DragDropEffects.None;
            }

            else if ((e.AllowedEffects & DragDropEffects.Move) == 0 &&
                     (e.AllowedEffects & DragDropEffects.Copy) == 0)
            {
                e.Effects = DragDropEffects.None;
            }

            else if ((e.AllowedEffects & DragDropEffects.Move) != 0 &&
                     (e.AllowedEffects & DragDropEffects.Copy) != 0)
            {
                e.Effects = ((e.KeyStates & DragDropKeyStates.ControlKey) != 0)
                                ? DragDropEffects.Copy
                                : DragDropEffects.Move;
            }
        }

        /* ____________________________________________________________________
		 *		Drag Source events 
		 * ____________________________________________________________________
		 */

        private static void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Make this the new drag source
            //CurrentDragSourceAdvisor = GetDragSourceAdvisor(sender as DependencyObject);

            //if (CurrentDragSourceAdvisor.IsDraggable(e.Source as UIElement) == false)
            //{
            //	return;
            //}

            //_draggedElt = e.Source as UIElement;
            //_dragStartPoint = e.GetPosition(CurrentDragSourceAdvisor.GetTopContainer());
            //_offsetPoint = e.GetPosition(_draggedElt);
            //_isMouseDown = true;
        }

        private static void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isMouseDown)
            {
                _currentDragSourceAdvisor = GetDragSourceAdvisor(sender as DependencyObject);

                if (_currentDragSourceAdvisor?.IsDraggable(e.Source as UIElement) == true)
                {
                    _draggedElt = e.Source as UIElement;
                    _dragStartPoint = e.GetPosition(_currentDragSourceAdvisor.GetTopContainer());
                    _offsetPoint = e.GetPosition(_draggedElt);
                    _isMouseDown = true;
                }
            }

            if (_isMouseDown && IsDragGesture(e.GetPosition(_currentDragSourceAdvisor?.GetTopContainer())))
            {
                DragStarted(sender as UIElement);
            }
        }

        private static void DragSource_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;

            //Mouse.Capture(null);
        }

        private static void DragStarted(UIElement? uiElt)
        {
            //var data = CurrentDragSourceAdvisor.GetDataObject(_draggedElt);
            var data = _currentDragSourceAdvisor?.DragData;
            if (data == null) return;

            _isMouseDown = false;
            Mouse.Capture(uiElt);

            data.SetData(_dragOffsetFormat, _offsetPoint);
            DragDropEffects supportedEffects = _currentDragSourceAdvisor?.SupportedEffects ?? DragDropEffects.None;

            // Perform DragDrop

            if (_draggedElt == null)
            {
                Mouse.Capture(null);
                return;
            }

            var effects = System.Windows.DragDrop.DoDragDrop(_draggedElt, data, supportedEffects);
            _currentDragSourceAdvisor?.FinishDrag(_draggedElt, effects);

            // Clean up
            RemovePreviewAdorner();
            Mouse.Capture(null);
            _draggedElt = null;
        }

        private static bool IsDragGesture(Point point)
        {
            bool hGesture = Math.Abs(point.X - _dragStartPoint.X) >
                            SystemParameters.MinimumHorizontalDragDistance;
            bool vGesture = Math.Abs(point.Y - _dragStartPoint.Y) >
                            SystemParameters.MinimumVerticalDragDistance;

            return (hGesture | vGesture);
        }

        /* ____________________________________________________________________
		 *		Utility functions
		 * ____________________________________________________________________
		 */

        private static void CreatePreviewAdorner(UIElement? adornedElt, IDataObject data)
        {
            if (_overlayElt != null)
            {
                return;
            }

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(_currentDropTargetAdvisor?.GetTopContainer());
            UIElement? feedbackUI = _currentDropTargetAdvisor?.GetVisualFeedback(data);
            _overlayElt = new DropPreviewAdorner(feedbackUI, adornedElt);
            PositionAdorner();
            layer.Add(_overlayElt);
        }

        private static void PositionAdorner()
        {
            if (_overlayElt is null)
                return;

            _overlayElt.Left = _adornerPosition.X - _offsetPoint.X;
            _overlayElt.Top = _adornerPosition.Y - _offsetPoint.Y;
        }

        private static void RemovePreviewAdorner()
        {
            if (_overlayElt != null)
            {
                AdornerLayer.GetAdornerLayer(_currentDropTargetAdvisor?.GetTopContainer()).Remove(_overlayElt);
                _overlayElt = null;
            }
        }
    }
}


