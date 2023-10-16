using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using uWindowCapture;
using Display = WindowsDisplayAPI.Display;

[RequireComponent(typeof(UwcManager))]
public class Manager : MonoBehaviour
{
    private static Manager _manager;

    [Tooltip("Prefab for the monitors.")]
    [SerializeField]
    private UwcWindowTexture windowPrefab;

    [Tooltip("Scale per 1000 pixels.")]
    [Min(float.Epsilon)]
    [SerializeField]
    private float scalePer1000Pixel = 1;

    [Tooltip("The height to place the screens at.")]
    [Min(0)]
    [SerializeField]
    private float height;

    [Tooltip("The distance to place the screens at.")]
    [Min(0)]
    [SerializeField]
    private float distance;

    private WindowContainer[] _windows;

    private DisplayData[] _data;

    private int2 _offset;

    private void Start()
    {
        if (_manager != null)
        {
            if (_manager != this)
            {
                Destroy(gameObject);
            }
            
            return;
        }

        _manager = this;

        if (GetComponent<UwcManager>() == null)
        {
            gameObject.AddComponent<UwcManager>();
        }
        
        UwcManager.instance.debugModeFromInspector = DebugMode.None;
        UwcManager.instance.windowTitlesUpdateTiming = WindowTitlesUpdateTiming.Manual;

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;
        
        Display[] displays = Display.GetDisplays().ToArray();

        _windows = new WindowContainer[displays.Length];
        _data = new DisplayData[displays.Length];
        
        for (int i = 0; i < displays.Length; i++)
        {
            _data[i] = new(displays[i].DisplayName, displays[i].CurrentSetting.Position.X, displays[i].CurrentSetting.Position.Y);
            
            _windows[i] = new(Instantiate(windowPrefab))
            {
                Window =
                {
                    name = _data[i].Name,
                    type = WindowTextureType.Desktop,
                    updateTitle = false,
                    createChildWindows = false,
                    altTabWindow = false,
                    captureMode = CaptureMode.WindowsGraphicsCapture,
                    searchTiming = WindowSearchTiming.OnlyWhenParameterChanged,
                    capturePriority = CapturePriority.Auto,
                    desktopIndex = i,
                    scalePer1000Pixel = scalePer1000Pixel
                },
                Set = false,
                Data = i
            };

            Collider c = _windows[i].Window.GetComponent<Collider>();
            if (c != null)
            {
                Destroy(c);
            }

            if (_data[i].X < minX)
            {
                minX = _data[i].X;
            }

            if (_data[i].X > maxX)
            {
                maxX = _data[i].X;
            }
            
            if (_data[i].Y < minY)
            {
                minY = _data[i].Y;
            }

            if (_data[i].Y > maxY)
            {
                maxY = _data[i].Y;
            }
        }

        _offset = new((maxX + minX) / 2, (maxY + minY) / 2);
    }

    private void Update()
    {
        bool update = false;
        
        for (int i = 0; i < _windows.Length; i++)
        {
            _windows[i].Window.desktopIndex = i;
            _windows[i].Window.scalePer1000Pixel = scalePer1000Pixel;
            _windows[i].Set = false;
            if (_windows[i].Updated)
            {
                update = true;
            }
        }

        if (update)
        {
            for (int i = 0; i < _windows.Length; i++)
            {
                WindowContainer match = _windows.Where(w => !w.Set).OrderByDescending(w => w.Name == _data[i].Name).First();
                match.Set = true;
                match.Data = i;
                match.Window.gameObject.name = match.Name;
            }
        }

        float scale = 1000 * scalePer1000Pixel;

        float2 offset = new(_offset.x * scalePer1000Pixel, _offset.y * scalePer1000Pixel);
        
        for (int i = 0; i < _windows.Length; i++)
        {
            float x = (_data[_windows[i].Data].X * scalePer1000Pixel - offset.x) / scale;
            float y = -(_data[_windows[i].Data].Y * scalePer1000Pixel - offset.y) / scale;
            if (_windows[i].Window.window != null)
            {
                x /= _windows[i].Window.window.width / (_windows[i].Window.transform.localScale.x * 1000f);
                y /= _windows[i].Window.window.height / (_windows[i].Window.transform.localScale.y * 1000f);
            }
            _windows[i].Window.transform.position = new(x, y + height, 0 + distance);
        }
    }

    private class WindowContainer
    {
        public readonly UwcWindowTexture Window;

        public bool Set;

        public int Data;

        public string Name => Window == null || Window.window == null ? null : Window.window.title;

        public bool Updated => Window != null && Name != Window.gameObject.name;

        public WindowContainer(UwcWindowTexture window)
        {
            Window = window;
        }
    }

    private class DisplayData
    {
        public readonly string Name;

        public readonly int X;

        public readonly int Y;

        public DisplayData(string name, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
        }
    }
}