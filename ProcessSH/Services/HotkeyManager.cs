using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace ProcessSH.Services;

/// <summary>
/// 全局快捷键管理器。使用 Win32 RegisterHotKey 实现。
/// </summary>
public sealed class HotkeyManager : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID = 0x0001;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private const int GWLP_WNDPROC = -4;

    private readonly IntPtr _hwnd;
    private IntPtr _originalWndProc;
    private WndProcDelegate? _wndProcDelegate;
    private Action? _callback;
    private bool _registered;

    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;

    public HotkeyManager(Window window)
    {
        _hwnd = WindowNative.GetWindowHandle(window);
        _wndProcDelegate = WndProc;
        _originalWndProc = SetWindowLongPtr(_hwnd, GWLP_WNDPROC,
            Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
    }

    /// <summary>注册或重新注册全局快捷键</summary>
    public bool Register(uint modifiers, uint virtualKey, Action callback)
    {
        _callback = callback;

        // 如果已注册，先注销
        if (_registered)
        {
            UnregisterHotKey(_hwnd, HOTKEY_ID);
            _registered = false;
        }

        if (!RegisterHotKey(_hwnd, HOTKEY_ID, modifiers, virtualKey))
            return false;

        _registered = true;
        return true;
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            _callback?.Invoke();
            return IntPtr.Zero;
        }

        return CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
    }

    /// <summary>把 "Ctrl+Win+R" 这种字符串解析成修饰键和虚拟键码</summary>
    public static (uint modifiers, uint vk)? Parse(string hotkey)
    {
        if (string.IsNullOrWhiteSpace(hotkey)) return null;

        uint modifiers = 0;
        uint? vk = null;

        var parts = hotkey.Split('+', StringSplitOptions.RemoveEmptyEntries);
        foreach (var raw in parts)
        {
            var part = raw.Trim();

            if (part.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("Control", StringComparison.OrdinalIgnoreCase))
                modifiers |= MOD_CONTROL;
            else if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                modifiers |= MOD_ALT;
            else if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                modifiers |= MOD_SHIFT;
            else if (part.Equals("Win", StringComparison.OrdinalIgnoreCase))
                modifiers |= MOD_WIN;
            else
            {
                // 主键：单个字母或数字
                if (part.Length == 1)
                {
                    var c = char.ToUpperInvariant(part[0]);
                    if (c >= 'A' && c <= 'Z')
                        vk = (uint)c;
                    else if (c >= '0' && c <= '9')
                        vk = (uint)c;
                }
            }
        }

        if (vk == null || modifiers == 0) return null;
        return (modifiers, vk.Value);
    }

    public void Dispose()
    {
        if (_registered)
        {
            UnregisterHotKey(_hwnd, HOTKEY_ID);
            _registered = false;
        }

        if (_originalWndProc != IntPtr.Zero)
        {
            SetWindowLongPtr(_hwnd, GWLP_WNDPROC, _originalWndProc);
        }
    }
}