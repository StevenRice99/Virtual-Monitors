using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using uWindowCapture;
using Debug = UnityEngine.Debug;

public class Manager : MonoBehaviour
{
    private static readonly string OS = Environment.Is64BitOperatingSystem ? "deviceinstaller64.exe" : "deviceinstaller32.exe";
    private static readonly string Dir = Path.Combine(Application.dataPath, "..", "usbmmidd_v2");

    private static Manager _manager;

    [Tooltip("The number of screens there can be.")]
    [Min(1)]
    [SerializeField]
    private int screens = 2;

    [Tooltip("Prefab for the monitors.")]
    [SerializeField]
    private UwcWindowTexture windowPrefab;

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
        if (_windows.Count < screens)
        {
            AddScreens();
        }
        else if (_windows.Count > screens)
        {
            RemoveScreens();
        }

        for (int i = 0; i < _windows.Count; i++)
        {
            _windows[i].desktopIndex = i;
        }
    }

    private void AddScreens()
    {
#if UNITY_EDITOR
        const int initial = 2;
#else
        int initial = Display.displays.Length;
#endif
        for (int i = 0; i < initial; i++)
        {
            CreateWindow(i);
        }

        int add = screens - initial;

        for (int i = 0; i < add; i++)
        {
            if (!ExecuteCommand(Path.Combine(Dir, $"{OS} enableidd 1")))
            {
                Debug.LogError("Failed to create a virtual monitor.");
                return;
            }
        
            CreateWindow(initial + i);
        }
    }

    private void RemoveScreens()
    {
        for (int i = _windows.Count; i > screens; i--)
        {
            Destroy(_windows[i - 1].gameObject);
            _windows.RemoveAt(i - 1);
        }
    }

    private void CreateWindow(int index)
    {
        if (_windows.Count > index)
        {
            return;
        }
        
        UwcWindowTexture window = Instantiate(windowPrefab);
        window.name = $"Desktop {index}";
        window.type = WindowTextureType.Desktop;
        window.desktopIndex = index;
        _windows.Add(window);
    }

    private void OnDestroy()
    {
        if (_manager != this)
        {
            return;
        }
        
        for (int i = 0; i < 4; i++)
        {
            if (!ExecuteCommand(Path.Combine(Dir, $"{OS} enableidd 0")))
            {
                break;
            }
        }
    }

    private static bool ExecuteCommand(string command)
    {
        Process process = Process.Start(new ProcessStartInfo("cmd.exe", $"/c {command}")
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        });
        
        if (process == null)
        {
            return false;
        }

        process.WaitForExit();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        int code = process.ExitCode;
        process.Close();

        if (!string.IsNullOrEmpty(output))
        {
            Debug.Log($"Exit code: {code}\n{output}");
        }

        if (string.IsNullOrEmpty(error))
        {
            return true;
        }

        Debug.LogError($"Exit code: {code}\n{error}");
        return false;
    }
}