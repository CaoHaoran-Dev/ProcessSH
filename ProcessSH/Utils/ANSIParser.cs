using System.Text;
using Microsoft.UI;
using Windows.UI;

namespace ProcessSH.Utils;

/// <summary>
/// ANSI 转义序列解析器。支持 8/16/256 色和 24 位真彩色。
/// 输出与 WinUI RichTextBlock 的 Run 属性直接对应。
/// </summary>
public static class ANSIParser
{
    public sealed class AnsiRun
    {
        public string Text { get; init; } = string.Empty;
        public Color? Foreground { get; init; }
        public Color? Background { get; init; }
        public bool Bold { get; init; }
        public bool Italic { get; init; }
        public bool Underline { get; init; }
        public bool Dim { get; init; }
        public bool Reversed { get; init; }
    }

    private struct SGRState
    {
        public int? FgIndex;
        public (int r, int g, int b)? FgRgb;
        public bool FgDefault;
        public int? BgIndex;
        public (int r, int g, int b)? BgRgb;
        public bool BgDefault;
        public bool Bold, Dim, Italic, Underline, Reversed;

        public void Reset() => this = new SGRState();
    }

    /// <summary>解析 ANSI 字符串，返回带样式的片段列表</summary>
    public static List<AnsiRun> Parse(string input, bool darkTheme)
    {
        var runs = new List<AnsiRun>();
        if (string.IsNullOrEmpty(input)) return runs;

        var state = new SGRState();
        var buffer = new StringBuilder();

        void Flush()
        {
            if (buffer.Length == 0) return;
            runs.Add(BuildRun(buffer.ToString(), state, darkTheme));
            buffer.Clear();
        }

        int i = 0;
        while (i < input.Length)
        {
            var ch = input[i];

            if (ch == '\u001B' && i + 1 < input.Length && input[i + 1] == '[')
            {
                int j = i + 2;
                var paramsStr = new StringBuilder();
                bool found = false;
                while (j < input.Length)
                {
                    var c = input[j];
                    if (c == 'm') { found = true; break; }
                    if (char.IsDigit(c) || c == ';') { paramsStr.Append(c); j++; }
                    else break;
                }
                if (found)
                {
                    Flush();
                    ApplySGR(paramsStr.ToString(), ref state);
                    i = j + 1;
                    continue;
                }
            }

            // 处理 \r\n 与 \r
            if (ch == '\r')
            {
                i++;
                if (i < input.Length && input[i] == '\n') i++;
                buffer.Append('\n');
                continue;
            }

            buffer.Append(ch);
            i++;
        }
        Flush();

        return runs;
    }

    /// <summary>移除所有 ANSI 转义序列</summary>
    public static string StripANSI(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        var sb = new StringBuilder();
        int i = 0;
        while (i < input.Length)
        {
            if (input[i] == '\u001B' && i + 1 < input.Length && input[i + 1] == '[')
            {
                int j = i + 2;
                bool found = false;
                while (j < input.Length)
                {
                    var c = input[j];
                    if (c == 'm') { found = true; break; }
                    if (char.IsDigit(c) || c == ';') j++;
                    else break;
                }
                if (found) { i = j + 1; continue; }
            }
            sb.Append(input[i]);
            i++;
        }
        return sb.ToString();
    }

    private static AnsiRun BuildRun(string text, SGRState state, bool dark)
    {
        Color? fg = null, bg = null;

        if (state.FgRgb.HasValue)
            fg = Color.FromArgb(255, (byte)state.FgRgb.Value.r,
                                     (byte)state.FgRgb.Value.g,
                                     (byte)state.FgRgb.Value.b);
        else if (state.FgIndex.HasValue)
            fg = IndexedColor(state.FgIndex.Value, dark);
        else if (state.FgDefault)
            fg = dark ? Colors.White : Colors.Black;

        if (state.BgRgb.HasValue)
            bg = Color.FromArgb(255, (byte)state.BgRgb.Value.r,
                                     (byte)state.BgRgb.Value.g,
                                     (byte)state.BgRgb.Value.b);
        else if (state.BgIndex.HasValue)
            bg = IndexedColor(state.BgIndex.Value, dark);
        else if (state.BgDefault)
            bg = dark ? Colors.Black : Colors.White;

        if (state.Reversed)
        {
            var newFg = bg ?? (dark ? Colors.Black : Colors.White);
            var newBg = fg ?? (dark ? Colors.White : Colors.Black);
            fg = newFg;
            bg = newBg;
        }

        if (state.Dim && fg.HasValue)
        {
            var c = fg.Value;
            fg = Color.FromArgb(180, c.R, c.G, c.B);
        }

        return new AnsiRun
        {
            Text = text,
            Foreground = fg,
            Background = bg,
            Bold = state.Bold,
            Italic = state.Italic,
            Underline = state.Underline,
            Dim = state.Dim,
            Reversed = state.Reversed,
        };
    }

    private static void ApplySGR(string paramsStr, ref SGRState state)
    {
        var codes = string.IsNullOrEmpty(paramsStr)
            ? new[] { 0 }
            : paramsStr.Split(';').Select(s => int.TryParse(s, out var n) ? n : 0).ToArray();

        int i = 0;
        while (i < codes.Length)
        {
            var code = codes[i];
            switch (code)
            {
                case 0: state.Reset(); break;
                case 1: state.Bold = true; break;
                case 2: state.Dim = true; break;
                case 3: state.Italic = true; break;
                case 4: state.Underline = true; break;
                case 7: state.Reversed = true; break;
                case 21:
                case 22: state.Bold = false; state.Dim = false; break;
                case 23: state.Italic = false; break;
                case 24: state.Underline = false; break;
                case 27: state.Reversed = false; break;

                case >= 30 and <= 37:
                    state.FgIndex = code - 30; state.FgRgb = null; state.FgDefault = false; break;
                case 39:
                    state.FgIndex = null; state.FgRgb = null; state.FgDefault = true; break;
                case >= 40 and <= 47:
                    state.BgIndex = code - 40; state.BgRgb = null; state.BgDefault = false; break;
                case 49:
                    state.BgIndex = null; state.BgRgb = null; state.BgDefault = true; break;
                case >= 90 and <= 97:
                    state.FgIndex = code - 90 + 8; state.FgRgb = null; state.FgDefault = false; break;
                case >= 100 and <= 107:
                    state.BgIndex = code - 100 + 8; state.BgRgb = null; state.BgDefault = false; break;

                case 38:
                case 48:
                    if (i + 1 < codes.Length)
                    {
                        var mode = codes[i + 1];
                        if (mode == 5 && i + 2 < codes.Length)
                        {
                            var n = codes[i + 2];
                            if (code == 38) { state.FgIndex = n; state.FgRgb = null; state.FgDefault = false; }
                            else { state.BgIndex = n; state.BgRgb = null; state.BgDefault = false; }
                            i += 2;
                        }
                        else if (mode == 2 && i + 4 < codes.Length)
                        {
                            var rgb = (codes[i + 2], codes[i + 3], codes[i + 4]);
                            if (code == 38) { state.FgRgb = rgb; state.FgIndex = null; state.FgDefault = false; }
                            else { state.BgRgb = rgb; state.BgIndex = null; state.BgDefault = false; }
                            i += 4;
                        }
                        else if (i + 3 < codes.Length)
                        {
                            // 简写 38;R;G;B
                            var rgb = (codes[i + 1], codes[i + 2], codes[i + 3]);
                            if (code == 38) { state.FgRgb = rgb; state.FgIndex = null; state.FgDefault = false; }
                            else { state.BgRgb = rgb; state.BgIndex = null; state.BgDefault = false; }
                            i += 3;
                        }
                    }
                    break;
            }
            i++;
        }
    }

    private static Color IndexedColor(int idx, bool dark)
    {
        if (idx < 0) return dark ? Colors.White : Colors.Black;
        if (idx < 8) return StandardColor(idx, false, dark);
        if (idx < 16) return StandardColor(idx - 8, true, dark);
        if (idx < 232)
        {
            int i = idx - 16;
            byte r = (byte)((i / 36) % 6 * 51);
            byte g = (byte)((i / 6) % 6 * 51);
            byte b = (byte)(i % 6 * 51);
            return Color.FromArgb(255, r, g, b);
        }
        byte v = (byte)((idx - 232) * 255 / 23);
        return Color.FromArgb(255, v, v, v);
    }

    private static Color StandardColor(int n, bool bright, bool dark)
    {
        n = Math.Clamp(n, 0, 7);

        if (dark)
        {
            var normal = new[]
            {
                Color.FromArgb(255, 89, 89, 89),
                Color.FromArgb(255, 242, 89, 89),
                Color.FromArgb(255, 102, 217, 102),
                Color.FromArgb(255, 242, 204, 89),
                Color.FromArgb(255, 102, 166, 242),
                Color.FromArgb(255, 217, 128, 230),
                Color.FromArgb(255, 102, 217, 230),
                Color.FromArgb(255, 217, 217, 217),
            };
            var brights = new[]
            {
                Color.FromArgb(255, 140, 140, 140),
                Color.FromArgb(255, 255, 128, 128),
                Color.FromArgb(255, 140, 255, 140),
                Color.FromArgb(255, 255, 242, 140),
                Color.FromArgb(255, 140, 204, 255),
                Color.FromArgb(255, 242, 179, 255),
                Color.FromArgb(255, 140, 255, 255),
                Color.FromArgb(255, 255, 255, 255),
            };
            return bright ? brights[n] : normal[n];
        }
        else
        {
            var normal = new[]
            {
                Color.FromArgb(255, 38, 38, 38),
                Color.FromArgb(255, 191, 38, 38),
                Color.FromArgb(255, 38, 140, 38),
                Color.FromArgb(255, 166, 128, 13),
                Color.FromArgb(255, 38, 89, 191),
                Color.FromArgb(255, 153, 51, 166),
                Color.FromArgb(255, 26, 128, 140),
                Color.FromArgb(255, 140, 140, 140),
            };
            var brights = new[]
            {
                Color.FromArgb(255, 102, 102, 102),
                Color.FromArgb(255, 230, 64, 64),
                Color.FromArgb(255, 51, 179, 51),
                Color.FromArgb(255, 204, 166, 26),
                Color.FromArgb(255, 64, 128, 230),
                Color.FromArgb(255, 191, 89, 204),
                Color.FromArgb(255, 38, 166, 179),
                Color.FromArgb(255, 26, 26, 26),
            };
            return bright ? brights[n] : normal[n];
        }
    }
}