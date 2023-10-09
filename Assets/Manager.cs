using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using uWindowCapture;
using WindowsDisplayAPI;
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

    private readonly List<UwcWindowTexture> _windows = new();

    private void Awake()
    {
        if (_manager != null)
        {
            Destroy(gameObject);
        }

        _manager = this;
    }

    private void Update()
    {
        Display[] displays = Display.GetDisplays().OrderByDescending(d => d.DisplayName).ToArray();
        
        for (int i = 0; i < displays.Length; i++)
        {
            CreateWindow(i);
        }

        while (_windows.Count > displays.Length)
        {
            Destroy(_windows[^1].gameObject);
            _windows.Remove(_windows[^1]);
        }

        for (int i = 0; i < _windows.Count; i++)
        {
            _windows[i].transform.position = new(displays[i].CurrentSetting.Position.X / (1000 * scalePer1000Pixel), -displays[i].CurrentSetting.Position.Y / (1000 * scalePer1000Pixel), 0);
        }
    }

    private void CreateWindow(int index)
    {
        if (_windows.Count <= index)
        {
            _windows.Add(Instantiate(windowPrefab));
            _windows[index].name = $"Desktop {index}";
            _windows[index].type = WindowTextureType.Desktop;
        }
        
        _windows[index].desktopIndex = index;
        _windows[index].scalePer1000Pixel = scalePer1000Pixel;
    }
}