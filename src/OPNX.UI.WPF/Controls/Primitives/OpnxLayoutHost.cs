using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    /// <summary>
    /// The process mode.
    /// </summary>
    public enum ProcessMode
    {
        /// <summary>
        /// The normal
        /// </summary>
        Normal,
        /// <summary>
        /// The edit.
        /// </summary>
        Edit,
    }

    /// <summary>
    /// The base layout control.
    /// </summary>
    [ContentProperty("Children")]
    public class OpnxLayoutHost : OpnxControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpnxLayoutHost"/> class.
        /// </summary>
        static OpnxLayoutHost()
        {
            //WindowRect = Rect.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpnxLayoutHost"/> class.
        /// </summary>
        public OpnxLayoutHost()
        {
            this._children = new UIElementCollection(this, this);
        }

        /// <summary>
        /// Window Rect by Main Window.
        /// </summary>
        //public static Rect WindowRect { get; set; }

        public string PackageVersion
        {
            get
            {
                return (string)this.GetValue(PackageVersionProperty);
            }

            set
            {
                this.SetValue(PackageVersionProperty, value);
            }
        }

        public string VersionTag
        {
            get => this.PackageVersion;
            set => this.PackageVersion = value;
        }

        public string UniqueKey
        {
            get
            {
                return (string)this.GetValue(UniqueKeyProperty);
            }

            set
            {
                this.SetValue(UniqueKeyProperty, value);
            }
        }

        public string LayoutKey
        {
            get => this.UniqueKey;
            set => this.UniqueKey = value;
        }
        /// <summary>
        /// "LockMode" 속성.
        /// </summary>
        public ControlLockMode LockMode
        {
            get
            {
                return (ControlLockMode)this.GetValue(LockModeProperty);
            }
            set
            {
                this.SetValue(LockModeProperty, value);
            }
        }

        public static readonly DependencyProperty PackageVersionProperty = DependencyProperty.Register(
            "Version",
            typeof(string),
            typeof(OpnxLayoutHost),
            new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty UniqueKeyProperty = DependencyProperty.Register(
            "Key",
            typeof(string),
            typeof(OpnxLayoutHost),
            new FrameworkPropertyMetadata(string.Empty));
        /// <summary>
        /// "LockMode" 첨부 프로퍼티 등록.
        /// </summary>
        public static readonly DependencyProperty LockModeProperty =
            DependencyProperty.RegisterAttached(
                nameof(LockMode),
                typeof(ControlLockMode),
                typeof(OpnxLayoutHost),
                new FrameworkPropertyMetadata(ControlLockMode.None, new PropertyChangedCallback(OnChangedLockMode)));

        private readonly UIElementCollection _children;

        /// <summary>
        /// 첨부 프로퍼티용 기본 메서드.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetLockMode(FrameworkElement element, ControlLockMode value)
        {
            element.SetValue(LockModeProperty, value);
        }

        /// <summary>
        /// 첨부 프로퍼티용 기본 메서드.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static ControlLockMode GetLockMode(FrameworkElement element)
        {
            return (ControlLockMode)element.GetValue(LockModeProperty);
        }

        /// <summary>
        /// Gets Children.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public UIElementCollection Children
        {
            get
            {
                return this._children;
            }
        }

        /// <summary>
        /// Gets LogicalChildren.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                return this._children.GetEnumerator();
            }
        }

        /// <summary>
        /// Gets VisualChildrenCount.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                return this._children.Count;
            }
        }

        protected static void OnChangedLockMode(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is OpnxControl sender)
            {
                var args = new RoutedPropertyChangedEventArgs<ControlLockMode>(
                    (ControlLockMode)e.OldValue, (ControlLockMode)e.NewValue, LockModeChangedEvent);

                sender.RaiseEvent(args);
            }
        }

        /// <summary>
        /// The do dispose.
        /// </summary>
        /// <param name="isManage">
        /// The is manage.
        /// </param>
        protected override void DoDispose(bool isManage)
        {
            if (isManage)
            {
                for (var i = this.Children.Count - 1; i >= 0; i--)
                {
                    var element = this.Children[i];
                    if (element is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }

            base.DoDispose(isManage);
        }

        /// <summary>
        /// The get visual child.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// return Visual.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= this._children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");
            }

            return this._children[index];
        }

        /// <summary>
        /// Update View Area.
        /// </summary>
        /// <param name="parentCanvasScreenRegion">Parent Canvas Screen Region.</param>
        /// <param name="parentsScreenRegion">All Parent Elements Screen Region.</param>
        public override void UpdateViewAreaForVisibility(Rect parentCanvasScreenRegion, List<Rect> parentsScreenRegion)
        {
        }
    }
}





