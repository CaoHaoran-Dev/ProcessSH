using System.Runtime.InteropServices;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using ProcessSH.Models;
using ProcessSH.Services;
using ProcessSH.Utils;
using Windows.Storage.Pickers;
using Windows.System;

namespace ProcessSH.Views;

public sealed partial class SettingsPage : Page
{
    private enum Section
    {
        General,
        Appearance,
        Paths,
        Aliases,
        Startup,
        Updates,
    }

    private Section _current = Section.General;
    private readonly List<(Section section, NavigationViewItem item)> _navItems = new();

    private bool _loading = true;

    private bool _recordingHotkey;
    private TextBox? _hotkeyBox;
    private TextBlock? _hotkeyStatusText;

    public SettingsPage()
    {
        InitializeComponent();
        BuildNav();
        ApplyLocalization();
        NavigateTo(Section.General);
        Localization.LanguageChanged += ApplyLocalization;
        Unloaded += OnUnloaded;
        _loading = false;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Localization.LanguageChanged -= ApplyLocalization;
        StopRecording();
    }

    // ─────────────────────────────────────────────
    // Win32 修饰键状态
    // ─────────────────────────────────────────────

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    private const int VK_SHIFT = 0x10;
    private const int VK_CONTROL = 0x11;
    private const int VK_MENU = 0x12;

    private static bool IsDown(int vk) => (GetKeyState(vk) & 0x8000) != 0;

    // ─────────────────────────────────────────────
    // 导航
    // ─────────────────────────────────────────────

    private void BuildNav()
    {
        _navItems.Clear();
        NavPanel.Children.Clear();

        AddNavItem(Section.General,    "settings.category.general",    "\xE713");
        AddNavItem(Section.Appearance, "settings.category.appearance", "\xE790");
        AddNavItem(Section.Paths,      "settings.category.paths",      "\xE8B7");
        AddNavItem(Section.Aliases,    "settings.category.aliases",    "\xE8F1");
        AddNavItem(Section.Startup,    "settings.category.startup",    "\xE7E8");
        AddNavItem(Section.Updates,    "settings.category.updates",    "\xE895");
    }

    private void AddNavItem(Section section, string titleKey, string glyph)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        panel.Children.Add(new FontIcon
        {
            Glyph = glyph,
            FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
            FontSize = 14,
        });
        panel.Children.Add(new TextBlock
        {
            Text = Localization.Get(titleKey),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
        });

        var item = new NavigationViewItem
        {
            Content = panel,
            Tag = section,
        };

        item.Tapped += (_, _) => NavigateTo(section);
        NavPanel.Children.Add(item);

        _navItems.Add((section, item));
    }

    private void NavigateTo(Section section)
    {
        _current = section;

        foreach (var (s, item) in _navItems)
            item.IsSelected = s == section;

        RenderContent();
    }

    private void ApplyLocalization()
    {
        var prev = _current;
        _loading = true;
        BuildNav();
        _current = prev;
        foreach (var (s, item) in _navItems)
            item.IsSelected = s == prev;

        RenderContent();
        _loading = false;
    }

    // ─────────────────────────────────────────────
    // 内容分发
    // ─────────────────────────────────────────────

    private void RenderContent()
    {
        ContentPanel.Children.Clear();

        switch (_current)
        {
            case Section.General:    RenderGeneral();    break;
            case Section.Appearance: RenderAppearance(); break;
            case Section.Paths:      RenderPaths();      break;
            case Section.Aliases:    RenderAliases();    break;
            case Section.Startup:    RenderStartup();    break;
            case Section.Updates:    RenderUpdates();    break;
        }
    }

    // ─────────────────────────────────────────────
    // 通用 UI 辅助
    // ─────────────────────────────────────────────

    private void AddHeader(string titleKey, string subtitleKey)
    {
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.Add(new TextBlock
        {
            Text = Localization.Get(titleKey),
            FontSize = 24,
            FontWeight = FontWeights.SemiBold,
        });
        panel.Children.Add(new TextBlock
        {
            Text = Localization.Get(subtitleKey),
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
            TextWrapping = TextWrapping.Wrap,
        });
        ContentPanel.Children.Add(panel);

        ContentPanel.Children.Add(new Border
        {
            Height = 1,
            Background = (Brush)Application.Current.Resources["DividerStrokeColorDefaultBrush"],
            Margin = new Thickness(0, 4, 0, 4),
        });
    }

    private Border Card(string titleKey = "")
    {
        var stack = new StackPanel { Spacing = 12 };
        var border = new Border
        {
            Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
            BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Child = stack,
        };

        if (!string.IsNullOrEmpty(titleKey))
        {
            stack.Children.Add(new TextBlock
            {
                Text = Localization.Get(titleKey),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
            });
        }

        ContentPanel.Children.Add(border);
        return border;
    }

    private static StackPanel GetCardStack(Border border) => (StackPanel)border.Child;

    private void AddRow(StackPanel parent, string labelKey, FrameworkElement control, string hintKey = "")
    {
        var grid = new Grid { ColumnSpacing = 12 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var labelStack = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        labelStack.Children.Add(new TextBlock
        {
            Text = Localization.Get(labelKey),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
        });
        if (!string.IsNullOrEmpty(hintKey))
        {
            labelStack.Children.Add(new TextBlock
            {
                Text = Localization.Get(hintKey),
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"],
                TextWrapping = TextWrapping.Wrap,
            });
        }
        Grid.SetColumn(labelStack, 0);
        grid.Children.Add(labelStack);

        Grid.SetColumn(control, 1);
        grid.Children.Add(control);

        parent.Children.Add(grid);
    }

    private void AddFullWidthHint(StackPanel parent, string hintKey)
    {
        parent.Children.Add(new TextBlock
        {
            Text = Localization.Get(hintKey),
            FontSize = 11,
            Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"],
            TextWrapping = TextWrapping.Wrap,
        });
    }

    // ─────────────────────────────────────────────
    // General
    // ─────────────────────────────────────────────

    private void RenderGeneral()
    {
        AddHeader("settings.category.general", "settings.general.subtitle");

        // 快捷键
        var card1 = Card("settings.hotkey");
        var stack1 = GetCardStack(card1);

        _hotkeyBox = new TextBox
        {
            Text = _recordingHotkey
                ? Localization.Get("settings.hotkey.recording")
                : AppSettings.Current.ToggleHotkey,
            IsReadOnly = true,
            FontFamily = new FontFamily("Cascadia Mono, Consolas"),
            FontSize = 12,
            MinWidth = 160,
        };

        var recordBtn = new Button
        {
            Content = _recordingHotkey
                ? Localization.Get("settings.hotkey.cancelled")
                : Localization.Get("settings.hotkey.record"),
        };
        var resetBtn = new Button { Content = Localization.Get("settings.hotkey.reset") };

        var hotkeyPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
        };
        hotkeyPanel.Children.Add(_hotkeyBox);
        hotkeyPanel.Children.Add(recordBtn);
        hotkeyPanel.Children.Add(resetBtn);

        AddRow(stack1, "settings.hotkey", hotkeyPanel, "settings.hotkey.hint");

        _hotkeyStatusText = new TextBlock
        {
            FontSize = 11,
            Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"],
            TextWrapping = TextWrapping.Wrap,
            Visibility = _recordingHotkey ? Visibility.Visible : Visibility.Collapsed,
            Text = _recordingHotkey
                ? Localization.Get("settings.hotkey.recording.hint")
                : string.Empty,
        };
        stack1.Children.Add(_hotkeyStatusText);

        if (_recordingHotkey)
        {
            this.AddHandler(
                UIElement.KeyDownEvent,
                new KeyEventHandler(OnHotkeyRecordingKeyDown),
                true);
        }

        recordBtn.Click += (_, _) =>
        {
            if (_recordingHotkey) return;
            StartRecording();
        };

        resetBtn.Click += (_, _) =>
        {
            StopRecording();
            const string defaultHotkey = "Ctrl+Alt+R";
            if (App.ApplyHotkey(defaultHotkey))
            {
                AppSettings.Current.ToggleHotkey = defaultHotkey;
                AppSettings.Current.Save();
                if (_hotkeyBox != null) _hotkeyBox.Text = defaultHotkey;
                SetHotkeyStatus(Localization.Get("settings.hotkey.resetdone"));
            }
            else
            {
                SetHotkeyStatus(Localization.Get("settings.hotkey.resetfailed"));
            }
        };

        // 窗口行为
        var card2 = Card("settings.window");
        var stack2 = GetCardStack(card2);

        var hideToggle = new ToggleSwitch
        {
            IsOn = AppSettings.Current.HideOnDeactivate,
        };
        hideToggle.Toggled += (_, _) =>
        {
            AppSettings.Current.HideOnDeactivate = hideToggle.IsOn;
            AppSettings.Current.Save();
        };
        AddRow(stack2, "settings.hideOnDeactivate", hideToggle);

        // 语言
        var card3 = Card("settings.language");
        var stack3 = GetCardStack(card3);

        var langCombo = new ComboBox { MinWidth = 160 };
        langCombo.Items.Add(new ComboBoxItem { Content = "简体中文", Tag = "SimplifiedChinese" });
        langCombo.Items.Add(new ComboBoxItem { Content = "繁體中文", Tag = "TraditionalChinese" });
        langCombo.Items.Add(new ComboBoxItem { Content = "English", Tag = "English" });

        for (int i = 0; i < langCombo.Items.Count; i++)
        {
            if (langCombo.Items[i] is ComboBoxItem item &&
                item.Tag is string tag &&
                tag == AppSettings.Current.Language.ToString())
            {
                langCombo.SelectedIndex = i;
                break;
            }
        }

        langCombo.SelectionChanged += (_, _) =>
        {
            if (_loading) return;
            if (langCombo.SelectedItem is not ComboBoxItem item) return;
            if (item.Tag is not string tag) return;
            if (Enum.TryParse<AppLanguage>(tag, out var lang))
            {
                AppSettings.Current.Language = lang;
                AppSettings.Current.Save();
                Localization.Current = lang;
            }
        };

        AddRow(stack3, "settings.language", langCombo);
    }

    // ─── 快捷键录制 ───

    private void StartRecording()
    {
        _recordingHotkey = true;

        if (_hotkeyBox != null)
            _hotkeyBox.Text = Localization.Get("settings.hotkey.recording");

        SetHotkeyStatus(Localization.Get("settings.hotkey.recording.hint"));

        this.AddHandler(
            UIElement.KeyDownEvent,
            new KeyEventHandler(OnHotkeyRecordingKeyDown),
            true);

        this.Focus(FocusState.Programmatic);
    }

    private void StopRecording()
    {
        if (!_recordingHotkey) return;
        _recordingHotkey = false;

        this.RemoveHandler(
            UIElement.KeyDownEvent,
            new KeyEventHandler(OnHotkeyRecordingKeyDown));

        if (_hotkeyBox != null && _hotkeyBox.Text == Localization.Get("settings.hotkey.recording"))
            _hotkeyBox.Text = AppSettings.Current.ToggleHotkey;
    }

    private void SetHotkeyStatus(string text)
    {
        if (_hotkeyStatusText == null) return;
        _hotkeyStatusText.Text = text;
        _hotkeyStatusText.Visibility = string.IsNullOrEmpty(text)
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void OnHotkeyRecordingKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (!_recordingHotkey) return;

        var key = e.Key;

        // 修饰键单独按下不处理
        if (key == VirtualKey.Control ||
            key == VirtualKey.Menu ||
            key == VirtualKey.Shift ||
            key == VirtualKey.LeftWindows ||
            key == VirtualKey.RightWindows)
        {
            e.Handled = true;
            return;
        }

        if (key == VirtualKey.Escape)
        {
            e.Handled = true;
            StopRecording();
            if (_hotkeyBox != null)
                _hotkeyBox.Text = AppSettings.Current.ToggleHotkey;
            SetHotkeyStatus(Localization.Get("settings.hotkey.cancelled"));
            return;
        }

        e.Handled = true;

        var ctrl = IsDown(VK_CONTROL);
        var alt = IsDown(VK_MENU);
        var shift = IsDown(VK_SHIFT);

        if (!ctrl && !alt && !shift)
        {
            SetHotkeyStatus(Localization.Get("settings.hotkey.needModifier"));
            return;
        }

        var keyChar = KeyToChar(key);
        if (keyChar == null)
        {
            SetHotkeyStatus(Localization.Get("settings.hotkey.needKey"));
            return;
        }

        var parts = new List<string>();
        if (ctrl) parts.Add("Ctrl");
        if (alt) parts.Add("Alt");
        if (shift) parts.Add("Shift");
        parts.Add(keyChar);

        var hotkeyString = string.Join("+", parts);

        var ok = App.ApplyHotkey(hotkeyString);

        if (ok)
        {
            AppSettings.Current.ToggleHotkey = hotkeyString;
            AppSettings.Current.Save();
            if (_hotkeyBox != null) _hotkeyBox.Text = hotkeyString;
            SetHotkeyStatus(Localization.Get("settings.hotkey.saved"));
        }
        else
        {
            SetHotkeyStatus(Localization.Get("settings.hotkey.conflict"));
        }

        StopRecording();
    }

    private static string? KeyToChar(VirtualKey key)
    {
        if (key >= VirtualKey.A && key <= VirtualKey.Z)
            return ((char)('A' + (key - VirtualKey.A))).ToString();
        if (key >= VirtualKey.Number0 && key <= VirtualKey.Number9)
            return ((char)('0' + (key - VirtualKey.Number0))).ToString();
        return null;
    }

    // ─────────────────────────────────────────────
    // Appearance
    // ─────────────────────────────────────────────

    private void RenderAppearance()
    {
        AddHeader("settings.category.appearance", "settings.appearance.subtitle");

        // ─── 窗口材质 ───
        var card0 = Card("settings.appearance.material");
        var stack0 = GetCardStack(card0);

        var materialCombo = new ComboBox { MinWidth = 180 };

        var candidates = new List<(WindowMaterial mat, string key)>
        {
            (WindowMaterial.Mica,    "material.mica"),
            (WindowMaterial.MicaAlt, "material.micaAlt"),
            (WindowMaterial.Acrylic, "material.acrylic"),
            (WindowMaterial.None,    "material.none"),
        };

        foreach (var (mat, key) in candidates)
        {
            if (mat == WindowMaterial.Mica && !BackdropHelper.IsMicaSupported) continue;
            if (mat == WindowMaterial.MicaAlt && !BackdropHelper.IsMicaSupported) continue;
            if (mat == WindowMaterial.Acrylic && !BackdropHelper.IsAcrylicSupported) continue;

            materialCombo.Items.Add(new ComboBoxItem
            {
                Content = Localization.Get(key),
                Tag = mat.ToString(),
            });
        }

        for (int i = 0; i < materialCombo.Items.Count; i++)
        {
            if (materialCombo.Items[i] is ComboBoxItem item &&
                item.Tag is string tag &&
                tag == AppSettings.Current.BackgroundMaterial.ToString())
            {
                materialCombo.SelectedIndex = i;
                break;
            }
        }

        if (materialCombo.SelectedIndex < 0 && materialCombo.Items.Count > 0)
            materialCombo.SelectedIndex = 0;

        materialCombo.SelectionChanged += (_, _) =>
        {
            if (_loading) return;
            if (materialCombo.SelectedItem is not ComboBoxItem item) return;
            if (item.Tag is not string tag) return;
            if (!Enum.TryParse<WindowMaterial>(tag, out var mat)) return;

            AppSettings.Current.BackgroundMaterial = mat;
            AppSettings.Current.Save();

            App.ApplyMaterialToAll(mat);
        };

        AddRow(stack0, "settings.appearance.material", materialCombo,
               "settings.appearance.material.hint");

        // ─── 输出颜色 ───
        var card1 = Card("settings.output.colors");
        var stack1 = GetCardStack(card1);

        var colorsToggle = new ToggleSwitch
        {
            IsOn = AppSettings.Current.OutputColorsEnabled,
        };
        colorsToggle.Toggled += (_, _) =>
        {
            AppSettings.Current.OutputColorsEnabled = colorsToggle.IsOn;
            AppSettings.Current.Save();
            App.MainWindowInstance?.RefreshMainPage();
        };
        AddRow(stack1, "settings.output.colors.enabled", colorsToggle, "settings.output.colors.hint");

        var schemeCombo = new ComboBox { MinWidth = 140 };
        schemeCombo.Items.Add(new ComboBoxItem { Content = Localization.Get("output.colors.scheme.auto"),  Tag = "Auto" });
        schemeCombo.Items.Add(new ComboBoxItem { Content = Localization.Get("output.colors.scheme.dark"),  Tag = "Dark" });
        schemeCombo.Items.Add(new ComboBoxItem { Content = Localization.Get("output.colors.scheme.light"), Tag = "Light" });

        for (int i = 0; i < schemeCombo.Items.Count; i++)
        {
            if (schemeCombo.Items[i] is ComboBoxItem item &&
                item.Tag is string tag &&
                tag == AppSettings.Current.OutputColorScheme.ToString())
            {
                schemeCombo.SelectedIndex = i;
                break;
            }
        }

        schemeCombo.SelectionChanged += (_, _) =>
        {
            if (_loading) return;
            if (schemeCombo.SelectedItem is not ComboBoxItem item) return;
            if (item.Tag is not string tag) return;
            if (Enum.TryParse<OutputColorScheme>(tag, out var scheme))
            {
                AppSettings.Current.OutputColorScheme = scheme;
                AppSettings.Current.Save();
                App.MainWindowInstance?.RefreshMainPage();
            }
        };

        AddRow(stack1, "settings.output.colors.scheme", schemeCombo);
    }

    // ─────────────────────────────────────────────
    // Paths
    // ─────────────────────────────────────────────

    private void RenderPaths()
    {
        AddHeader("settings.category.paths", "settings.paths.subtitle");

        // 工作目录
        var card1 = Card("settings.workingdir");
        var stack1 = GetCardStack(card1);

        var dirBox = new TextBox
        {
            Text = string.IsNullOrEmpty(AppSettings.Current.DefaultWorkingDirectory)
                ? ShellQuoting.AbbreviateHome(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
                : ShellQuoting.AbbreviateHome(AppSettings.Current.DefaultWorkingDirectory),
            IsReadOnly = true,
            FontFamily = new FontFamily("Cascadia Mono, Consolas"),
            FontSize = 12,
            Width = 320,
        };
        var chooseBtn = new Button { Content = Localization.Get("settings.workingdir.choose") };
        var resetBtn = new Button { Content = Localization.Get("settings.workingdir.reset") };

        var dirPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        dirPanel.Children.Add(dirBox);
        dirPanel.Children.Add(chooseBtn);
        dirPanel.Children.Add(resetBtn);

        AddRow(stack1, "settings.workingdir", dirPanel);

        var warnText = new TextBlock
        {
            Text = Localization.Get("settings.workingdir.invalid"),
            FontSize = 11,
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
            Visibility = AppSettings.Current.IsWorkingDirectoryValid
                ? Visibility.Collapsed : Visibility.Visible,
        };
        stack1.Children.Add(warnText);

        AddFullWidthHint(stack1, "settings.workingdir.new.window.hint");

        chooseBtn.Click += async (_, _) =>
        {
            var hwnd = GetHostHwnd();
            if (hwnd == IntPtr.Zero) return;

            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                AppSettings.Current.DefaultWorkingDirectory = folder.Path;
                AppSettings.Current.Save();
                dirBox.Text = ShellQuoting.AbbreviateHome(folder.Path);
                warnText.Visibility = Visibility.Collapsed;
            }
        };

        resetBtn.Click += (_, _) =>
        {
            AppSettings.Current.DefaultWorkingDirectory = string.Empty;
            AppSettings.Current.Save();
            dirBox.Text = ShellQuoting.AbbreviateHome(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            warnText.Visibility = Visibility.Collapsed;
        };

        // 自定义路径
        var card2 = Card("settings.paths.title");
        var stack2 = GetCardStack(card2);

        PathStore.Shared.Reload();
        var paths = PathStore.Shared.Paths.ToList();

        if (paths.Count == 0)
        {
            stack2.Children.Add(new TextBlock
            {
                Text = Localization.Get("settings.paths.empty"),
                FontSize = 12,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
            });
        }
        else
        {
            for (int i = 0; i < paths.Count; i++)
            {
                var idx = i;
                var path = paths[i];

                var row = new Grid { ColumnSpacing = 8 };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var tb = new TextBlock
                {
                    Text = path,
                    FontFamily = new FontFamily("Cascadia Mono, Consolas"),
                    FontSize = 12,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = PathStore.Shared.IsValid(path)
                        ? (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"]
                        : new SolidColorBrush(Microsoft.UI.Colors.Red),
                };
                Grid.SetColumn(tb, 0);
                row.Children.Add(tb);

                var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };

                var upBtn = new Button { Content = new FontIcon { Glyph = "\xE70E", FontSize = 10 } };
                upBtn.IsEnabled = idx > 0;
                upBtn.Click += (_, _) =>
                {
                    PathStore.Shared.Move(idx, idx - 1);
                    RenderPaths();
                };
                btnPanel.Children.Add(upBtn);

                var downBtn = new Button { Content = new FontIcon { Glyph = "\xE70D", FontSize = 10 } };
                downBtn.IsEnabled = idx < paths.Count - 1;
                downBtn.Click += (_, _) =>
                {
                    PathStore.Shared.Move(idx, idx + 1);
                    RenderPaths();
                };
                btnPanel.Children.Add(downBtn);

                var delBtn = new Button
                {
                    Content = new FontIcon
                    {
                        Glyph = "\xE74D",
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
                    },
                };
                delBtn.Click += (_, _) =>
                {
                    PathStore.Shared.Remove(path);
                    RenderPaths();
                };
                btnPanel.Children.Add(delBtn);

                Grid.SetColumn(btnPanel, 1);
                row.Children.Add(btnPanel);

                stack2.Children.Add(row);
            }
        }

        var addPathBtn = new Button { Content = Localization.Get("settings.paths.add") };
        addPathBtn.Click += async (_, _) =>
        {
            var hwnd = GetHostHwnd();
            if (hwnd == IntPtr.Zero) return;

            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                PathStore.Shared.Add(folder.Path);
                RenderPaths();
            }
        };
        stack2.Children.Add(addPathBtn);

        AddFullWidthHint(stack2, "settings.paths.hint");
    }

    private IntPtr GetHostHwnd()
    {
        if (App.MainWindowInstance != null)
            return WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
        return IntPtr.Zero;
    }

    // ─────────────────────────────────────────────
    // Aliases
    // ─────────────────────────────────────────────

    private void RenderAliases()
    {
        AddHeader("settings.category.aliases", "settings.aliases.subtitle");

        var card = Card("settings.aliases.title");
        var stack = GetCardStack(card);

        AliasStore.Shared.Reload();
        var aliases = AliasStore.Shared.Aliases.ToList();

        if (aliases.Count == 0)
        {
            stack.Children.Add(new TextBlock
            {
                Text = Localization.Get("settings.aliases.empty"),
                FontSize = 12,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
            });
        }
        else
        {
            foreach (var alias in aliases)
            {
                var row = new Grid { ColumnSpacing = 8 };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var nameTb = new TextBlock
                {
                    Text = alias.Name,
                    FontFamily = new FontFamily("Cascadia Mono, Consolas"),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(nameTb, 0);
                row.Children.Add(nameTb);

                var arrow = new TextBlock
                {
                    Text = "→",
                    FontSize = 12,
                    Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"],
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(arrow, 1);
                row.Children.Add(arrow);

                var expTb = new TextBlock
                {
                    Text = alias.Expansion,
                    FontFamily = new FontFamily("Cascadia Mono, Consolas"),
                    FontSize = 12,
                    Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(expTb, 2);
                row.Children.Add(expTb);

                var btns = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };

                var editBtn = new Button { Content = new FontIcon { Glyph = "\xE70F", FontSize = 10 } };
                editBtn.Click += async (_, _) => await EditAliasDialog(alias);
                btns.Children.Add(editBtn);

                var delBtn = new Button
                {
                    Content = new FontIcon
                    {
                        Glyph = "\xE74D",
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
                    },
                };
                delBtn.Click += async (_, _) =>
                {
                    if (this.XamlRoot == null) return;

                    var dlg = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Title = Localization.Get("settings.aliases.delete.title"),
                        Content = string.Format(
                            Localization.Get("settings.aliases.delete.message"), alias.Name),
                        PrimaryButtonText = Localization.Get("button.delete"),
                        CloseButtonText = Localization.Get("button.cancel"),
                        DefaultButton = ContentDialogButton.Close,
                    };
                    if (await dlg.ShowAsync() == ContentDialogResult.Primary)
                    {
                        AliasStore.Shared.Remove(alias.Name);
                        RenderAliases();
                    }
                };
                btns.Children.Add(delBtn);

                Grid.SetColumn(btns, 3);
                row.Children.Add(btns);

                stack.Children.Add(row);
            }
        }

        var addBtn = new Button { Content = Localization.Get("settings.aliases.add") };
        addBtn.Click += async (_, _) => await EditAliasDialog(null);
        stack.Children.Add(addBtn);

        AddFullWidthHint(stack, "settings.aliases.hint");
    }

    private async Task EditAliasDialog(CommandAlias? existing)
    {
        if (this.XamlRoot == null) return;

        var nameBox = new TextBox
        {
            Header = Localization.Get("settings.aliases.field.name"),
            Text = existing?.Name ?? string.Empty,
            FontFamily = new FontFamily("Cascadia Mono, Consolas"),
            FontSize = 13,
        };
        var expBox = new TextBox
        {
            Header = Localization.Get("settings.aliases.field.expansion"),
            Text = existing?.Expansion ?? string.Empty,
            FontFamily = new FontFamily("Cascadia Mono, Consolas"),
            FontSize = 13,
        };

        var errorText = new TextBlock
        {
            FontSize = 11,
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
            Visibility = Visibility.Collapsed,
        };

        var panel = new StackPanel { Spacing = 12, MinWidth = 360 };
        panel.Children.Add(nameBox);
        panel.Children.Add(expBox);
        panel.Children.Add(new TextBlock
        {
            Text = Localization.Get("settings.aliases.editor.hint"),
            FontSize = 11,
            Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"],
            TextWrapping = TextWrapping.Wrap,
        });
        panel.Children.Add(errorText);

        var dlg = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = existing == null
                ? Localization.Get("settings.aliases.add.title")
                : Localization.Get("settings.aliases.edit.title"),
            Content = panel,
            PrimaryButtonText = Localization.Get("button.save"),
            CloseButtonText = Localization.Get("button.cancel"),
            DefaultButton = ContentDialogButton.Primary,
        };

        dlg.PrimaryButtonClick += (_, args) =>
        {
            var name = nameBox.Text.Trim();
            var expansion = expBox.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(expansion))
            {
                errorText.Text = Localization.Get("settings.aliases.error.empty");
                errorText.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            if (name.Contains(' '))
            {
                errorText.Text = Localization.Get("settings.aliases.error.space");
                errorText.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            if ((existing == null || existing.Name != name) &&
                AliasStore.Shared.Aliases.Any(a => a.Name == name))
            {
                errorText.Text = Localization.Get("settings.aliases.error.duplicate");
                errorText.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            if (existing != null && existing.Name != name)
                AliasStore.Shared.Remove(existing.Name);

            AliasStore.Shared.Add(new CommandAlias { Name = name, Expansion = expansion });
        };

        if (await dlg.ShowAsync() == ContentDialogResult.Primary)
            RenderAliases();
    }

    // ─────────────────────────────────────────────
    // Startup
    // ─────────────────────────────────────────────

    private void RenderStartup()
    {
        AddHeader("settings.category.startup", "settings.startup.subtitle");

        var card = Card();
        var stack = GetCardStack(card);

        var launchToggle = new ToggleSwitch
        {
            IsOn = LaunchAtLogin.IsEnabled,
        };
        launchToggle.Toggled += (_, _) =>
        {
            var ok = LaunchAtLogin.SetEnabled(launchToggle.IsOn);
            if (!ok)
                launchToggle.IsOn = LaunchAtLogin.IsEnabled;
        };
        AddRow(stack, "settings.startup.launchAtLogin", launchToggle, "settings.startup.launchAtLogin.hint");

        var hideToggle = new ToggleSwitch
        {
            IsOn = AppSettings.Current.HideWindowOnLaunch,
        };
        hideToggle.Toggled += (_, _) =>
        {
            AppSettings.Current.HideWindowOnLaunch = hideToggle.IsOn;
            AppSettings.Current.Save();
        };
        AddRow(stack, "settings.startup.hideOnLaunch", hideToggle, "settings.startup.hideOnLaunch.hint");
    }

    // ─────────────────────────────────────────────
    // Updates
    // ─────────────────────────────────────────────

    private void RenderUpdates()
    {
        AddHeader("settings.category.updates", "settings.updates.subtitle");

        var card = Card("settings.updates.currentVersion");
        var stack = GetCardStack(card);

        var version = System.Reflection.Assembly.GetExecutingAssembly()
            .GetName().Version?.ToString(3) ?? "1.0.0";

        stack.Children.Add(new TextBlock
        {
            Text = version,
            FontFamily = new FontFamily("Cascadia Mono, Consolas"),
            FontSize = 13,
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
        });

        var link = new HyperlinkButton
        {
            Content = Localization.Get("settings.updates.getLatest"),
            NavigateUri = new Uri("https://github.com/CaoHaoran-Dev/ProcessSH/releases"),
        };
        stack.Children.Add(link);

        AddFullWidthHint(stack, "settings.updates.note");
    }
}