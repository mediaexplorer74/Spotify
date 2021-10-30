// New Releases

using Spotify.Frames;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Spotify.Controls.Announcements
{
    public sealed partial class NewReleases : UserControl
    {
        public NewReleases()
        {
            this.InitializeComponent();
        }

        private void ToPage_Click(object sender, RoutedEventArgs e)
        {
            if (App.mainPage != null)
            {
                App.mainPage.HideAnnouncements();
                App.mainPage.SelectHamburgerOption("BrowseItem", true);
                if (MainPage.browsePage != null)
                {
                    MainPage.browsePage.GoToNewReleases();
                }
                App.mainPage.CloseAnnouncements_Click(null, null);
            }
        }

        /// <summary>
        /// Free up memory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (MainPage.closedAnnouncements || (App.isInBackgroundMode && MainPage.closedAnnouncements))
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ToPage.Click -= ToPage_Click;
                });
            }
        }
    }
}
