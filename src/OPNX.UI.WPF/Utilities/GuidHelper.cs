using OPNX.UI.WPF.Controls;
using OPNX.UI.WPF.Infrastructure;
using System.Windows;

namespace OPNX.UI.WPF.Utilities
{
    /// <summary>
    /// UIElement Guid 留ㅽ븨??議고쉶?섍퀬 愿由ы븯???꾩슦誘?
    /// </summary>
    public static class GuidHelper
    {
        private static readonly Dictionary<Guid, List<UIElement>?> _originGuidMap = [];
        private static readonly Dictionary<Guid, List<UIElement>?> _syncGuidMap = [];

        public static void ClearMappings()
        {
            _originGuidMap.Clear();
            _syncGuidMap.Clear();
        }

        public static Guid GetOriginId(UIElement element)
        {
            Guid result = Guid.Empty;

            if (element is OpnxControl control)
            {
                result = control.OriginId;
            }

            return result;
        }

        public static Guid GetSyncId(UIElement element)
        {
            Guid result = Guid.Empty;

            if (element is OpnxControl control)
            {
                result = control.SyncId;
            }

            return result;
        }

        public static UIElement? FindBySyncId(Guid syncId)
        {
            if (_syncGuidMap.TryGetValue(syncId, out List<UIElement>? elements) &&
                elements is { Count: > 0 })
            {
                return elements[0];
            }

            return null;
        }

        public static IReadOnlyList<UIElement> FindAllBySyncId(Guid syncId)
        {
            if (_syncGuidMap.TryGetValue(syncId, out var elements) && elements != null)
            {
                return elements;
            }

            return [];
        }

        public static void AddOriginMapping(Guid originId, UIElement? element)
        {
            if (element is not OpnxControl)
            {
                return;
            }

            if (!_originGuidMap.TryGetValue(originId, out var elements) || elements == null)
            {
                elements = [];
                _originGuidMap[originId] = elements;
            }

            elements.Add(element);
        }

        public static void AddSyncMapping(Guid syncId, UIElement? element)
        {
            if (element is null || element is IInternalVisualElement)
            {
                return;
            }

            if (!_syncGuidMap.TryGetValue(syncId, out var elements) || elements == null)
            {
                elements = [];
                _syncGuidMap[syncId] = elements;
            }

            elements.Add(element);
        }

        public static void RemoveOriginMapping(Guid originId, UIElement? element)
        {
            if (element is null || element is IInternalVisualElement)
            {
                return;
            }

            if (!_originGuidMap.TryGetValue(originId, out var elements) || elements == null)
            {
                _originGuidMap.Remove(originId);
                return;
            }

            elements.Remove(element);

            if (elements.Count == 0)
            {
                _originGuidMap.Remove(originId);
            }
        }

        public static void RemoveSyncMapping(Guid syncId, UIElement? element)
        {
            if (element is null || element is IInternalVisualElement)
            {
                return;
            }

            if (!_syncGuidMap.TryGetValue(syncId, out var elements) || elements == null)
            {
                _syncGuidMap.Remove(syncId);
                return;
            }

            elements.Remove(element);

            if (elements.Count == 0)
            {
                _syncGuidMap.Remove(syncId);
            }
        }
    }
}


