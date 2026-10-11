using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using ProcessSH.Models;
using Windows.System;

namespace ProcessSH.Views;

public sealed partial class HistorySearchPage : Page
{
    public event Action<string>? CommandChosen;
    public event Action? CloseRequested;

    private List<HistoryEntry> _results = new();

    public HistorySearchPage()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            SearchBox.Focus(FocusState.Programmatic);
            Refresh("");
        };
    }

    private void Refresh(string query)
    {
        _results = CommandHistory.Shared.Search(query ?? string.Empty);
        ResultList.ItemsSource = _results;
        if (_results.Count > 0) ResultList.SelectedIndex = 0;
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            Refresh(sender.Text);
    }

    private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Down:
                if (ResultList.Items.Count > 0)
                {
                    ResultList.SelectedIndex = Math.Min(ResultList.SelectedIndex + 1, ResultList.Items.Count - 1);
                    ResultList.ScrollIntoView(ResultList.SelectedItem);
                }
                e.Handled = true;
                break;

            case VirtualKey.Up:
                if (ResultList.Items.Count > 0)
                {
                    ResultList.SelectedIndex = Math.Max(ResultList.SelectedIndex - 1, 0);
                    ResultList.ScrollIntoView(ResultList.SelectedItem);
                }
                e.Handled = true;
                break;

            case VirtualKey.Enter:
                ApplySelection();
                e.Handled = true;
                break;

            case VirtualKey.Escape:
                CloseRequested?.Invoke();
                e.Handled = true;
                break;
        }
    }

    private void ResultList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is HistoryEntry entry)
            CommandChosen?.Invoke(entry.Command);
    }

    private void ApplySelection()
    {
        if (ResultList.SelectedItem is HistoryEntry entry)
            CommandChosen?.Invoke(entry.Command);
    }
}