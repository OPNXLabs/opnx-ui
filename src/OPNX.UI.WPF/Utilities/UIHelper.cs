using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OPNX.UI.WPF.Utilities
{
    public static class UIHelper
    {
        public static BitmapSource? CaptureElement(FrameworkElement element)
        {
            if (element == null) return null;

            // 크기 확인
            double width = element.ActualWidth;
            double height = element.ActualHeight;

            // 만약 아직 배치되지 않았다면, 강제로 Measure & Arrange
            if (width == 0 || height == 0)
            {
                if (double.IsNaN(element.Width) || double.IsNaN(element.Height))
                {
                    // 기본 크기 지정 (예: 300x200)
                    width = 300;
                    height = 200;
                }
                else
                {
                    width = element.Width;
                    height = element.Height;
                }

                element.Measure(new Size(width, height));
                element.Arrange(new Rect(0, 0, width, height));
                element.UpdateLayout();
            }

            var renderBitmap = new RenderTargetBitmap(
                (int)width,
                (int)height,
                96, // DPI X
                96, // DPI Y
                PixelFormats.Pbgra32);

            renderBitmap.Render(element);
            renderBitmap.Freeze();
            return renderBitmap;
        }

        public static double GetRatioHighWithScreen(FrameworkElement element)
        {
            try
            {
                // 부모와 연결되어 있지 않으면 처리하지 않음.
                if (PresentationSource.FromVisual(element) == null)
                {
                    return double.NaN;
                }

                Point startPoint = element.PointToScreen(new Point(0, 0));
                Point endPoint = element.PointToScreen(new Point(element.Width, element.Height));

                if (endPoint.Y - startPoint.Y <= 0)
                    return double.NaN;

                if (element.Height <= 0)
                    return double.NaN;

                return element.Height / (endPoint.Y - startPoint.Y);
            }
            catch (Exception)
            {
                return double.NaN;
            }

        }

        public static double GetRatioWidthWithScreen(FrameworkElement element)
        {
            try
            {
                // 부모와 연결되어 있지 않으면 처리하지 않음.
                if (PresentationSource.FromVisual(element) == null)
                {
                    return double.NaN;
                }

                Point startPoint = element.PointToScreen(new Point(0, 0));
                Point endPoint = element.PointToScreen(new Point(element.Width, element.Height));

                if (endPoint.X - startPoint.X <= 0)
                    return double.NaN;

                if (element.Width <= 0)
                    return double.NaN;

                return element.Width / (endPoint.X - startPoint.X);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("InnoConvertUtil.GetRatioWidthWithScreen Exception: " + e.Message);
                return double.NaN;
            }

        }

        public static BindingFlags AllBindings
        {
            get
            {
                return BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Instance | BindingFlags.FlattenHierarchy |
                       BindingFlags.Static | BindingFlags.GetField | BindingFlags.Default | BindingFlags.GetProperty | BindingFlags.SetField;
            }
        }

        /// <summary>
        /// 객체가 가지고 있는 전체 이벤트 핸들러를 제거한다.
        /// </summary>
        /// <param name="eventSource"></param>
        public static void RemoveAllEventHandlers(object eventSource)
        {
            // 사용자 이벤트의 핸들러를 제거한다.
            RemoveEventHandlerByReflection(eventSource);

            // 객체가 DependencyObject일 경우 RoutedEvent에 대해 별도 루틴을 실행한다.
            if (eventSource is DependencyObject)
            {
                var dependencyObject = eventSource as DependencyObject;
                RemoveRoutedEventHandlersRecursive(dependencyObject);
            }
        }

        /// <summary>
        /// 해당 객체가 가지고 있는 RoutedEvent 의 핸들러를 제거한다.
        /// </summary>
        /// <param name="eventSource">(DependencyObject)제거 대한 객체.</param>
        public static void RemoveRoutedEventHandlers(object? eventSource)
        {
            RemoveRoutedEventHandlersByInternalStore(eventSource);
        }

        public static void RemoveRoutedEventHandlersByInternalStore(object? eventSource)
        {
            if (eventSource == null) return;

            var routedEvents = EventManager.GetRoutedEvents();
            var type = eventSource.GetType();
            foreach (var routedEvent in routedEvents)
            {
                var handlerInfo = GetRoutedEventHandlers(eventSource, routedEvent);
                if (handlerInfo == null)
                    continue;


                var onwerEvent = type.GetEvent(routedEvent.Name, AllBindings);
                if (onwerEvent == null)
                    continue;

                foreach (var routedEventHandlerInfo in handlerInfo)
                {
                    onwerEvent.RemoveEventHandler(eventSource, routedEventHandlerInfo.Handler);
                }
            }
        }

        /// <summary>
        /// 자신과 자식 객체의 RoutedEvent의 핸들러까지 제거한다.
        /// </summary>
        /// <param name="eventSource">대상 객체.</param>
        public static void RemoveRoutedEventHandlersRecursive(DependencyObject? eventSource)
        {
            if (eventSource == null)
                return;

            // 기본 RoutedEvent 핸들러를 제거한다.
            RemoveRoutedEventHandlers(eventSource);

            // 자식을 가질 수 있는 객체는 자식을 모두 재귀로 찾아 해당 이벤트들을 제거한다.
            if (eventSource is ContentControl contentControl)
            {
                if (contentControl.Content != null)
                {
                    var dependencyObject = contentControl.Content as DependencyObject;
                    RemoveRoutedEventHandlersRecursive(dependencyObject);
                }
            }
            else if (eventSource is Panel panel)
            {
                foreach (FrameworkElement child in panel.Children)
                {
                    RemoveRoutedEventHandlersRecursive(child);
                }
            }
            else if (eventSource is Decorator decorator)
            {
                if (decorator.Child != null)
                {
                    RemoveRoutedEventHandlersRecursive(decorator.Child);
                }
            }
        }

        public static void RemoveEventHandlerByReflection(object obj)
        {
            if (obj == null) return;

            // 해당 객체의 Type 객체 가져온다.
            var type = obj.GetType();

            // 가져온 Type 객체로부터 Event 목록 가져온다. (타입은 EventInfo[])
            var events = type.GetEvents();

            // 가져온 EventInfo[]를 하나씩 돌면서 실제 Event를 찾아서 제거한다.
            foreach (var eventInfo in events)
            {
                // Type 객체에서 해당 Event 이름을 가진 Field를 가져온다.
                var field = type.GetField(eventInfo.Name, AllBindings);
                if (field == null)
                    continue;

                field.SetValue(obj, null);
            }
        }

        public static void RemoveEventHandler<T>(ref EventHandler<T>? targetEvent) where T : EventArgs
        {
            if (targetEvent == null)
                return;

            foreach (Delegate handler in targetEvent.GetInvocationList())
            {
                targetEvent -= (EventHandler<T>)handler;
            }
        }

        public static RoutedEventHandlerInfo[]? GetRoutedEventHandlers(object? eventSource, RoutedEvent routedEvent)
        {
            if (eventSource == null)
                return null;

            // EventHandlerStore를 가져온다.
            var propertyInfo = eventSource.GetType().GetProperty("EventHandlersStore", AllBindings);
            if (propertyInfo == null)
                return null;

            // 등록된 핸들러가 없으면 리턴.
            var eventHandlersStore = propertyInfo.GetValue(eventSource, null);
            if (eventHandlersStore == null)
                return null;

            var methodInfo = eventHandlersStore.GetType().GetMethod("GetRoutedEventHandlers");
            return methodInfo?.Invoke(eventHandlersStore, [routedEvent]) as RoutedEventHandlerInfo[];
        }

        public static List<string> GetRoutedEventHandlersNames(object? eventSource)
        {
            var routedEventNameList = new List<string>();

            var events = EventManager.GetRoutedEvents();
            foreach (var routedEvent in events)
            {
                var handlerInfo = GetRoutedEventHandlers(eventSource, routedEvent);
                if (handlerInfo == null)
                    continue;

                routedEventNameList.Add(routedEvent.Name);
            }

            if (eventSource is DependencyObject)
            {
                var dependencyObject = eventSource as DependencyObject;
                if (dependencyObject is ContentControl)
                {
                    var contentControl = dependencyObject as ContentControl;
                    if (contentControl?.Content != null)
                    {
                        if (contentControl.Content is DependencyObject)
                        {
                            var contents = contentControl.Content as DependencyObject;
                            routedEventNameList.AddRange(GetRoutedEventHandlersNames(contents));
                        }
                    }
                }
                else if (dependencyObject is Panel panel)
                {
                    foreach (FrameworkElement child in panel.Children)
                    {
                        routedEventNameList.AddRange(GetRoutedEventHandlersNames(child));
                    }
                }
                else if (dependencyObject is Decorator decorator)
                {
                    if (decorator.Child != null)
                    {
                        routedEventNameList.AddRange(GetRoutedEventHandlersNames(decorator.Child));
                    }
                }
            }

            return routedEventNameList;
        }

        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && parent is not T)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        public static T? FindChild<T>(DependencyObject depObj, string childName)
            where T : DependencyObject
        {
            // Confirm obj is valid. 
            if (depObj == null) return null;

            // success case
            if (depObj is T && ((FrameworkElement)depObj).Name == childName)
                return depObj as T;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                //DFS
                T? obj = FindChild<T>(child, childName);

                if (obj != null)
                    return obj;
            }

            return null;
        }

        public static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            // 자식 요소를 순회하면서 탐색
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T foundChild)
                {
                    // 요소가 찾고자하는 타입과 일치하면 반환
                    return foundChild;
                }

                // 자식 요소를 순회하여 재귀적으로 탐색
                var childOfChild = FindChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }

            // 찾지 못한 경우 null 반환
            return null;
        }

        //public static IEnumerable<T> FindChilds<T>(DependencyObject depObj) where T : DependencyObject
        //{
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        //    {
        //        DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
        //        if (child is T)
        //            yield return (T)child;

        //        foreach (T childOfChild in FindChilds<T>(child))
        //        {
        //            yield return childOfChild;
        //        }
        //    }
        //}

        public static IEnumerable<T> FindChilds<T>(DependencyObject depObj) where T : DependencyObject
        {
            // Check if depObj is the same type you're looking for
            if (depObj is T value)
            {
                yield return value;
            }

            // Now find children using LogicalTreeHelper
            foreach (var child in LogicalTreeHelper.GetChildren(depObj))
            {
                if (child is DependencyObject childDepObj)
                {
                    foreach (var childOfChild in FindChilds<T>(childDepObj))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}


