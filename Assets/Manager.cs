using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using uWindowCapture;
using Display = WindowsDisplayAPI.Display;

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

    private WindowContainer[] _windows;

    private DisplayData[] _data;

    private int2 _offset;

    private void Awake()
    {
        if (_manager != null)
        {
            Destroy(gameObject);
        }

        _manager = this;
        
        Display[] displays = Display.GetDisplays().OrderByDescending(d => d.DisplayName).ToArray();

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        _windows = new WindowContainer[displays.Length];
        
        for (int i = 0; i < displays.Length; i++)
        {
            _windows[i] = new(Instantiate(windowPrefab))
            {
                Window =
                {
                    name = "Desktop Display",
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
                Set = false
            };

            if (displays[i].CurrentSetting.Position.X < minX)
            {
                minX = displays[i].CurrentSetting.Position.X;
            }

            if (displays[i].CurrentSetting.Position.X > maxX)
            {
                maxX = displays[i].CurrentSetting.Position.X;
            }
            
            if (displays[i].CurrentSetting.Position.Y < minY)
            {
                minY = displays[i].CurrentSetting.Position.Y;
            }

            if (displays[i].CurrentSetting.Position.Y > maxY)
            {
                maxY = displays[i].CurrentSetting.Position.Y;
            }
        }

        _offset = new((maxX + minX) / 2, (maxY + minY) / 2);

        _data = new DisplayData[displays.Length];
        for (int i = 0; i < _data.Length; i++)
        {
            _data[i] = new(displays[i].DisplayName, displays[i].CurrentSetting.Position.X, displays[i].CurrentSetting.Position.Y);
        }
        
        UwcManager.instance.debugModeFromInspector = DebugMode.None;
    }

    private void Update()
    {
        for (int i = 0; i < _windows.Length; i++)
        {
            _windows[i].Window.desktopIndex = i;
            _windows[i].Window.scalePer1000Pixel = scalePer1000Pixel;
            _windows[i].Set = false;
        }

        float scale = 1000 * scalePer1000Pixel;
        
        for (int i = 0; i < _windows.Length; i++)
        {
            WindowContainer match = _windows.Where(w => !w.Set).OrderByDescending(w => w.Name == _data[i].Name).First();
            match.Set = true;
            match.Window.transform.position = new((_data[i].X - _offset.x) / scale, -(_data[i].Y - _offset.y) / scale, 0);
        }
    }

    private class WindowContainer
    {
        public readonly UwcWindowTexture Window;

        public bool Set;

        public string Name => Window == null || Window.window == null ? null : Window.window.title;

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