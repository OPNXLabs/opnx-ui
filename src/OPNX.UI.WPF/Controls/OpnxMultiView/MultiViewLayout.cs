using System.Windows;

namespace OPNX.UI.WPF.Controls.OpnxMultiView
{
    public interface IMultiViewLayout
    {
        List<MultiViewCellLayout> CellLayouts { get; set; }
    }
    public class MultiViewLayout : IMultiViewLayout
    {
        public List<MultiViewCellLayout> CellLayouts { get; set; } = [];
    }

    public interface IMultiViewCellLayout
    {
        Guid SyncId { get; set; }
        Rect RectForCanvas { get; set; }
    }

    public class MultiViewCellLayout : IMultiViewCellLayout
    {
        public Guid SyncId { get; set; }
        public Rect RectForCanvas { get; set; }
    }
}

