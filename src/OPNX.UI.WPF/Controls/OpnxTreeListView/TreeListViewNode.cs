using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Controls.OpnxTreeListView
{
    public class TreeListViewNode : DependencyObject, INotifyPropertyChanged, INotifyPropertyChanging
    {
        #region Fields
        private readonly OpnxTreeListView _treeListView;
        private readonly List<object> _visibleItems;
        private readonly IDictionary<object, TreeListViewNode> _nodesByItem;
        private readonly ListCollectionView _visibleItemsView;

        private bool _expanded;
        private TreeListViewNode? _parent = null;
        private int _level;

        private List<TreeListViewNode>? _children = null;
        #endregion

        #region Constructors
        public TreeListViewNode(object target, OpnxTreeListView treeListView, List<object> visibleItems, IDictionary<object, TreeListViewNode> nodesByItem, ListCollectionView visibleItemsView)
        {
            _treeListView = treeListView;
            _visibleItems = visibleItems;
            _nodesByItem = nodesByItem;
            _visibleItemsView = visibleItemsView;
            Target = target;
            Expanded = _treeListView.ExpandAll;
            BindingOperations.SetBinding(this, IdProperty, new Binding() { Path = _treeListView.IdPath, Source = Target, Mode = BindingMode.OneTime });
            if (_treeListView.ParentIdPath != null)
            {
                BindingOperations.SetBinding(this, ParentIdProperty, new Binding() { Path = _treeListView.ParentIdPath, Source = Target, Mode = BindingMode.OneTime });
            }
        }
        #endregion

        #region Events
        /// <inheritdoc cref="INotifyPropertyChanged.PropertyChanged"/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc cref="INotifyPropertyChanging.PropertyChanging"/>
        public event PropertyChangingEventHandler? PropertyChanging;
        protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            //ArgumentNullException.ThrowIfNull(e);

            PropertyChanging?.Invoke(this, e);
        }
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            //ArgumentNullException.ThrowIfNull(e);

            PropertyChanged?.Invoke(this, e);
        }
        #endregion

        #region Properties
        public object Target { get; }

        public bool Expanded
        {
            get { return _expanded; }
            set
            {
                if (SetProperty(ref _expanded, value))
                {
                    if (_expanded)
                        Expand();
                    else
                        UnExpand();
                }
            }
        }
        public int Level { get => _level; internal set => SetProperty(ref _level, value); }
        #endregion

        //private async Task HandleExpandToggleAsync(bool expand, CancellationToken token)
        //{
        //    _isHandlingExpand = true;
        //    try
        //    {
        //        // 짧은 딜레이로 연속 클릭 묶기 (ex: 100ms)
        //        await Task.Delay(1, token);
        //        if (token.IsCancellationRequested) return;

        //        if (expand)
        //            Expand();
        //        else
        //            UnExpand();
        //    }
        //    catch (TaskCanceledException) { }
        //    finally
        //    {
        //        _isHandlingExpand = false;
        //    }
        //}

        private void CancelEditOrNewIfNeeded()
        {
            if (_visibleItemsView == null)
                return;

            static void SafeCancel(Action cancelAction, string operationName)
            {
                try
                {
                    cancelAction();
                }
                catch (InvalidOperationException)
                {
                    // 상태가 이미 해제되었을 수 있음 - 무시
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{operationName} 취소 실패: {ex.Message}");
                }
            }

            if (_visibleItemsView.IsEditingItem)
                SafeCancel(_visibleItemsView.CancelEdit, "EditItem");

            if (_visibleItemsView.IsAddingNew)
                SafeCancel(_visibleItemsView.CancelNew, "AddNew");
        }

        private void Expand(bool needRefresh = true)
        {
            try
            {
                // 안전하게 편집/추가 상태 종료
                CancelEditOrNewIfNeeded();

                var startIndex = _visibleItems.IndexOf(Target);
                if (startIndex < 0)
                    return;

                var visibleChildren = GetVisibleChildren();
                if (visibleChildren.Count == 0)
                    return;

                int insertStartIndex = startIndex + 1;
                int insertCount = visibleChildren.Count;

                bool areChildrenAlreadyPresent =
                    _visibleItems.Count >= insertStartIndex + insertCount &&
                    Enumerable.Range(0, insertCount).All(i => Equals(_visibleItems[insertStartIndex + i], visibleChildren[i]));

                if (!areChildrenAlreadyPresent)
                {
                    int currentEndIndex = _visibleItems.FindIndex(
                        insertStartIndex,
                        item => _nodesByItem.TryGetValue(item, out var node) && node.Level <= Level);

                    int currentRemoveCount = (currentEndIndex == -1)
                        ? _visibleItems.Count - insertStartIndex
                        : currentEndIndex - insertStartIndex;

                    if (currentRemoveCount > 0)
                        _visibleItems.RemoveRange(insertStartIndex, currentRemoveCount);

                    _visibleItems.InsertRange(insertStartIndex, visibleChildren);
                }

                if (needRefresh)
                {
                    // 트랜잭션 상태 확인 후 안전하게 Refresh
                    SafeRefreshCollectionView();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void UnExpand(bool needRefresh = true)
        {
            try
            {
                if (ChildrenDatas == null || !ChildrenDatas.Any())
                    return;

                // 안전하게 편집/추가 상태 종료
                CancelEditOrNewIfNeeded();

                var startIndex = _visibleItems.IndexOf(Target);
                if (startIndex < 0) return;

                startIndex++;
                int endIndex = _visibleItems.FindIndex(
                    startIndex,
                    item => !_nodesByItem.TryGetValue(item, out var node) || node.Level <= Level);

                int removeCount = (endIndex == -1)
                    ? _visibleItems.Count - startIndex
                    : endIndex - startIndex;

                if (removeCount > 0)
                    _visibleItems.RemoveRange(startIndex, removeCount);

                if (needRefresh)
                {
                    // 트랜잭션 상태 확인 후 안전하게 Refresh
                    SafeRefreshCollectionView();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void SafeRefreshCollectionView()
        {
            try
            {
                // 트랜잭션 상태 확인
                if (_visibleItemsView != null)
                {
                    // CollectionView가 편집/추가 상태인지 확인
                    if (_visibleItemsView.IsAddingNew || _visibleItemsView.IsEditingItem)
                    {
                        // 트랜잭션이 진행 중이면 비동기로 지연 실행
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            SafeRefreshCollectionView();
                        }), DispatcherPriority.Background);
                        return;
                    }

                    _visibleItemsView.Refresh();
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"CollectionView Refresh 실패: {ex.Message}");

                // 재시도 (최대 3회)
                RetryRefresh(3);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"예상치 못한 오류: {ex.Message}");
            }
        }

        // 재시도 메서드
        private void RetryRefresh(int retryCount)
        {
            if (retryCount <= 0) return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (_visibleItemsView != null &&
                        !_visibleItemsView.IsAddingNew &&
                        !_visibleItemsView.IsEditingItem)
                    {
                        _visibleItemsView.Refresh();
                    }
                    else
                    {
                        RetryRefresh(retryCount - 1);
                    }
                }
                catch (InvalidOperationException)
                {
                    RetryRefresh(retryCount - 1);
                }
            }), DispatcherPriority.Background);
        }



        //private void unExpand(bool needFresh = true)
        //{
        //    if (this.ChildrenDatas?.Any() != true) { return; }

        //    if (_itemsSourceView.CanCancelEdit)
        //        _itemsSourceView.CancelEdit();
        //    var startIndex = _itemsDisplayList.IndexOf(Target);
        //    if (startIndex == -1) return;
        //    startIndex++;
        //    var end = _itemsDisplayList.FindIndex(startIndex, o => _dicToTreeDatas[o].Leve == Leve);
        //    var removeCount = (end == -1) ? _itemsDisplayList.Count - startIndex : end - startIndex;
        //    if (removeCount > 0)
        //    {
        //        _itemsDisplayList.RemoveRange(startIndex, removeCount);
        //    }

        //    if (needFresh)
        //        _itemsSourceView.Refresh();

        //    if (_expanded != false)
        //    {
        //        SetProperty(ref _expanded, false, nameof(Expanded));
        //    }
        //}

        //private void Expand(bool needFresh = true)
        //{
        //    if (_itemsSourceView.CanCancelEdit)
        //        _itemsSourceView.CancelEdit();

        //    var startIndex = _itemsDisplayList.IndexOf(Target);
        //    if (startIndex == -1) return;

        //    foreach (var item in getVisibleChildren())
        //    {
        //        startIndex++;
        //        if (_itemsDisplayList.Count > startIndex && item == _itemsDisplayList[startIndex])
        //            continue;
        //        _itemsDisplayList.Insert(startIndex, item);
        //    }

        //    if (needFresh)
        //        _itemsSourceView.Refresh();


        //    if (_expanded != true)
        //    {
        //        SetProperty(ref _expanded, true, nameof(Expanded));
        //    }
        //}

        //private IEnumerable<object> getVisibleChildren()
        //{
        //    if (Children == null || !Children.Any())
        //        return Enumerable.Empty<object>();

        //    var result = new List<object>();
        //    var visitedNodes = new HashSet<TreeListControlRowData>();
        //    var visitedTargets = new HashSet<object>();
        //    var stack = new Stack<TreeListControlRowData>();

        //    // 역순으로 스택에 추가 (원래 순서 유지를 위해)
        //    for (int i = Children.Count - 1; i >= 0; i--)
        //    {
        //        if (Children[i] != null)
        //            stack.Push(Children[i]);
        //    }

        //    while (stack.Count > 0)
        //    {
        //        var node = stack.Pop();

        //        if (node?.Target == null || !visitedNodes.Add(node) || !visitedTargets.Add(node.Target))
        //            continue;

        //        result.Add(node.Target);

        //        if (node.Expanded && node.Children?.Any() == true)
        //        {
        //            // 역순으로 스택에 추가
        //            for (int i = node.Children.Count - 1; i >= 0; i--)
        //            {
        //                var child = node.Children[i];
        //                if (child != null && !visitedNodes.Contains(child))
        //                {
        //                    stack.Push(child);
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

        private List<object> GetVisibleChildren()
        {
            if (Children == null || Children.Count <= 0)
                return []; // 빈 리스트, 용량 0

            // 예상 크기로 초기 용량 설정 (리사이징 최소화)
            var result = new List<object>(Children.Count * 2);
            var visitedNodes = new HashSet<TreeListViewNode>();
            var visitedTargets = new HashSet<object>();

            // Stack이 Queue보다 약간 더 빠름 (캐시 지역성)
            var stack = new Stack<TreeListViewNode>(Children.Count);

            // 역순 추가로 원래 순서 유지
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                if (Children[i] != null)
                    stack.Push(Children[i]);
            }

            while (stack.Count > 0)
            {
                var node = stack.Pop();

                if (node?.Target == null ||
                    !visitedNodes.Add(node) ||
                    !visitedTargets.Add(node.Target))
                    continue;

                result.Add(node.Target);

                if (node.Expanded && node.Children?.Count > 0)
                {
                    for (int i = node.Children.Count - 1; i >= 0; i--)
                    {
                        var child = node.Children[i];
                        if (child != null && !visitedNodes.Contains(child))
                            stack.Push(child);
                    }
                }
            }

            return result;
        }

        public TreeListViewNode? Parent { get => _parent; set => _parent = value; }

        public List<TreeListViewNode>? Children
        {
            get { return _children; }
            set
            {
                if (_children != value)
                {
                    _children = value;
                    OnPropertyChanged();
                }
            }
        }
        public IEnumerable<TreeListViewNode> AllChildren => GetAllChildren();


        public bool HasChild => Children?.Count > 0;

        public int ChildrenCount => Children?.Count ?? 0;

        public IEnumerable<object> ChildrenDatas => Children?.Select(t => t.Target) ?? [];

        public int AllChildrenCount => GetAllChildren().Count;

        public IEnumerable<object> AllChildrenDatas => GetAllChildren().Select(t => t.Target);

        private List<TreeListViewNode> GetAllChildren()
        {
            // 초기 빈 리스트 생성
            var result = new List<TreeListViewNode>();

            if (Children == null || Children.Count == 0)
                return result;

            // Stack을 사용하여 깊은 트리 순회
            var stack = new Stack<TreeListViewNode>(Children);

            // 반복문을 사용하여 트리의 모든 자식 요소를 순회
            while (stack.Count > 0)
            {
                var current = stack.Pop();  // 현재 노드 꺼내기
                result.Add(current);         // 결과 리스트에 추가

                var children = current.Children;
                if (children != null && children.Count > 0)
                {
                    // 자식 노드를 스택에 뒤에서부터 푸시 (후입선출 LIFO)
                    for (int i = children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(children[i]);
                    }
                }
            }

            return result;
        }

        #region Dependency Property
        public object Id
        {
            get { return (object)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }
        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register(nameof(Id), typeof(object), typeof(TreeListViewNode), new PropertyMetadata(null));

        public object ParentId
        {
            get { return (object)GetValue(ParentIdProperty); }
            set { SetValue(ParentIdProperty, value); }
        }
        public static readonly DependencyProperty ParentIdProperty =
            DependencyProperty.Register(nameof(ParentId), typeof(object), typeof(TreeListViewNode), new PropertyMetadata(null));
        #endregion

        internal void SetIsOpenAll(bool isOpen)
        {
            if (isOpen)
            {
                Expand(false);
            }
            else
            {
                UnExpand(false);
            }

            if (Children?.Count > 0)
            {
                foreach (var child in Children)
                {
                    child.SetIsOpenAll(isOpen);
                }
            }
        }

        //internal static CancellationTokenSource GetTaskCancellationSource => new CancellationSource();
        //private class CancellationSource : CancellationTokenSource
        //{
        //    public int Count { get; set; }
        //}
        protected bool SetProperty<T>([NotNullIfNotNull(nameof(newValue))] ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (ReferenceEquals(field, newValue))
                return false;

            if (field != null && EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            OnPropertyChanging(new PropertyChangingEventArgs(propertyName));

            field = newValue;

            OnPropertyChanged(propertyName);

            return true;
        }
    }
}
