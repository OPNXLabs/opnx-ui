using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OPNX.UI.WPF.Controls;

public enum Map2DItemKind { Rectangle, Ellipse, Line, Polygon, Image, Text, Path }

/// <summary>One item in document coordinates. X/Y are the unrotated top-left; rotation is clockwise about its center.</summary>
public sealed class Map2DItem : INotifyPropertyChanged
{
    #region Fields
    private string _id = Guid.NewGuid().ToString("N"), _text = "Text", _fill = "#5538BDF8", _stroke = "#38BDF8", _fontFamily = "Malgun Gothic";
    private string? _imageData, _pathData, _toolTip;
    private double _x, _y, _width = 100, _height = 70, _rotation, _strokeWidth = 2, _opacity = 1, _fontSize = 24;
    private bool _isLocked, _isBlinking;
    private Map2DItemKind _kind;

    #endregion

    #region Properties
    public string Id { get => _id; set => Set(ref _id, value); }
    public Map2DItemKind Kind { get => _kind; set => Set(ref _kind, value); }
    public double X { get => _x; set => Set(ref _x, value); }
    public double Y { get => _y; set => Set(ref _y, value); }
    public double Width { get => _width; set => Set(ref _width, value); }
    public double Height { get => _height; set => Set(ref _height, value); }
    public double Rotation { get => _rotation; set => Set(ref _rotation, value); }
    public string Fill { get => _fill; set => Set(ref _fill, value); }
    public string Stroke { get => _stroke; set => Set(ref _stroke, value); }
    public double StrokeWidth { get => _strokeWidth; set => Set(ref _strokeWidth, value); }
    public double Opacity { get => _opacity; set => Set(ref _opacity, value); }
    public string Text { get => _text; set => Set(ref _text, value); }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    public string FontFamily { get => _fontFamily; set => Set(ref _fontFamily, value); }
    /// <summary>Base64 encoded image bytes, embedded in the document for portability.</summary>
    public string? ImageData { get => _imageData; set => Set(ref _imageData, value); }
    /// <summary>SVG path data, fitted into this item's bounds. No SVG document or scripts are executed.</summary>
    public string? PathData { get => _pathData; set => Set(ref _pathData, value); }
    public string? ToolTip { get => _toolTip; set => Set(ref _toolTip, value); }
    public bool IsLocked { get => _isLocked; set => Set(ref _isLocked, value); }
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsBlinking { get => _isBlinking; set => Set(ref _isBlinking, value); }
    #endregion

    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Private / Protected Methods
    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion
}
