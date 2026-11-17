namespace OPNX.UI.WPF.Controls;

/// <summary>Portable 2D map data. All dimensions use document pixels, independent of view zoom and WPF DPI.</summary>
public sealed class Map2DDocument
{
    #region Properties
    public int Version { get; set; } = 1;
    public double Width { get; set; } = 1600;
    public double Height { get; set; } = 1000;
    public string? BackgroundImageData { get; set; }
    /// <summary>Back-to-front drawing order.</summary>
    public List<Map2DItem> Items { get; set; } = [];
    #endregion
}

public sealed class Map2DItemEventArgs(Map2DItem item, System.Windows.Point position, int clickCount) : EventArgs
{
    #region Properties
    public Map2DItem Item { get; } = item;
    public System.Windows.Point Position { get; } = position;
    public int ClickCount { get; } = clickCount;
    #endregion
}
