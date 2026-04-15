using OPNX.UI.WPF.Utilities;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OPNX.UI.WPF.Controls.Primitives
{
    /// <summary>
    /// 모든 컨트롤의 최상위 정의.
    /// </summary>
    public abstract class OpnxControl : Control, IDisposable
    {
        public static readonly DependencyProperty AppearanceMaxLevelProperty =
            DependencyProperty.Register(
                "AppearanceMaxLevel",
                typeof(double),
                typeof(OpnxControl),
                new FrameworkPropertyMetadata(double.PositiveInfinity, OnChangedAppearanceMaxLevel));

        public static readonly DependencyProperty AppearanceMinLevelProperty =
            DependencyProperty.Register(
                "AppearanceMinLevel",
                typeof(double),
                typeof(OpnxControl),
                new FrameworkPropertyMetadata(0.0, OnChangedAppearanceMinLevel));

        public static readonly DependencyProperty IsEditVisibleProperty =
            DependencyProperty.Register(
                nameof(IsEditVisible),
                typeof(bool),
                typeof(OpnxControl),
                new FrameworkPropertyMetadata(true, OnChangedIsEditVisible));

        public static readonly DependencyProperty IsVisibleAtAllLevelsProperty =
            DependencyProperty.Register(
                nameof(IsVisibleAtAllLevels),
                typeof(bool),
                typeof(OpnxControl),
                new FrameworkPropertyMetadata(true, OnChangedIsVisibleAtAllLevels));

        public static readonly DependencyProperty IsLockEditProperty =
            DependencyProperty.Register(
                "IsLockEdit",
                typeof(bool),
                typeof(OpnxControl),
                new FrameworkPropertyMetadata(OnChangedIsLockEdit));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                "IsSelected",
                typeof(bool),
                typeof(OpnxControl),
                new FrameworkPropertyMetadata(false, OnChangedIsSelected));

        /// <summary>
        /// MouseClick시에 연결되는 ActionProcess의 SyncId.
        /// </summary>
        public static readonly DependencyProperty MouseClickIdProperty =
            DependencyProperty.Register(
                nameof(MouseClickId),
                typeof(Guid),
                typeof(OpnxControl),
                new FrameworkPropertyMetadata(Guid.Empty, OnChangedMouseClickId));

        public static readonly DependencyProperty OriginIdProperty =
            DependencyProperty.Register(
                nameof(OriginId),
                typeof(Guid),
                typeof(OpnxControl),
                new FrameworkPropertyMetadata(Guid.Empty, OnChangedOriginId));

        public static readonly DependencyProperty SyncIdProperty =
            DependencyProperty.Register(
                nameof(SyncId),
                typeof(Guid),
                typeof(OpnxControl),
                new FrameworkPropertyMetadata(Guid.Empty, OnChangedSyncId));

        /// <summary>
        /// 그룹 태그를 정의한다. (자동 Navigation 생성시 논리 그룹을 정의하는데 사용됨)
        /// </summary>
        public static readonly DependencyProperty GroupTagProperty = DependencyProperty.Register(
            "GroupTag", typeof(string), typeof(OpnxControl), new FrameworkPropertyMetadata(string.Empty, OnChangedGroupTag));

        /// <summary>
        /// Control의 Description을 정의한다. (자동 Navigation 생성시 Navigation에 표시될 Control의 이름)
        /// </summary>
        //[Category("Camera Settings")]
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(OpnxControl), new FrameworkPropertyMetadata(string.Empty));

        public static readonly RoutedEvent SelectChangedEvent =
            EventManager.RegisterRoutedEvent(
                "SelectChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(OpnxControl));

        public static readonly RoutedEvent LockModeChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(LockModeChanged),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ControlLockMode>),
                typeof(OpnxControl));

        public static readonly RoutedEvent MouseOverChangedEvent =
            EventManager.RegisterRoutedEvent(
                "MouseOverChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<bool>),
                typeof(OpnxControl));

        private bool _isMouseOver;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpnxControl"/> class.
        /// </summary>
        public OpnxControl()
        {
            if (this.OriginId == Guid.Empty)
            {
                this.OriginId = Guid.NewGuid();
            }

            if (this.SyncId == Guid.Empty)
            {
                this.SyncId = Guid.NewGuid();
            }

            //this.IsManipulationEnabled = true;
        }

        public event EventHandler<ChangedDoubleValueEventArgs>? ChangedCenterLatitudeEvent;
        public event EventHandler<ChangedDoubleValueEventArgs>? ChangedCenterLongitudeEvent;

        public event EventHandler<ChangedDoubleValueEventArgs>? ChangedLeftEventHandler;
        public event EventHandler<ChangedDoubleValueEventArgs>? ChangedTopEventHandler;

        public event EventHandler<ChangedDoubleValueEventArgs>? ChangedWidthEventHandler;
        public event EventHandler<ChangedDoubleValueEventArgs>? ChangedHeightEventHandler;

        /// <summary>
        /// The e select changed.
        /// </summary>
        public event RoutedEventHandler SelectChanged
        {
            add
            {
                this.AddHandler(SelectChangedEvent, value);
            }

            remove
            {
                this.RemoveHandler(SelectChangedEvent, value);
            }
        }



        /// <summary>
        /// The is Lock type changed.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<ControlLockMode> LockModeChanged
        {
            add
            {
                this.AddHandler(LockModeChangedEvent, value);
            }

            remove
            {
                this.RemoveHandler(LockModeChangedEvent, value);
            }
        }

        /// <summary>
        /// The mouse over changed.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<bool> MouseOverChanged
        {
            add
            {
                this.AddHandler(MouseOverChangedEvent, value);
            }

            remove
            {
                this.RemoveHandler(MouseOverChangedEvent, value);
            }
        }

        public void OnChangedCenterLongitudeEvent(ChangedDoubleValueEventArgs e)
        {
            ChangedCenterLongitudeEvent?.Invoke(this, e);
        }

        public void OnChangedCenterLatitudeEvent(ChangedDoubleValueEventArgs e)
        {
            ChangedCenterLatitudeEvent?.Invoke(this, e);
        }

        public void OnChangedLeftEventHandler(ChangedDoubleValueEventArgs e)
        {
            ChangedLeftEventHandler?.Invoke(this, e);
        }

        public void OnChangedTopEventHandler(ChangedDoubleValueEventArgs e)
        {
            ChangedTopEventHandler?.Invoke(this, e);
        }

        public void OnChangedHeightEventHandler(ChangedDoubleValueEventArgs e)
        {
            ChangedHeightEventHandler?.Invoke(this, e);
        }

        public void OnChangedWidthEventHandler(ChangedDoubleValueEventArgs e)
        {
            ChangedWidthEventHandler?.Invoke(this, e);
        }

        /// <summary>
        /// The on mouse enter.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            this.IsMouseOver = true;
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (this.Process == null)
            {
                return;
            }

            if (!e.Handled)
            {
                //System.Diagnostics.Debug.WriteLine("OnPreviewMouseLeftButtonUp");

                this.Process.PreviewMouseLeftButtonUpProcess(this, e);
            }
        }

        /// <summary>
        /// 스타일러스 Enter 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStylusEnter(StylusEventArgs e)
        {
            base.OnStylusEnter(e);
            this.IsMouseOver = true;
        }

        /// <summary>
        /// 터치 Enter 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTouchEnter(TouchEventArgs e)
        {
            base.OnTouchEnter(e);
            this.IsMouseOver = true;
        }

        /// <summary>
        /// The on mouse leave.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            this.IsMouseOver = false;
        }

        /// <summary>
        /// 스타일러스 Leave 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStylusLeave(StylusEventArgs e)
        {
            base.OnStylusLeave(e);
            this.IsMouseOver = false;
        }

        /// <summary>
        /// 터치 Leave 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTouchLeave(TouchEventArgs e)
        {
            base.OnTouchLeave(e);
            this.IsMouseOver = false;
        }

        /// <summary>
        /// 오른쪽 버트 다운.
        /// 플레이어에서 줌아웃으로 사용된다.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            if (this.Process == null)
            {
                return;
            }

            if (!e.Handled)
            {
                this.Process.MouseRightButtonDownProcess(this, e);
            }
        }

        /// <summary>
        /// 오른쪽 버트 업.
        /// 플레이어에서 줌아웃으로 사용된다.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);

            if (this.Process == null)
            {
                //Debug.WriteLine("Process가 설정되지 않았습니다.");
                return;
            }

            if (!e.Handled)
            {
                this.Process.MouseRightButtonUpProcess(this, e);
            }
        }

        /// <summary>
        /// 왼쪽 버튼 다운 이벤트.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (this.Process == null)
            {
                return;
            }

            // System.Diagnostics.Debug.WriteLine("OnMouseLeftButtonDown");

            if (!e.Handled)
            {
                this.Process.MouseLeftButtonDownProcess(this, e);
            }
        }

        /// <summary>
        /// 스타일러스 다운 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStylusDown(StylusDownEventArgs e)
        {
            base.OnStylusDown(e);

            if (this.Process == null)
            {
                return;
            }

            //System.Diagnostics.Debug.WriteLine("OnStylusDown");

            if (!e.Handled)
            {
                this.Process.StylusDownProcess(this, e);
            }
        }

        protected override void OnStylusSystemGesture(StylusSystemGestureEventArgs e)
        {
            base.OnStylusSystemGesture(e);

            if (this.Process == null)
            {
                return;
            }

            if (!e.Handled)
            {
                this.Process.StylusSystemGesture(this, e);
            }
        }

        /// <summary>
        /// 터치 다운 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTouchDown(TouchEventArgs e)
        {
            base.OnTouchDown(e);

            if (this.Process == null)
            {
                return;
            }

            //System.Diagnostics.Debug.WriteLine("OnTouchDown");

            //if (!e.Handled)
            //{
            //    this.Process.TouchDownProcess(this, e);
            //}
        }

        /// <summary>
        /// 왼쪽 버튼 업 이벤트.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (this.Process == null)
            {
                return;
            }

            // System.Diagnostics.Debug.WriteLine("OnMouseLeftButtonUp");

            if (!e.Handled)
            {
                this.Process.MouseLeftButtonUpProcess(this, e);
            }
        }

        /// <summary>
        /// 스타일러스 업 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStylusUp(StylusEventArgs e)
        {
            base.OnStylusUp(e);

            if (this.Process == null)
            {
                return;
            }

            //System.Diagnostics.Debug.WriteLine("OnStylusUp");

            if (!e.Handled)
            {
                this.Process.StylusUpProcess(this, e);
            }
        }

        /// <summary>
        /// 터치 업 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTouchUp(TouchEventArgs e)
        {
            base.OnTouchUp(e);

            if (this.Process == null)
            {
                return;
            }

            //System.Diagnostics.Debug.WriteLine("OnTouchUp");

            //if (!e.Handled)
            //{
            //    this.Process.TouchUpProcess(this, e);
            //}
        }

        /// <summary>
        /// 마우스 휠 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (this.Process != null)
            {
                if (!e.Handled)
                {
                    this.Process.MouseWheelProcess(this, e);
                }
            }
        }

        /// <summary>
        /// 마우스가 움직였을 때 이벤트.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (this.Process == null)
            {
                return;
            }

            // System.Diagnostics.Debug.WriteLine("OnMouseMove");

            if (!e.Handled)
            {
                this.Process.MouseMoveProcess(this, e);
            }
        }

        /// <summary>
        /// 스타일러스가 움직였을 때 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStylusMove(StylusEventArgs e)
        {
            base.OnStylusMove(e);

            if (this.Process == null)
            {
                return;
            }

            //System.Diagnostics.Debug.WriteLine("OnStylusMove");

            if (!e.Handled)
            {
                this.Process.StylusMoveProcess(this, e);
            }
        }

        /// <summary>
        /// 터치가 움직였을 때 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTouchMove(TouchEventArgs e)
        {
            base.OnTouchMove(e);

            if (this.Process == null)
            {
                return;
            }

            //System.Diagnostics.Debug.WriteLine("OnTouchMove");

            //if (!e.Handled)
            //{
            //    this.Process.TouchMoveProcess(this, e);
            //}
        }

        /// <summary>
        /// 객체를 터치 입력 시작시 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
        {
            base.OnManipulationStarting(e);

            if (this.Process == null)
            {
                return;
            }

            if (!e.Handled)
            {
                //System.Diagnostics.Debug.WriteLine("OnManipulationStarting");

                this.Process.ManipulationStarting(this, e);
            }
        }

        /// <summary>
        /// 터치 입력이 변경되었을 때 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            if (this.Process == null)
            {
                return;
            }

            if (!e.Handled)
            {
                //System.Diagnostics.Debug.WriteLine("OnManipulationDelta");

                this.Process.ManipulationDelta(this, e);
            }
        }

        /// <summary>
        /// 터치를 화면에 놨을 때 발생하는 이벤트.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnManipulationInertiaStarting(ManipulationInertiaStartingEventArgs e)
        {
            base.OnManipulationInertiaStarting(e);

            if (this.Process == null)
            {
                return;
            }

            if (!e.Handled)
            {
                //System.Diagnostics.Debug.WriteLine("OnManipulationInertiaStarting");

                this.Process.ManipulationInertiaStarting(this, e);
            }
        }

        /// <summary>
        /// Gets or sets Process.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OpnxInteractionController? Process { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OpnxInteractionController? InputController
        {
            get => this.Process;
            set => this.Process = value;
        }

        /// <summary>
        /// Gets or sets AppearanceMaxLevel.
        /// </summary>
        public double AppearanceMaxLevel
        {
            get
            {
                return (double)this.GetValue(AppearanceMaxLevelProperty);
            }

            set
            {
                this.SetValue(AppearanceMaxLevelProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets AppearanceMinLevel.
        /// </summary>
        public double AppearanceMinLevel
        {
            get
            {
                return (double)this.GetValue(AppearanceMinLevelProperty);
            }

            set
            {
                this.SetValue(AppearanceMinLevelProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control is visible in edit mode.
        /// </summary>
        public bool IsEditVisible
        {
            get
            {
                return (bool)this.GetValue(IsEditVisibleProperty);
            }

            set
            {
                this.SetValue(IsEditVisibleProperty, value);
            }
        }

        /// <summary>
        /// Gets Icon.
        /// </summary>
        public FrameworkElement Icon
        {
            get
            {
                return this.GetIcon();
            }
        }

        public string CategoryTag
        {
            get => this.GroupTag;
            set => this.GroupTag = value;
        }

        public string DisplayName
        {
            get => this.Description;
            set => this.Description = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control is visible at every zoom level.
        /// </summary>
        public bool IsVisibleAtAllLevels
        {
            get
            {
                return (bool)this.GetValue(IsVisibleAtAllLevelsProperty);
            }

            set
            {
                this.SetValue(IsVisibleAtAllLevelsProperty, value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsDisposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsLockEdit.
        /// </summary>
        public bool IsLockEdit
        {
            get
            {
                return (bool)this.GetValue(IsLockEditProperty);
            }

            set
            {
                this.SetValue(IsLockEditProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether IsMouseOver.
        /// </summary>
        public new bool IsMouseOver
        {
            get
            {
                return this._isMouseOver;
            }

            set
            {
                this.SetMouseOver(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether IsSelected.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return (bool)this.GetValue(IsSelectedProperty);
            }

            set
            {
                this.SetValue(IsSelectedProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets MouseClickId.
        /// </summary>
        public Guid MouseClickId
        {
            get
            {
                return (Guid)this.GetValue(MouseClickIdProperty);
            }

            set
            {
                this.SetValue(MouseClickIdProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets OriginId.
        /// </summary>
        public Guid OriginId
        {
            get
            {
                return (Guid)this.GetValue(OriginIdProperty);
            }

            protected set
            {
                this.SetValue(OriginIdProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets SyncId.
        /// </summary>
        public Guid SyncId
        {
            get
            {
                return (Guid)this.GetValue(SyncIdProperty);
            }

            set
            {
                this.SetValue(SyncIdProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets GroupTag.
        /// </summary>
        public string GroupTag
        {
            get
            {
                return (string)this.GetValue(GroupTagProperty);
            }

            set
            {
                this.SetValue(GroupTagProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description
        {
            get
            {
                return (string)this.GetValue(DescriptionProperty);
            }

            set
            {
                this.SetValue(DescriptionProperty, value);
            }
        }

        private static double GetThreeDigit(double number)
        {
            return Convert.ToDouble(string.Format("{0:F3}", number));
        }

        public double Left
        {
            get
            {
                return GetThreeDigit(Canvas.GetLeft(this));
            }
            set
            {
                var left = GetThreeDigit(value);
                if (Canvas.GetLeft(this).Equals(left)) return;
                Canvas.SetLeft(this, left);
                this.OnChangedLeftEventHandler(new ChangedDoubleValueEventArgs(left));
            }
        }

        public double Top
        {
            get
            {
                return GetThreeDigit(Canvas.GetTop(this));
            }
            set
            {
                var top = GetThreeDigit(value);
                if (Canvas.GetTop(this).Equals(top)) return;
                Canvas.SetTop(this, top);
                this.OnChangedTopEventHandler(new ChangedDoubleValueEventArgs(top));
            }
        }

        public new double Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                var width = GetThreeDigit(value);
                if (Width.Equals(width)) return;
                base.Width = width;
                this.OnChangedWidthEventHandler(new ChangedDoubleValueEventArgs(width));
            }
        }
        public new double Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                var height = GetThreeDigit(value);
                if (Height.Equals(height)) return;
                base.Height = height;
                this.OnChangedHeightEventHandler(new ChangedDoubleValueEventArgs(height));
            }
        }


        public OpnxLayoutHost? ParentOpnxLayoutHost
        {
            get
            {
                return GetParentOpnxLayoutHost(this);
            }
        }

        /// <summary>
        /// The get icon.
        /// </summary>
        /// <returns>
        /// return FrameworkElement.
        /// </returns>
        public virtual FrameworkElement GetIcon()
        {
            return new DefaultControlIcon();
        }

        /// <summary>
        /// 객체 반환 코드.
        /// </summary>
        /// <returns></returns>
        public virtual int GetObjectCount()
        {
            return 1;
        }

        /// <summary>
        /// Update View Area.
        /// </summary>
        /// <param name="parentCanvasScreenRegion">Parent Canvas Screen Region.</param>
        /// <param name="parentsScreenRegion">All Parent Elements Screen Region.</param>
        public virtual void UpdateViewArea(Rect parentCanvasScreenRegion, List<Rect> parentsScreenRegion)
        {
        }

        /// <summary>
        /// Update View Area.
        /// </summary>
        /// <param name="parentCanvasScreenRegion">Parent Canvas Screen Region.</param>
        /// <param name="parentsScreenRegion">All Parent Elements Screen Region.</param>
        public virtual void UpdateViewAreaForVisibility(Rect parentCanvasScreenRegion, List<Rect> parentsScreenRegion)
        {
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // http://anasoft.wordpress.com/2007/06/17/more-about-gc-dispose-and-finalize/
            // Prevent the GC to call Finalize again, since you have alreadycleaned up.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The on changed appearance max level.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected static void OnChangedAppearanceMaxLevel(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// The on changed appearance min level.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected static void OnChangedAppearanceMinLevel(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// The on changed edit visible.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected static void OnChangedIsEditVisible(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var baseControl = (OpnxControl)o;
            baseControl.ChangeVisible(e);
        }

        /// <summary>
        /// The on changed is appear in all level.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected static void OnChangedIsVisibleAtAllLevels(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// The on changed is lock edit.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected static void OnChangedIsLockEdit(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// The on changed is selected.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected static void OnChangedIsSelected(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var baseControl = (OpnxControl)o;
            baseControl.RaiseEvent(new RoutedEventArgs(SelectChangedEvent, baseControl));
        }

        /// <summary>
        /// The on changed mouse click guid.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected static void OnChangedMouseClickId(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// The on changed origin guid.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected static void OnChangedOriginId(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GuidHelper.RemoveOriginMapping((Guid)e.OldValue, o as UIElement);
            GuidHelper.AddOriginMapping((Guid)e.NewValue, o as UIElement);
        }

        /// <summary>
        /// The on changed sync guid.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected static void OnChangedSyncId(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GuidHelper.RemoveSyncMapping((Guid)e.OldValue, o as UIElement);
            GuidHelper.AddSyncMapping((Guid)e.NewValue, o as UIElement);
        }

        /// <summary>
        /// The on changed group tag.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected static void OnChangedGroupTag(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is OpnxControl control)
            {
                control.GroupTagChanged();
            }
        }

        //Tag에 직접 Binding을 할 경우 느려지는 증상이 발생함 !!
        //GroupTag를 중간에 두고 Tag의 값을 변경할 경우 느려지는 증상이 없어짐 !!
        private void GroupTagChanged()
        {
            this.Tag = this.GroupTag;
        }

        /// <summary>
        /// The do dispose.
        /// </summary>
        /// <param name="isManage">
        /// The is manage.
        /// </param>
        protected virtual void DoDispose(bool isManage)
        {
            // Guid등록 해제
            this.UnregisterGuid();
        }

        /// <summary>
        /// The set mouse over.
        /// </summary>
        /// <param name="value">
        /// The is mouse over.
        /// </param>
        protected void SetMouseOver(bool value)
        {
            if (this._isMouseOver != value)
            {
                bool oldValue = this._isMouseOver;
                this._isMouseOver = value;

                this.RaiseEvent(new RoutedPropertyChangedEventArgs<bool>(oldValue, this._isMouseOver, MouseOverChangedEvent));
            }
        }

        private void ChangeVisible(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                this.Visibility = Visibility.Visible;
            }
            else
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        private void Dispose(bool isManage)
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsDisposed = true;

            this.DoDispose(isManage);
        }

        private void UnregisterGuid()
        {
            Guid syncGuid = Guid.Empty;
            Guid originGuid = Guid.Empty;

            if (this.Dispatcher.CheckAccess())
            {
                syncGuid = this.SyncId;
                originGuid = this.OriginId;
            }
            else
            {
                this.Dispatcher.Invoke(
                    new Action(
                        () =>
                        {
                            syncGuid = this.SyncId;
                            originGuid = this.OriginId;
                        }));
            }
            GuidHelper.RemoveSyncMapping(syncGuid, this);
            GuidHelper.RemoveOriginMapping(originGuid, this);
        }

        // LayoutUtils 의 사용 빈도를 줄이고픔. (2011.08.25)
        private static OpnxLayoutHost? GetParentOpnxLayoutHost(FrameworkElement element)
        {
            if (element == null)
            {
                return null;
            }

            return FindOpnxLayoutHostAtParent(element);
        }

        private static OpnxLayoutHost? FindOpnxLayoutHostAtParent(FrameworkElement element)
        {
            if (element == null)
            {
                return null;
            }

            if (element.Parent is OpnxLayoutHost baseLayoutControl)
            {
                return baseLayoutControl;
            }

            if (element.Parent is FrameworkElement frameworkElement)
            {
                return FindOpnxLayoutHostAtParent(frameworkElement);
            }

            return null;
        }

        /// <summary>
        /// 최상위 LAYOUT 인지 체크 한다. 
        /// </summary>
        /// <returns></returns>
        public bool IsRootLayoutControl()
        {
            if (this is OpnxLayoutHost && this.ParentOpnxLayoutHost == null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 중점 좌표를 반환한다.
        /// </summary>
        /// <returns>중점 좌표</returns>
        public Point GetCenterPosition()
        {
            var pt = new Point()
            {
                X = Canvas.GetLeft(this),
                Y = Canvas.GetTop(this)
            };

            pt.X += this.Width / 2;
            pt.Y += this.Height / 2;

            return pt;
        }


        ///// <summary>
        ///// 좌상단 좌표를 반환한다.
        ///// </summary>
        ///// <returns>중점 좌표</returns>
        //public Point GetLeftTopPosition()
        //{
        //    Point pt = new Point();

        //    pt.X = Canvas.GetLeft(this);
        //    pt.Y = Canvas.GetTop(this);

        //    return pt;
        //}

        ///// <summary>
        ///// 우하단 좌표를 반환한다.
        ///// </summary>
        ///// <returns>중점 좌표</returns>
        //public Point GetRightBottomPosition()
        //{
        //    Point pt = new Point();

        //    pt.X = Canvas.GetRight(this);
        //    pt.Y = Canvas.GetBottom(this);

        //    return pt;
        //}
    }

    public class ChangedDoubleValueEventArgs(double value) : EventArgs
    {
        public double DoubleValue { get; set; } = value;
    }

    /// <summary>
    /// OpnxControl을 상속받은 Control들의 공통이름을 정의함 !!
    /// </summary>
    public enum OpnxControlType
    {
        Base,
        MultiView,
    }

    /// <summary>
    /// Zoom In / Out 상황에 대한 엘리먼트의 잠금 종류.
    /// </summary>
    public enum ControlLockMode
    {
        /// <summary>
        /// 장금을 설정하지 않는다. ( NONE )
        /// </summary>
        None,

        /// <summary>
        /// 크기를 항상 고정한다. ( PIN )
        /// </summary>
        OnlySize,

        /// <summary>
        /// 크기와 위치를 항상 고정한다. ( LOGO )
        /// </summary>
        PositionAndSize,
    }

}








