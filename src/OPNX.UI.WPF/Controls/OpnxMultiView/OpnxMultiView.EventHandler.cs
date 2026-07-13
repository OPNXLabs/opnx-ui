using System.Windows;

namespace OPNX.UI.WPF.Controls
{
    public class OpnxMultiViewSelectionChangedForStaticEventArgs(IList<MultiViewCell> selectedCells, OpnxMultiView sourceMultiView) : EventArgs
    {
        public IList<MultiViewCell> SelectedCells { get; } = selectedCells;

        public OpnxMultiView SourceMultiView { get; } = sourceMultiView;
    }

    public class OpnxMultiViewZoomedForStaticEventArgs(MultiViewCell zoomedCell, int? selectedStage) : EventArgs
    {
        public MultiViewCell ZoomedCell { get; } = zoomedCell;

        public int? SelectedStage { get; } = selectedStage;
    }

    public class ZoomedCellHoldEventArgs(bool isChecked) : EventArgs
    {
        public bool IsChecked { get; } = isChecked;
    }

    public class OpnxMultiViewSelectionChangedEventArgs(
            int selectedCellCount,
            IList<UIElement> firstCellChildren,
            int rowCount,
            int columnCount) : EventArgs
    {
        public int ColumnCount { get; } = columnCount;

        public IList<UIElement> FirstCellChildren { get; } = firstCellChildren;

        public int RowCount { get; } = rowCount;

        public int SelectedCellCount { get; } = selectedCellCount;
    }

    public class OpnxMultiViewLayoutChangedEventArgs(Guid multiViewId) : EventArgs
    {
        public Guid MultiViewId { get; } = multiViewId;
    }

    public class FullScreenChangedEventArgs : EventArgs
    {
        public Guid CellSyncId { get; set; }

        public Guid MultiViewSyncId { get; set; }

        public bool IsZoomed { get; set; }

        public bool UseSync { get; set; }
    }

    public class CellElementDropCompletedEventArgs(OpnxMultiView targetMultiView, object source, MultiViewCell targetCell) : EventArgs
    {
        public OpnxMultiView TargetMultiView { get; } = targetMultiView;

        public object Source { get; } = source;

        public MultiViewCell TargetCell { get; } = targetCell;
    }

    public class CellElementChangedEventArgs(UIElement element) : EventArgs
    {
        public UIElement Element { get; } = element;
    }
}


