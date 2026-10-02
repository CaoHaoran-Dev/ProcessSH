using Microsoft.UI.Xaml.Controls;
using ProcessSH.Models;

namespace ProcessSH.Views;

public sealed partial class AboutPage : Page
{
    public AboutPage()
    {
        InitializeComponent();
        ApplyLocalization();
        Localization.LanguageChanged += ApplyLocalization;
    }

    private void ApplyLocalization()
    {
        VersionText.Text = Localization.Get("about.version");
        DescriptionText.Text = Localization.Get("about.description");
        RepoLink.Content = Localization.Get("about.repo");
        CopyrightText.Text = Localization.Get("about.copyright");
    }
}