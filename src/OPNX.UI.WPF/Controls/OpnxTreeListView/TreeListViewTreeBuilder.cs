using System.Collections;

namespace OPNX.UI.WPF.Controls.OpnxTreeListView
{
    public class TreeListViewTreeBuilder(OpnxTreeListView treeListView)
    {
        private readonly OpnxTreeListView _treeListView = treeListView;
        private IEnumerable ItemsSource => _treeListView.ItemsSource;
        private Dictionary<object, TreeListViewNode> NodesByItem => _treeListView.NodesByItem;
        private List<object> VisibleItems => _treeListView.VisibleItems;
        private List<TreeListViewNode> RootNodes => _treeListView.RootNodes;

        public void BuildTree()
        {
            NodesByItem.Clear();
            VisibleItems.Clear();
            RootNodes.Clear();

            if (this.ItemsSource == null)
                return;

            var source = this.ItemsSource.Cast<object>()
                .Where(t => t != null)
                .Select(t => new TreeListViewNode(t, _treeListView, VisibleItems, NodesByItem, _treeListView.VisibleItemsView))
                .ToList();

            var childrenByParentId = new Dictionary<object, List<TreeListViewNode>>();
            var roots = new List<TreeListViewNode>();

            foreach (var node in source)
            {
                if (node.ParentId != null)
                {
                    if (!childrenByParentId.TryGetValue(node.ParentId, out var list))
                        childrenByParentId[node.ParentId] = list = [];
                    list.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }

            RootNodes.AddRange(roots);


            //        var source = this.ItemsSource.Cast<object>()
            //.Select(t =>
            //    _dicToTreeDatasOld.ContainsKey(t)
            //        ? _dicToTreeDatasOld[t]
            //        : new TreeListControlRowData(t, treelistControl, _itemsDisplayList, _dicToTreeDatas, treelistControl.ItemsDisplayListView)
            //).ToList();


            //var source = this.ItemsSource.Cast<object>()
            //    .Select(t =>
            //    _dicToTreeDatasOld.ContainsKey(t) ? _dicToTreeDatasOld[t] : new TreeListControlRowData(t, treelistControl, _itemsDisplayList, _dicToTreeDatas, treelistControl.ItemsDisplayListView)
            //    );

            //var roots = dicPIdGroups.Where(g => dicPIdGroups.Where(t => !t.Equals(g)).SelectMany(t => t.Value).All(dv => dv.Id.Equals(g.Key) != true));
            //if (roots != null)
            //{
            //    foreach (var root in roots)
            //    {
            //        _roots.AddRange(root.Value);
            //    }
            //}

            //var roots = dicPIdGroups.FirstOrDefault(g => dicPIdGroups.Where(t => !t.Equals(g)).SelectMany(t => t.Value).All(dv => dv.Id.Equals(g.Key) != true));
            //if (roots.Value?.Count > 0)
            //    _roots.AddRange(roots.Value);            

            BuildTreeInner(RootNodes, childrenByParentId);
        }

        private void BuildTreeInner(List<TreeListViewNode> currentLevelNodes,
            IDictionary<object, List<TreeListViewNode>> childrenByParentId,
            TreeListViewNode? parent = null, int level = 0, bool visibleChild = true)
        {
            if (currentLevelNodes == null) return;
            foreach (var node in currentLevelNodes)
            {
                if (NodesByItem.TryGetValue(node.Target, out var value) && level > value.Level)
                {
                    continue;
                }

                node.Level = level;
                node.Parent = parent;
                NodesByItem[node.Target] = node;
                if (visibleChild)
                {
                    VisibleItems.Add(node.Target);
                }

                if (node.Id != null)
                {
                    if (childrenByParentId.TryGetValue(node.Id, out var childs))
                    {
                        node.Children = childs;
                        BuildTreeInner(node.Children, childrenByParentId, node, level + 1, node.Expanded && visibleChild);
                    }
                }
            }
        }
    }
}
