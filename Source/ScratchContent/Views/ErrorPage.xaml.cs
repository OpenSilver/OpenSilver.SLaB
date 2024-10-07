﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SLaB.Navigation.ContentLoaders.Error;

namespace ScratchContent.Views
{
    public partial class ErrorPage : Page
    {
        public ErrorPage()
        {
            InitializeComponent();
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.errorInformation.Content = ErrorPageLoader.GetError(this);
            this.uriLink.NavigateUri = e.Uri;
        }

        private async void ButtonClick(object sender, RoutedEventArgs e)
        {
            await Clipboard.SetTextAsync(ErrorPageLoader.GetError(this).ToString());
        }
    }
}