using Microsoft.UI.Xaml;

namespace ProcessSH.Models;

public sealed class Suggestion : IEquatable<Suggestion>
{
    public string Text { get; }
    public SuggestionType Type { get; }
    public int Priority { get; }
    public string? Subtitle { get; }

    public Suggestion(string text, SuggestionType type, string? subtitle = null)
    {
        Text = text;
        Type = type;
        Subtitle = subtitle;
        Priority = type switch
        {
            SuggestionType.Alias   => 500,
            SuggestionType.History => 300,
            SuggestionType.Command => 200,
            SuggestionType.Path    => 100,
            _ => 0,
        };
    }

    public Suggestion(string text, SuggestionType type, int historyCount)
    {
        Text = text;
        Type = type;
        Subtitle = null;
        var safe = Math.Clamp(historyCount, 0, 100);
        Priority = type switch
        {
            SuggestionType.Alias   => 500,
            SuggestionType.History => 300 + safe,
            SuggestionType.Command => 200,
            SuggestionType.Path    => 100,
            _ => 0,
        };
    }

    public bool Equals(Suggestion? other)
    {
        if (other is null) return false;
        return Text == other.Text && Type == other.Type;
    }

    public override bool Equals(object? obj) => Equals(obj as Suggestion);

    public override int GetHashCode() => HashCode.Combine(Text, Type);

    public override string ToString() => Text;

    public string IconGlyph => Type switch
    {
        SuggestionType.Alias   => "\xE8F1", // Wand
        SuggestionType.History => "\xE81C", // History
        SuggestionType.Command => "\xE756", // CommandPrompt
        SuggestionType.Path    => "\xE8B7", // Folder
        _ => "\xE9CE",
    };

    /// <summary>Subtitle 为空时折叠。供 x:Bind 使用，避免用转换器。</summary>
    public Visibility SubtitleVisibility =>
        string.IsNullOrEmpty(Subtitle) ? Visibility.Collapsed : Visibility.Visible;
}

public enum SuggestionType
{
    Alias,
    History,
    Command,
    Path,
}