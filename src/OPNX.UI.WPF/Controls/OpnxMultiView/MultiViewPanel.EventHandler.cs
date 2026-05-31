using System.Windows;

namespace OPNX.UI.WPF.Controls
{
    /// <summary>
    /// ControlledCellChangedEventArgs class.
    /// </summary>
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

    /// <summary>
    /// CellAddedArgs class.
    /// </summary>
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

    /// <summary>
    /// Cell Drop Completed Args class.
    /// </summary>
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

    /// <summary>
    /// CellRemovedArgs class.
    /// </summary>
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

    /// <summary>
    /// SelectionChangedArgs class.
    /// </summary>
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

    /// <summary>
    /// SelectionEnableChangedArgs class.
    /// </summary>
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

    /// <summary>
    /// ZoomedCellChangeEventArgs class.
    /// </summary>
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

    ///// <summary>
    ///// DeserializeFavoriteDataEventArgs class.
    ///// </summary>
    //internal class DeserializeFavoriteDataEventArgs : EventArgs
    //{
    //    #region Constructors and Destructors

    //    internal DeserializeFavoriteDataEventArgs(MultiGridDataForFavorite favoriteData, List<NewMultiGridElementInfo> favoriteNewElementInfoList,
    //        List<CameraInformation> cameraInfos)
    //    {
    //        this.FavoriteData = favoriteData;
    //        this.FavoriteNewElementInfoList = favoriteNewElementInfoList;
    //        this.FavoriteCameraInfoList = cameraInfos;
    //    }

    //    #endregion

    //    #region Properties

    //    internal MultiGridDataForFavorite FavoriteData { get; private set; }
    //    internal List<NewMultiGridElementInfo> FavoriteNewElementInfoList { get; private set; }
    //    internal List<CameraInformation> FavoriteCameraInfoList { get; private set; }

    //    #endregion
    //}
}


