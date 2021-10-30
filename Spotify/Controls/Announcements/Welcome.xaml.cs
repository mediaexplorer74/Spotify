// Welcome 

using Spotify.Frames;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Spotify.Controls.Announcements
{
    public sealed partial class Welcome : UserControl
    {
        /// <summary>
        /// Main constructor
        /// </summary>
        public Welcome()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// User wishes to close the Announcements flipview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            App.mainPage.CloseAnnouncements_Click(null, null);
        }

        /// <summary>
        /// User wishes to continue on and configure settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {

            App.mainPage.NextAnnouncement_Click(null, null);
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
                    Close.Click -= Close_Click;
                    Settings.Click -= Settings_Click;
                });
            }
        }
    }
}
