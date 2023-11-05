using System.Linq;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine;
using uWindowCapture;
using Display = WindowsDisplayAPI.Display;

/// <summary>
/// Control the creation and placement of virtual monitors.
/// </summary>
[RequireComponent(typeof(UwcManager))]
public class Manager : MonoBehaviour
{
    /// <summary>
    /// There should only be one manager.
    /// </summary>
    private static Manager _manager;

    /// <summary>
    /// The prefab of the monitors.
    /// </summary>
    [Tooltip("Prefab for the monitors.")]
    [SerializeField]
    private UwcWindowTexture windowPrefab;

    /// <summary>
    /// The scale per 1000 pixels to size for Unity units.
    /// </summary>
    [Tooltip("Scale per 1000 pixels.")]
    [Min(float.Epsilon)]
    [SerializeField]
    private float scalePer1000Pixel = 1;

    /// <summary>
    /// How height the center of the screens should be positioned at.
    /// </summary>
    [Tooltip("The height to place the screens at.")]
    [Min(0)]
    [SerializeField]
    private float height;

    /// <summary>
    /// How far along the Z axis the screens should be positioned at.
    /// </summary>
    [Tooltip("The distance to place the screens at.")]
    [Min(0)]
    [SerializeField]
    private float distance;

    /// <summary>
    /// The container for all the monitors in Unity.
    /// </summary>
    private WindowContainer[] _windows;

    /// <summary>
    /// The captured display data.
    /// </summary>
    private DisplayData[] _data;

    /// <summary>
    /// The offset to shift monitors based upon their overall resolution to keep them centered.
    /// </summary>
    private int2 _offset;

    /// <summary>
    /// The height the virtual camera begins it to initially base the height and distance based off of.
    /// In the future this could be useful for some kind of "reset" or "recenter" function.
    /// </summary>
    private float _cameraHeight;

    private void Start()
    {
        // Ensure there is only one manager.
        if (_manager != null)
        {
            if (_manager != this)
            {
                Destroy(gameObject);
            }
            
            return;
        }

        _manager = this;

        // Zero out the origin and get the height the headset is at.
        XROrigin xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin != null)
        {
            xrOrigin.transform.position = Vector3.zero;
            _cameraHeight = xrOrigin.CameraYOffset;
        }

        // Ensure there is a window manager to control the rendering of the screens.
        if (GetComponent<UwcManager>() == null)
        {
            gameObject.AddComponent<UwcManager>();
        }
        
        // Set them to the most performant modes as we don't need these features since titles will not change.
        UwcManager.instance.debugModeFromInspector = DebugMode.None;
        UwcManager.instance.windowTitlesUpdateTiming = WindowTitlesUpdateTiming.Manual;

        // The lower and upper bound pixel values.
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;
        
        // The Unity library for getting monitors doesn't get their position data in relation to each other.
        // Additionally, no built-in Unity methods get this data we need either.
        // We need this data to position the monitors as they appear in Windows settings.
        // WindowsDisplayAPI can get this missing data for us.
        // This library is a wrapper around Windows Display APIs for C# but has a terrible overhead cost of calling.
        // For instance, trying to calling this in Update will drop your frames like crazy.
        // So, call it only once in Start() and cache the values in our "DisplayData" class.
        // In the future, this logic could perhaps be moved to a method call to update to a new monitor layout.
        Display[] displays = Display.GetDisplays().ToArray();

        // Store the number of monitors and the cached data.
        _windows = new WindowContainer[displays.Length];
        _data = new DisplayData[displays.Length];
        
        // Create every new monitor.
        for (int i = 0; i < displays.Length; i++)
        {
            // Cache the data for performance.
            _data[i] = new(displays[i].DisplayName, displays[i].CurrentSetting.Position.X, displays[i].CurrentSetting.Position.Y);
            
            // Create the monitor in Unity.
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

            // We don't need colliders so remove them, maybe in the future they could have a use.
            Collider c = _windows[i].Window.GetComponent<Collider>();
            if (c != null)
            {
                Destroy(c);
            }

            // See if this is any min or max value.
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

        // Set the offset so monitors are centered.
        _offset = new((maxX + minX) / 2, (maxY + minY) / 2);
    }

    private void Update()
    {
        // During the initial setup, the monitors may not be fully populated and since the order is not deterministic,
        // it may need to get updated here and correct which monitor ID goes to which location.
        bool update = false;
        
        // Loop through every monitor.
        for (int i = 0; i < _windows.Length; i++)
        {
            // Ensure the index is correct.
            _windows[i].Window.desktopIndex = i;
            
            // Set the scale.
            _windows[i].Window.scalePer1000Pixel = scalePer1000Pixel;
            
            // This is used if we do need to update the monitors, and once one is "set" in place with the correct
            // matching data, it won't be used again. This is mainly a failsafe and should never actually matter.
            _windows[i].Set = false;
            
            // If the name of the GameObject does not match the monitor its trying to render's name, the monitors are
            // not in the correct places so flag that they need to be updated.
            if (_windows[i].updated)
            {
                update = true;
            }
        }

        // Update the monitors if they need to be.
        if (update)
        {
            // Go through all the monitors.
            for (int i = 0; i < _windows.Length; i++)
            {
                // Find the matching monitor in the cached data we got in Start().
                // This is done by first doing the failsafe of removing monitors that are not yet set, then getting the
                // monitor with the matching name.
                WindowContainer match = _windows.Where(w => !w.Set).OrderByDescending(w => w.name == _data[i].Name).First();
                
                // Flag that it was set.
                match.Set = true;
                
                // Link the virtual monitor index.
                match.Data = i;
                
                // Match the names so it should not fail the needing update check above next time.
                match.Window.gameObject.name = match.name;
            }
        }

        // Convert the Unity scale to pixels.
        float scale = 1000 * scalePer1000Pixel;

        // Get the Unity scale offset from the monitor pixels.
        float2 offset = new(_offset.x * scalePer1000Pixel, _offset.y * scalePer1000Pixel);
        
        // Position every monitor.
        for (int i = 0; i < _windows.Length; i++)
        {
            // Get the X and Y positions.
            float x = (_data[_windows[i].Data].X * scalePer1000Pixel - offset.x) / scale;
            float y = -(_data[_windows[i].Data].Y * scalePer1000Pixel - offset.y) / scale;
            
            // Check that the window object from the library we are using is set as a failsafe.
            if (_windows[i].Window.window != null)
            {
                x /= _windows[i].Window.window.width / (_windows[i].Window.transform.localScale.x * 1000f);
                y /= _windows[i].Window.window.height / (_windows[i].Window.transform.localScale.y * 1000f);
            }
            
            // Position the monitor in the Unity scene.
            _windows[i].Window.transform.position = new(x, y + height + _cameraHeight, 0 + distance);
        }
    }

    /// <summary>
    /// Helper class to store the virtual monitors.
    /// </summary>
    private class WindowContainer
    {
        /// <summary>
        /// The object displaying the monitor itself.
        /// </summary>
        public readonly UwcWindowTexture Window;

        /// <summary>
        /// If this monitor has been set during a monitor reassignment loop.
        /// </summary>
        public bool Set;

        /// <summary>
        /// The corresponding data object this monitor is linked to.
        /// </summary>
        public int Data;

        /// <summary>
        /// The name of the monitor that this is displaying.
        /// </summary>
        public string name => Window == null || Window.window == null ? null : Window.window.title;

        /// <summary>
        /// If the saved name (name of the GameObject) does not match the current monitor name that is rendering,
        /// an update has occured that needs to be reassigned for.
        /// </summary>
        public bool updated => Window != null && name != Window.gameObject.name;

        /// <summary>
        /// Constructor to assign the monitor displaying component.
        /// </summary>
        /// <param name="window">The monitor displaying component.</param>
        public WindowContainer(UwcWindowTexture window)
        {
            Window = window;
        }
    }

    /// <summary>
    /// Helper class to store the cached display data from WindowsDisplayAPI.
    /// </summary>
    private class DisplayData
    {
        /// <summary>
        /// The name of the monitor.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The X center of the monitor in pixels in relation to the main monitor.
        /// </summary>
        public readonly int X;

        /// <summary>
        /// The Y center of the monitor in pixels in relation to the main monitor.
        /// </summary>
        public readonly int Y;

        /// <summary>
        /// Constructor to store the data.
        /// </summary>
        /// <param name="name">The name of the monitor.</param>
        /// <param name="x">The X center of the monitor in pixels in relation to the main monitor.</param>
        /// <param name="y">The Y center of the monitor in pixels in relation to the main monitor.</param>
        public DisplayData(string name, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
        }
    }
}