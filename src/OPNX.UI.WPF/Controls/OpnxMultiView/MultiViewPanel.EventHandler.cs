using System.Windows;

namespace OPNX.UI.WPF.Controls
{
    internal class ControlledCellChangedEventArgs : EventArgs
    {
        #region Constructors and Destructors

        internal ControlledCellChangedEventArgs(MultiViewCell? cell)
        {
            this.Cell = cell;
        }

        #endregion

        #region Properties

        internal MultiViewCell? Cell { get; private set; }

        #endregion
    }

    internal class CellAddedArgs : EventArgs
    {
        #region Constructors and Destructors

        internal CellAddedArgs(IList<MultiViewCell> addedCells)
        {
            this.AddedCells = addedCells;
        }

        #endregion

        #region Properties

        internal IList<MultiViewCell> AddedCells { get; private set; }

        #endregion
    }

    internal class CellDropCompletedArgs : EventArgs
    {
        #region Constructors and Destructors

        internal CellDropCompletedArgs(MultiViewPanel targetMultiView, object source, MultiViewCell targetCell)
        {
            this.TargetMultiView = targetMultiView;
            this.Source = source;
            this.TargetCell = targetCell;
        }

        #endregion

        #region Properties

        internal MultiViewPanel TargetMultiView { get; private set; }
        internal object Source { get; private set; }
        internal MultiViewCell TargetCell { get; private set; }

        #endregion
    }

    internal class CellRemovedArgs : EventArgs
    {
        #region Constructors and Destructors

        internal CellRemovedArgs(IList<MultiViewCell> removedCells)
        {
            this.RemovedCells = removedCells;
        }

        #endregion

        #region Properties

        internal IList<MultiViewCell> RemovedCells { get; private set; }

        #endregion
    }

    internal class SelectionChangedArgs : EventArgs
    {
        #region Constructors and Destructors

        internal SelectionChangedArgs(Rect actualSelectionArea, IList<MultiViewCell> selectedCells)
        {
            this.ActualSelectionArea = actualSelectionArea;
            this.SelectedCells = selectedCells;
        }

        #endregion

        #region Properties

        internal Rect ActualSelectionArea { get; private set; }

        internal IList<MultiViewCell> SelectedCells { get; private set; }

        #endregion
    }

    internal class SelectionEnableChangedArgs : EventArgs
    {
        #region Constructors and Destructors

        internal SelectionEnableChangedArgs(bool selectionEnabled)
        {
            this.SelectionEnabled = selectionEnabled;
        }

        #endregion

        #region Properties

        internal bool SelectionEnabled { get; private set; }

        #endregion
    }

    internal class ZoomedCellChangeEventArgs : EventArgs
    {
        #region Constructors and Destructors

        internal ZoomedCellChangeEventArgs(MultiViewCell? cell)
        {
            this.Cell = cell;
        }

        #endregion

        #region Properties

        internal MultiViewCell? Cell { get; private set; }

        #endregion
    }
}


