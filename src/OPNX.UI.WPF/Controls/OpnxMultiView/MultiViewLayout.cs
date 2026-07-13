using System.Windows;

namespace OPNX.UI.WPF.Controls
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
        public string? EntityTypeName { get; set; }
        public int EntityID { get; set; }
        Guid SyncId { get; set; }
        Rect RectForCanvas { get; set; }
    }

    public class MultiViewCellLayout : IMultiViewCellLayout
    {
        public string? EntityTypeName { get; set; }
        public int EntityID { get; set; }
        public Guid SyncId { get; set; }
        public Rect RectForCanvas { get; set; }
    }
}


