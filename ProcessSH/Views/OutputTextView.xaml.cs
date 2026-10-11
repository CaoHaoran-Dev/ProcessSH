using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using ProcessSH.Models;
using ProcessSH.Utils;

namespace ProcessSH.Views;

public sealed partial class OutputTextView : UserControl
{
    public OutputTextView()
    {
        InitializeComponent();
        ActualThemeChanged += (_, _) => Render();
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(OutputTextView),
            new PropertyMetadata(string.Empty, OnTextChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OutputTextView view) view.Render();
    }

    private void Render()
    {
        OutputBlock.Blocks.Clear();

        var text = Text ?? string.Empty;
        if (string.IsNullOrEmpty(text)) return;

        var dark = ActualTheme == ElementTheme.Dark
                   || (ActualTheme == ElementTheme.Default
                       && Application.Current.RequestedTheme == ApplicationTheme.Dark);

        var colorsEnabled = AppSettings.Current.OutputColorsEnabled;
        var scheme = AppSettings.Current.OutputColorScheme;

        if (scheme == OutputColorScheme.Dark) dark = true;
        else if (scheme == OutputColorScheme.Light) dark = false;

        var paragraph = new Paragraph();

        var runs = colorsEnabled
            ? ANSIParser.Parse(text, dark)
            : new List<ANSIParser.AnsiRun>
              {
                  new() { Text = ANSIParser.StripANSI(text) }
              };

        foreach (var ansi in runs)
        {
            // ⚠️ WinUI 3 的 Run / Span 都没有 Background。
            //    Inline 家族只支持前景色、字体、下划线。
            //    背景色渲染留待将来换自定义控件时实现。
            var run = new Run { Text = ansi.Text };

            if (ansi.Foreground.HasValue)
                run.Foreground = new SolidColorBrush(ansi.Foreground.Value);

            if (ansi.Bold)
                run.FontWeight = Microsoft.UI.Text.FontWeights.Bold;

            if (ansi.Italic)
                run.FontStyle = Windows.UI.Text.FontStyle.Italic;

            if (ansi.Underline)
                run.TextDecorations = Windows.UI.Text.TextDecorations.Underline;

            paragraph.Inlines.Add(run);
        }

        OutputBlock.Blocks.Add(paragraph);
    }

    public void ScrollToEnd()
    {
        try { RootScroll.ChangeView(null, RootScroll.ScrollableHeight, null); }
        catch { }
    }
}