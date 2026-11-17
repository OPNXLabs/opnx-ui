using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SkiaSharp;

namespace OPNX.UI.WPF.Controls;

/// <summary>A standalone SkiaSharp map surface with WPF input, document editing and portable persistence.</summary>
public sealed partial class Opnx2DMapControl : FrameworkElement
{
    #region Fields
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly DispatcherTimer _animation;
    private readonly Dictionary<string, SKBitmap> _images = [];
    private readonly HashSet<Map2DItem> _observed = [];
    private readonly Stack<string> _undo = [], _redo = [];
    private WriteableBitmap? _surface;
    private SKBitmap? _background;
    private string? _backgroundData;
    private double _documentWidth = 1600, _documentHeight = 1000, _zoom = 1;
    private Vector _offset;
    private bool _restoring;
    private Map2DItem? _selected;
    #endregion

    #region Constructors
    public Opnx2DMapControl()
    {
        Focusable = true;
        ClipToBounds = true;
        Items.CollectionChanged += Items_CollectionChanged;
        _animation = new DispatcherTimer(DispatcherPriority.Render, Dispatcher) { Interval = TimeSpan.FromMilliseconds(100) };
        _animation.Tick += (_, _) => InvalidateVisual();
        Loaded += (_, _) => UpdateAnimation();
        Unloaded += (_, _) => { _animation.Stop(); ReleaseImages(); _surface = null; };
        SizeChanged += (_, _) => InvalidateVisual();
    }
    #endregion

    #region Properties
    public ObservableCollection<Map2DItem> Items { get; } = [];
    public static readonly DependencyProperty IsEditModeProperty = DependencyProperty.Register(
        nameof(IsEditMode), typeof(bool), typeof(Opnx2DMapControl), new FrameworkPropertyMetadata(true, (d, _) =>
        { var map = (Opnx2DMapControl)d; map.CancelInteraction(); map.InvalidateVisual(); }));
    public bool IsEditMode { get => (bool)GetValue(IsEditModeProperty); set => SetValue(IsEditModeProperty, value); }
    public Map2DItem? SelectedItem
    {
        get => _selected;
        set
        {
            VerifyAccess();
            if (value != null && !Items.Contains(value)) throw new ArgumentException("The item does not belong to this map.", nameof(value));
            if (ReferenceEquals(_selected, value)) return;
            _selected = value;
            InvalidateVisual();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public double Zoom => _zoom;
    public double DocumentWidth => _documentWidth;
    public double DocumentHeight => _documentHeight;
    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    #endregion

    #region Events
    public event EventHandler? SelectionChanged;
    public event EventHandler? DocumentChanged;
    public event EventHandler? ViewChanged;
    public event EventHandler<Map2DItemEventArgs>? ItemClick;
    public event EventHandler<Map2DItemEventArgs>? ItemDoubleClick;
    #endregion

    #region Public Methods
    public Point ViewToDocument(Point point) => new((point.X - _offset.X) / _zoom, (point.Y - _offset.Y) / _zoom);
    public Point DocumentToView(Point point) => new(point.X * _zoom + _offset.X, point.Y * _zoom + _offset.Y);

    public void ZoomAt(double factor, Point viewPoint)
    {
        VerifyAccess();
        if (!double.IsFinite(factor) || factor <= 0) throw new ArgumentOutOfRangeException(nameof(factor));
        Point anchor = ViewToDocument(viewPoint);
        _zoom = Math.Clamp(_zoom * factor, 0.02, 32);
        _offset = new Vector(viewPoint.X - anchor.X * _zoom, viewPoint.Y - anchor.Y * _zoom);
        NotifyView();
    }

    public void FitToView()
    {
        VerifyAccess();
        if (ActualWidth <= 0 || ActualHeight <= 0) return;
        _zoom = Math.Clamp(Math.Min(Math.Max(1, ActualWidth - 48) / _documentWidth,
            Math.Max(1, ActualHeight - 48) / _documentHeight), 0.02, 32);
        CenterView();
    }

    public void ResetView() { VerifyAccess(); _zoom = 1; CenterView(); }

    public void SetBackgroundImage(byte[]? imageBytes)
    {
        VerifyAccess();
        CancelInteraction();
        using var image = imageBytes == null ? null : Decode(imageBytes);
        string before = SaveDocument();
        _backgroundData = imageBytes == null ? null : Convert.ToBase64String(imageBytes);
        _background?.Dispose();
        _background = null;
        if (image != null) { _documentWidth = image.Width; _documentHeight = image.Height; }
        Commit(before);
        FitToView();
    }

    public void AddItem(Map2DItem item)
    {
        VerifyAccess();
        ValidateItem(item);
        if (Items.Any(x => x.Id == item.Id)) throw new ArgumentException("Item IDs must be unique.", nameof(item));
        string before = SaveDocument();
        Items.Add(item);
        SelectedItem = item;
        Commit(before);
    }

    public void DeleteSelected()
    {
        VerifyAccess();
        if (SelectedItem is not { IsLocked: false } item) return;
        string before = SaveDocument();
        Items.Remove(item);
        Commit(before);
    }

    /// <summary>Groups property changes into one undo step and validates the result.</summary>
    public void UpdateSelected(Action<Map2DItem> update)
    {
        VerifyAccess();
        ArgumentNullException.ThrowIfNull(update);
        if (SelectedItem is not { } item) return;
        string before = SaveDocument();
        double x = item.X, y = item.Y, width = item.Width, height = item.Height, rotation = item.Rotation;
        bool locked = item.IsLocked;
        try
        {
            update(item); ValidateItem(item);
            if (locked && item.IsLocked && (item.X != x || item.Y != y || item.Width != width || item.Height != height || item.Rotation != rotation))
                throw new InvalidOperationException("Unlock the item before changing its position or size.");
        }
        catch { Restore(before, false); throw; }
        Commit(before);
    }

    public void MoveSelectedInOrder(bool toFront)
    {
        VerifyAccess();
        if (SelectedItem is not { IsLocked: false } item) return;
        string before = SaveDocument();
        Items.Move(Items.IndexOf(item), toFront ? Items.Count - 1 : 0);
        Commit(before);
    }

    public void SetItemBlinking(string itemId, bool enabled)
    {
        VerifyAccess();
        var item = Items.FirstOrDefault(x => x.Id == itemId) ?? throw new ArgumentException("Unknown item ID.", nameof(itemId));
        item.IsBlinking = enabled;
    }

    public string SaveDocument()
    {
        VerifyAccess();
        return JsonSerializer.Serialize(new Map2DDocument
        { Width = _documentWidth, Height = _documentHeight, BackgroundImageData = _backgroundData, Items = Items.ToList() }, JsonOptions);
    }

    public void LoadDocument(string json)
    {
        VerifyAccess();
        // Validate and decode before changing the active document.
        var document = ParseDocument(json);
        CancelInteraction();
        string before = SaveDocument();
        ApplyDocument(document);
        Commit(before);
        FitToView();
    }

    public void Undo()
    {
        VerifyAccess(); CancelInteraction();
        if (!_undo.TryPop(out string? json)) return;
        _redo.Push(SaveDocument()); Restore(json, true);
    }
    public void Redo()
    {
        VerifyAccess(); CancelInteraction();
        if (!_redo.TryPop(out string? json)) return;
        _undo.Push(SaveDocument()); Restore(json, true);
    }
    #endregion

    #region Private / Protected Methods
    private static SKBitmap Decode(byte[] bytes) => SKBitmap.Decode(bytes) ?? throw new InvalidDataException("Unsupported or damaged image.");

    private static Map2DDocument ParseDocument(string json)
    {
        var doc = JsonSerializer.Deserialize<Map2DDocument>(json) ?? throw new InvalidDataException("Map document is empty.");
        if (doc.Version != 1 || !Positive(doc.Width) || !Positive(doc.Height) || doc.Items == null)
            throw new InvalidDataException("Unsupported map document or dimensions.");
        if (doc.Items.Select(x => x?.Id).Distinct().Count() != doc.Items.Count)
            throw new InvalidDataException("Duplicate item IDs.");
        foreach (var item in doc.Items) ValidateItem(item);
        if (doc.BackgroundImageData != null)
        {
            using var image = Decode(Convert.FromBase64String(doc.BackgroundImageData));
            if (image.Width != doc.Width || image.Height != doc.Height)
                throw new InvalidDataException("Background size does not match document dimensions.");
        }
        return doc;
    }

    private static bool Positive(double value) => double.IsFinite(value) && value > 0 && value <= 1_000_000;
    private static void ValidateItem(Map2DItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (string.IsNullOrWhiteSpace(item.Id) || !Enum.IsDefined(item.Kind) || !double.IsFinite(item.X) || !double.IsFinite(item.Y) ||
            !Positive(item.Width) || !Positive(item.Height) || !double.IsFinite(item.Rotation) ||
            !double.IsFinite(item.StrokeWidth) || item.StrokeWidth is < 0 or > 1000 ||
            !double.IsFinite(item.Opacity) || item.Opacity is < 0 or > 1 || !Positive(item.FontSize) || string.IsNullOrWhiteSpace(item.FontFamily) ||
            !SKColor.TryParse(item.Fill, out _) || !SKColor.TryParse(item.Stroke, out _))
            throw new InvalidDataException("Invalid item geometry or style.");
        if (item.Kind == Map2DItemKind.Image)
        {
            if (string.IsNullOrWhiteSpace(item.ImageData)) throw new InvalidDataException("Image data is required.");
            using var image = Decode(Convert.FromBase64String(item.ImageData));
        }
        if (item.Kind == Map2DItemKind.Path)
        {
            using var path = SKPath.ParseSvgPathData(item.PathData ?? "");
            if (path == null || path.Bounds.Width <= 0 || path.Bounds.Height <= 0) throw new InvalidDataException("A nonempty SVG path is required.");
        }
    }

    private void ApplyDocument(Map2DDocument document)
    {
        _restoring = true;
        try
        {
            ReleaseImages();
            _backgroundData = document.BackgroundImageData;
            _documentWidth = document.Width; _documentHeight = document.Height;
            Items.Clear();
            foreach (var item in document.Items) Items.Add(item);
            SelectedItem = null;
        }
        finally { _restoring = false; }
        UpdateAnimation(); InvalidateVisual();
    }

    private void Restore(string json, bool notify)
    {
        string? id = SelectedItem?.Id;
        var blinking = Items.Where(x => x.IsBlinking).Select(x => x.Id).ToHashSet();
        ApplyDocument(ParseDocument(json));
        foreach (var item in Items) item.IsBlinking = blinking.Contains(item.Id);
        SelectedItem = Items.FirstOrDefault(x => x.Id == id);
        if (notify) DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Commit(string before)
    {
        if (before == SaveDocument()) return;
        _undo.Push(before); _redo.Clear();
        // Bound document snapshots, including embedded assets.
        if (_undo.Count > 40 || _undo.Sum(x => (long)x.Length) > 16_000_000)
        {
            long size = 0;
            var recent = new List<string>();
            foreach (string snapshot in _undo.Take(40))
            {
                if (recent.Count > 0 && size + snapshot.Length > 16_000_000) break;
                recent.Add(snapshot); size += snapshot.Length;
            }
            var keep = recent.AsEnumerable().Reverse().ToArray(); _undo.Clear();
            foreach (string entry in keep) _undo.Push(entry);
        }
        DocumentChanged?.Invoke(this, EventArgs.Empty);
        InvalidateVisual();
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var old in _observed.Where(x => !Items.Contains(x)).ToArray()) { old.PropertyChanged -= Item_Changed; _observed.Remove(old); }
        foreach (var item in Items) if (_observed.Add(item)) item.PropertyChanged += Item_Changed;
        if (_selected != null && !Items.Contains(_selected)) SelectedItem = null;
        PruneImages(); UpdateAnimation(); InvalidateVisual();
    }

    private void Item_Changed(object? sender, PropertyChangedEventArgs e)
    {
        VerifyAccess();
        if (e.PropertyName == nameof(Map2DItem.ImageData)) PruneImages();
        if (e.PropertyName == nameof(Map2DItem.IsBlinking)) UpdateAnimation();
        InvalidateVisual();
    }

    private void UpdateAnimation()
    {
        if (IsLoaded && !_restoring && Items.Any(x => x.IsBlinking)) _animation.Start(); else _animation.Stop();
    }
    private void PruneImages()
    {
        var active = Items.Where(x => x.ImageData != null).Select(x => x.ImageData!).ToHashSet();
        foreach (string key in _images.Keys.Where(x => !active.Contains(x)).ToArray()) { _images[key].Dispose(); _images.Remove(key); }
    }
    private void ReleaseImages()
    {
        _background?.Dispose(); _background = null;
        foreach (var image in _images.Values) image.Dispose();
        _images.Clear();
    }
    private void CenterView()
    {
        _offset = new Vector((ActualWidth - _documentWidth * _zoom) / 2, (ActualHeight - _documentHeight * _zoom) / 2);
        NotifyView();
    }
    private void NotifyView() { InvalidateVisual(); ViewChanged?.Invoke(this, EventArgs.Empty); }
    #endregion
}
