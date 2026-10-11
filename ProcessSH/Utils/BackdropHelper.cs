using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using ProcessSH.Models;

namespace ProcessSH.Utils;

/// <summary>
/// 窗口背景材质应用器。
/// - Mica / Mica Alt：Win11 22000+
/// - Acrylic：Win11 22000+ 完整支持；Win10 部分
/// - 不支持时降级为纯色（SystemBackdrop = null）
/// </summary>
public static class BackdropHelper
{
    public static bool IsMicaSupported => MicaController.IsSupported();

    public static bool IsAcrylicSupported => DesktopAcrylicController.IsSupported();

    public static void Apply(Window window, WindowMaterial material)
    {
        if (window == null) return;

        try
        {
            switch (material)
            {
                case WindowMaterial.Mica:
                    if (IsMicaSupported)
                        window.SystemBackdrop = new MicaBackdrop();
                    else if (IsAcrylicSupported)
                        window.SystemBackdrop = new DesktopAcrylicBackdrop();
                    else
                        window.SystemBackdrop = null;
                    break;

                case WindowMaterial.MicaAlt:
                    if (IsMicaSupported)
                        window.SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
                    else if (IsAcrylicSupported)
                        window.SystemBackdrop = new DesktopAcrylicBackdrop();
                    else
                        window.SystemBackdrop = null;
                    break;

                case WindowMaterial.Acrylic:
                    if (IsAcrylicSupported)
                        window.SystemBackdrop = new DesktopAcrylicBackdrop();
                    else
                        window.SystemBackdrop = null;
                    break;

                case WindowMaterial.None:
                default:
                    window.SystemBackdrop = null;
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[BackdropHelper] 应用材质失败: {material} — {ex.Message}");
            try { window.SystemBackdrop = null; } catch { }
        }
    }

    public static void ApplyFromSettings(Window window)
    {
        Apply(window, AppSettings.Current.BackgroundMaterial);
    }
}