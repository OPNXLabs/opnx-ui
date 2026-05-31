using OPNX.UI.WPF.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Utilities
{
    public static class ControlHelper
    {
        #region Public Methods

        /// <summary>
        /// Get AllLogicalChildren.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// return List of UIElement.
        /// </returns>
        public static List<UIElement> GetAllLogicalChildren(object obj)
        {
            var result = new List<UIElement>();

            if (obj is not DependencyObject parent)
            {
                return result;
            }

            foreach (object child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject childDependencyObject)
                {
                    AddLogicalChildren(childDependencyObject, result);
                }
            }

            return result;
        }

        /// <summary>
        /// GetAllParentForEditor method.
        /// </summary>
        /// <param name="child">
        /// The child.
        /// </param>
        /// <returns>
        /// return List of FrameworkElement.
        /// </returns>
        public static List<FrameworkElement> GetAllParentForEditor(FrameworkElement child)
        {
            var result = new List<FrameworkElement>();

            FrameworkElement? current = child.Parent as FrameworkElement;

            while (current != null)
            {
                if (current is Panel || current is Viewbox || current is ScrollViewer)
                {
                    result.Add(current);
                }

                current = current.Parent as FrameworkElement;
            }

            return result;
        }

        /// <summary>
        /// GetAllParents method.
        /// </summary>
        /// <param name="child">
        /// the child.
        /// </param>
        /// <param name="removeSelf">
        /// 자기 자신을 삭제할지 여부.
        /// </param>
        /// <returns>
        /// return List of FrameworkElement.
        /// </returns>
        public static List<FrameworkElement> GetAllParents(FrameworkElement child, bool removeSelf = true)
        {
            var result = new List<FrameworkElement>();

            for (FrameworkElement? current = removeSelf ? child.Parent as FrameworkElement : child;
                 current != null;
                 current = current.Parent as FrameworkElement)
            {
                result.Add(current);
            }

            return result;
        }

        /// <summary>
        /// Get AllVisualChildren.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// return List of UIElement.
        /// </returns>
        public static List<UIElement> GetAllVisualChildren(object obj)
        {
            var result = new List<UIElement>();

            if (obj is not DependencyObject parent)
            {
                return result;
            }

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                AddVisualChildren(VisualTreeHelper.GetChild(parent, i), result);
            }

            return result;
        }

        /// <summary>
        /// Get AncestryOpnxLayoutHosts.
        /// </summary>
        /// <param name="layoutControl">
        /// The layout control.
        /// </param>
        /// <returns>
        /// return List of OpnxLayoutHost.
        /// </returns>
        public static List<OpnxLayoutHost> GetAncestryOpnxLayoutHosts(OpnxLayoutHost layoutControl)
        {
            List<OpnxLayoutHost> result = [];
            result.Add(layoutControl);

            OpnxLayoutHost? curLayoutControl = GetParentOpnxLayoutHost(layoutControl);
            if (curLayoutControl != null)
            {
                result.AddRange(GetAncestryOpnxLayoutHosts(curLayoutControl));
            }

            return result;
        }

        /// <summary>
        /// Get ParentOpnxLayoutHost.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// return OpnxLayoutHost.
        /// </returns>
        public static OpnxLayoutHost? GetParentOpnxLayoutHost(FrameworkElement element)
        {
            if (element == null)
            {
                return null;
            }

            return FindOpnxLayoutHostAtParent(element);
        }

        /// <summary>
        /// Get RootOpnxLayoutHost.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// return OpnxLayoutHost.
        /// </returns>
        public static OpnxLayoutHost? GetRootOpnxLayoutHost(FrameworkElement element)
        {
            if (element == null)
            {
                return null;
            }

            OpnxLayoutHost? tmpLayout = null;
            return FindRootOpnxLayoutHostAtParent(element, ref tmpLayout);
        }

        /// <summary>
        /// childVisual이 속해있는 Visual Tree상의 최상위 Visual을 리턴한다.
        /// </summary>
        /// <param name="childVisual">
        /// child Visual.
        /// </param>
        /// <returns>
        /// return Visual.
        /// </returns>
        public static Visual? GetRootVisual(Visual? childVisual)
        {
            var current = childVisual;

            while (VisualTreeHelper.GetParent(current) is Visual parent)
            {
                current = parent;
            }

            return current;

            //DependencyObject obj = VisualTreeHelper.GetParent(childVisual);
            //if (obj == null || !(obj is Visual))
            //{
            //    return childVisual;
            //}
            //else
            //{
            //    return GetRootVisual(obj as Visual);
            //}
        }

        /// <summary>
        /// DispatcherFrame을 끊고 강제로 화면 Rendering을 요청한다.
        /// </summary>
        public static void DispatcherDoRenderEvents()
        {
            var frame = new DispatcherFrame();

            /* DispatcherPriority.Loaded: layout과 render가 다 끝나고 input 메시지들이 처리되기 전에 처리된다.
             * Input보다 같거나 낮은 Priority를 줄 시 이 method가 끝나기 전에 마우스 입력이 들어오고 있었으면 프로그램이 hang되는 문제가 있음.
             * 이 method는 화면을 강제로 Render하는 데 사용하는 method이므로, Priority를 Loaded로 지정해도 문제가 없어야 정상.
             * By iskim (2013.08.21) */

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded,
            new Action(() =>
            {
                frame.Continue = false;
            }));

            Dispatcher.PushFrame(frame);
        }

        /// <summary>
        /// parent 컨트롤 내 Tag값이 일치하는 컨트롤을 가져온다.
        /// </summary>
        /// <typeparam name="T">컨트롤 타입</typeparam>
        /// <param name="parent">부모 컨트롤</param>
        /// <param name="tag">찾은 Tag 내용</param>
        /// <returns>결과 컨트롤</returns>
        public static T? FindControlWithTag<T>(this DependencyObject parent, string tag) where T : UIElement
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                    if (typeof(FrameworkElement).IsAssignableFrom(child.GetType())
                        && ((string)((FrameworkElement)child).Tag == tag))
                    {
                        return child as T;
                    }
                    var item = FindControlWithTag<T>(child, tag);
                    if (item != null) return item as T;
                }
            }
            return null;
        }

        /// <summary>
        /// parent 컨트롤 내 Tag값이 일치하는 컨트롤을 가져온다.
        /// </summary>
        /// <typeparam name="T">컨트롤 타입</typeparam>
        /// <param name="parent">부모 컨트롤</param>
        /// <param name="name">찾은 Name 내용</param>
        /// <returns>결과 컨트롤</returns>
        public static T? FindControlWithName<T>(this DependencyObject parent, string name) where T : UIElement
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                    if (typeof(FrameworkElement).IsAssignableFrom(child.GetType())
                        && ((string)((FrameworkElement)child).Name == name))
                    {
                        return child as T;
                    }
                    var item = FindControlWithTag<T>(child, name);
                    if (item != null) return item as T;
                }
            }
            return null;
        }

        /// <summary>
        /// 버튼 클릭이벤트 발생(버튼 컨트롤)
        /// </summary>
        /// <param name="btn">클릭이벤트를 발생시킬 버튼</param>
        public static void PerformClick(this Button btn)
        {
            btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        /// <summary>
        /// 버튼 클릭이벤트 발생(토글버튼 컨트롤)
        /// </summary>
        /// <param name="toggleBtn">클릭이벤트를 발생시킬 토글버튼</param>
        public static void PerformClick(this ToggleButton toggleBtn)
        {
            toggleBtn.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
        }

        /// <summary>
        /// 부모 중 지정한 타입을 찾고 있으면 찾은 컨트롤을 반환한다.
        /// </summary>
        /// <typeparam name="T">부모 중 검색할 타입</typeparam>
        /// <param name="child">검색 시작 컨트롤</param>
        /// <returns></returns>
        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            var parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null)
            {
                return null;
            }

            //check if the parent matches the type we're looking for
            var parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }

        #endregion

        #region Methods

        private static OpnxLayoutHost? FindOpnxLayoutHostAtParent(FrameworkElement element)
        {
            OpnxLayoutHost? result = null;

            if (element == null)
            {
                result = null;
            }
            else if (element.Parent is OpnxLayoutHost baseLayoutControl)
            {
                result = baseLayoutControl;
            }
            else if (element.Parent is FrameworkElement frameworkElement)
            {
                result = FindOpnxLayoutHostAtParent(frameworkElement);
            }

            return result;
        }

        private static OpnxLayoutHost? FindRootOpnxLayoutHostAtParent(
            FrameworkElement? element, ref OpnxLayoutHost? tmpLayout)
        {
            if (element is OpnxLayoutHost)
            {
                tmpLayout = element as OpnxLayoutHost;
            }

            if (element?.Parent == null)
            {
                return tmpLayout;
            }

            return FindRootOpnxLayoutHostAtParent(element.Parent as FrameworkElement, ref tmpLayout);
        }

        private static void AddLogicalChildren(DependencyObject parent, List<UIElement> result)
        {
            if (parent is UIElement element)
            {
                result.Add(element);
            }

            foreach (object child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject childDependencyObject)
                {
                    AddLogicalChildren(childDependencyObject, result);
                }
            }
        }


        private static void AddVisualChildren(DependencyObject parent, List<UIElement> result)
        {
            if (parent is UIElement element)
            {
                result.Add(element);
            }

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                AddVisualChildren(VisualTreeHelper.GetChild(parent, i), result);
            }
        }

        #endregion
    }
}




