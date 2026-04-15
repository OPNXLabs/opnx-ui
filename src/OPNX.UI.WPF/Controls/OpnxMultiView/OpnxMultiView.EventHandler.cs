using System.Windows;

namespace OPNX.UI.WPF.Controls.OpnxMultiView
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

    ///// <summary>
    ///// SlideChangedEventArgs class.
    ///// </summary>
    //public class SlideUpdatedEventArgs : EventArgs
    //{
    //    #region Constructors and Destructors

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="SlideUpdatedEventArgs"/> class. 
    //    /// </summary>
    //    /// <param name="gridControlGuid">
    //    /// The grid control guid.
    //    /// </param>
    //    /// <param name="cellGuid">
    //    /// The cell guid.
    //    /// </param>
    //    /// <param name="element">
    //    /// The element.
    //    /// </param>
    //    public SlideUpdatedEventArgs(Guid gridControlGuid, Guid cellGuid)
    //    {
    //        this.GridControlGuid = gridControlGuid;
    //        this.CellGuid = cellGuid;
    //    }

    //    #endregion

    //    #region Properties

    //    /// <summary>
    //    /// Gets CellGuid.
    //    /// </summary>
    //    public Guid CellGuid { get; private set; }

    //    /// <summary>
    //    /// Gets or sets GridControlGuid.
    //    /// </summary>
    //    public Guid GridControlGuid { get; set; }

    //    #endregion
    //}

    ///// <summary>
    ///// SlideChangedEventArgs class.
    ///// </summary>
    //public class SlideChangedEventArgs : EventArgs
    //{
    //    #region Constructors and Destructors

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="SlideChangedEventArgs"/> class.
    //    /// </summary>
    //    /// <param name="gridControlGuid">
    //    /// The grid control guid.
    //    /// </param>
    //    /// <param name="cellGuid">
    //    /// The cell guid.
    //    /// </param>
    //    /// <param name="element">
    //    /// The element.
    //    /// </param>
    //    /// <param name="index">
    //    /// The index.
    //    /// </param>
    //    public SlideChangedEventArgs(Guid gridControlGuid, Guid cellGuid, UIElement element, int index)
    //    {
    //        this.GridControlGuid = gridControlGuid;
    //        this.CellGuid = cellGuid;
    //        this.Element = element;
    //        this.Index = index;
    //    }

    //    #endregion

    //    #region Properties

    //    /// <summary>
    //    /// Gets CellGuid.
    //    /// </summary>
    //    public Guid CellGuid { get; private set; }

    //    /// <summary>
    //    /// Gets Element.
    //    /// </summary>
    //    public UIElement Element { get; private set; }

    //    /// <summary>
    //    /// Gets or sets GridControlGuid.
    //    /// </summary>
    //    public Guid GridControlGuid { get; set; }

    //    /// <summary>
    //    /// Gets Index.
    //    /// </summary>
    //    public int Index { get; private set; }

    //    #endregion
    //}
}

