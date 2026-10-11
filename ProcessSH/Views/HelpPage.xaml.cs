using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ProcessSH.Models;

namespace ProcessSH.Views;

public sealed partial class HelpPage : Page
{
    private string _currentTopic = "overview";
    private string _filter = string.Empty;

    public HelpPage()
    {
        InitializeComponent();
        ApplyLocalization();
        Localization.LanguageChanged += ApplyLocalization;
        Unloaded += OnUnloaded;

        Loaded += (_, _) =>
        {
            if (Nav.MenuItems.Count > 0)
                Nav.SelectedItem = Nav.MenuItems[0];
            RenderTopic("overview");
        };
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Localization.LanguageChanged -= ApplyLocalization;
    }

    private void ApplyLocalization()
    {
        if (Nav.MenuItems.Count >= 5)
        {
            ((NavigationViewItem)Nav.MenuItems[0]).Content = Localization.Get("help.topic.overview");
            ((NavigationViewItem)Nav.MenuItems[1]).Content = Localization.Get("help.topic.shortcuts");
            ((NavigationViewItem)Nav.MenuItems[2]).Content = Localization.Get("help.topic.tips");
            ((NavigationViewItem)Nav.MenuItems[3]).Content = Localization.Get("help.topic.sudo");
            ((NavigationViewItem)Nav.MenuItems[4]).Content = Localization.Get("help.topic.aliases");
        }

        SearchBox.PlaceholderText = Localization.Get("help.search");
        RenderTopic(_currentTopic);
    }

    private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            _currentTopic = tag;
            RenderTopic(tag);
        }
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
        _filter = sender.Text.Trim();

        foreach (var obj in Nav.MenuItems)
        {
            if (obj is NavigationViewItem item && item.Content is string title)
            {
                item.Visibility = string.IsNullOrEmpty(_filter)
                    || title.Contains(_filter, StringComparison.OrdinalIgnoreCase)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }

    private void RenderTopic(string topic)
    {
        ContentPanel.Children.Clear();

        switch (topic)
        {
            case "overview":  RenderOverview(); break;
            case "shortcuts": RenderShortcuts(); break;
            case "tips":      RenderTips(); break;
            case "sudo":      RenderSudo(); break;
            case "aliases":   RenderAliases(); break;
        }
    }

    // ─── Sections ───

    private void RenderOverview()
    {
        AddHeading("ProcessSH");
        AddParagraph(Localization.Get("help.overview.p1"));
        AddParagraph(Localization.Get("help.overview.p2"));

        AddHeading(Localization.Get("help.overview.quick"));
        AddKeyRow("Ctrl+Alt+R", Localization.Get("help.shortcut.toggle"));
        AddKeyRow("Ctrl+N",     Localization.Get("help.shortcut.new"));
        AddKeyRow("Ctrl+R",     Localization.Get("help.shortcut.history"));
        AddKeyRow("Ctrl+,",     Localization.Get("help.shortcut.settings"));
        AddKeyRow("Ctrl+/",     Localization.Get("help.shortcut.help"));
        AddKeyRow("Enter",      Localization.Get("help.shortcut.enter"));
        AddKeyRow("Shift+Enter",Localization.Get("help.shortcut.newline"));
        AddKeyRow("Tab",        Localization.Get("help.shortcut.complete"));
        AddKeyRow("↑ ↓",        Localization.Get("help.shortcut.navigate"));
        AddKeyRow("Esc",        Localization.Get("help.shortcut.hide"));
    }

    private void RenderShortcuts()
    {
        AddHeading(Localization.Get("help.shortcuts.global"));
        AddKeyRow("Ctrl+Alt+R", Localization.Get("help.shortcut.toggle"));
        AddKeyRow("Ctrl+N",     Localization.Get("help.shortcut.new"));
        AddKeyRow("Ctrl+R",     Localization.Get("help.shortcut.history"));
        AddKeyRow("Ctrl+,",     Localization.Get("help.shortcut.settings"));
        AddKeyRow("Ctrl+/",     Localization.Get("help.shortcut.help"));
        AddKeyRow("Esc",        Localization.Get("help.shortcut.hide"));

        AddHeading(Localization.Get("help.shortcuts.input"));
        AddKeyRow("Enter",       Localization.Get("help.shortcut.enter"));
        AddKeyRow("Shift+Enter", Localization.Get("help.shortcut.newline"));
        AddKeyRow("Tab",         Localization.Get("help.shortcut.complete"));
        AddKeyRow("↑ ↓",         Localization.Get("help.shortcut.navigate"));
    }

    private void RenderTips()
    {
        AddHeading(Localization.Get("help.tip.drag.title"));
        AddParagraph(Localization.Get("help.tip.drag.desc"));

        AddHeading(Localization.Get("help.tip.app.title"));
        AddParagraph(Localization.Get("help.tip.app.desc"));
        AddCode("C:\\Windows\\notepad.exe");

        AddHeading(Localization.Get("help.tip.multiline.title"));
        AddParagraph(Localization.Get("help.tip.multiline.desc"));
    }

    private void RenderSudo()
    {
        AddHeading(Localization.Get("help.topic.sudo"));
        AddParagraph(Localization.Get("help.sudo.desc"));
        AddBullet(Localization.Get("help.sudo.bullet1"));
        AddBullet(Localization.Get("help.sudo.bullet2"));
        AddBullet(Localization.Get("help.sudo.bullet3"));
    }

    private void RenderAliases()
    {
        AddHeading(Localization.Get("help.topic.aliases"));
        AddParagraph(Localization.Get("help.aliases.desc"));

        AddHeading(Localization.Get("help.aliases.defaults"));
        AddCode("gs    → git status\ngp    → git pull --rebase\nll    → Get-ChildItem -Force\nserve → python3 -m http.server 8000");

        AddHeading(Localization.Get("help.aliases.custom"));
        AddParagraph(Localization.Get("help.aliases.custom.desc"));
        AddCode("%LOCALAPPDATA%\\ProcessSH\\aliases.json");
    }

    // ─── Helpers ───

    private void AddHeading(string text)
    {
        ContentPanel.Children.Add(new TextBlock
        {
            Text = text,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 8, 0, 0),
        });
    }

    private void AddParagraph(string text)
    {
        ContentPanel.Children.Add(new TextBlock
        {
            Text = text,
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
        });
    }

    private void AddBullet(string text)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        panel.Children.Add(new TextBlock
        {
            Text = "•",
            FontSize = 13,
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
        });
        panel.Children.Add(new TextBlock
        {
            Text = text,
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
        });
        ContentPanel.Children.Add(panel);
    }

    private void AddKeyRow(string keys, string desc)
    {
        var grid = new Grid { ColumnSpacing = 12 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var keyBorder = new Border
        {
            Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
            BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 2, 8, 2),
        };
        keyBorder.Child = new TextBlock
        {
            Text = keys,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12,
        };
        Grid.SetColumn(keyBorder, 0);
        grid.Children.Add(keyBorder);

        var descText = new TextBlock
        {
            Text = desc,
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(descText, 1);
        grid.Children.Add(descText);

        ContentPanel.Children.Add(grid);
    }

    private void AddCode(string code)
    {
        var border = new Border
        {
            Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
            BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8, 12, 8),
        };
        border.Child = new TextBlock
        {
            Text = code,
            FontFamily = new FontFamily("Cascadia Mono, Consolas"),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            IsTextSelectionEnabled = true,
        };
        ContentPanel.Children.Add(border);
    }
}