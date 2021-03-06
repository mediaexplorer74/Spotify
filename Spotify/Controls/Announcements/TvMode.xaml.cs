// TV Mode

using Spotify.Frames;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Spotify.Controls.Announcements
{
    public sealed partial class TvMode : UserControl
    {
        /// <summary>
        /// Main constructor
        /// </summary>
        public TvMode()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Construct the object with a predetermined toggle value
        /// </summary>
        /// <param name="enabled"></param>
        public TvMode(bool enabled) : this()
        {
            TvModeSwitch.Toggled -= TvModeSwitch_Toggled;
            TvModeSwitch.IsOn = enabled;
            TvModeSwitch.Toggled += TvModeSwitch_Toggled;
        }

        /// <summary>
        /// User wishes to change the TV mode setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TvModeSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            bool enabled = (sender as ToggleSwitch).IsOn;
            if (MainPage.settingsPage != null)
            {
                MainPage.settingsPage.SetTvSafeUI(enabled);
            }
            else
            {
                Settings.SetTvSafe(enabled);
                if (enabled)
                {
                    App.mainPage.SafeAreaOn();
                }
                else
                {
                    App.mainPage.SafeAreaOff();
                }
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
                    TvModeSwitch.Toggled -= TvModeSwitch_Toggled;
                });
            }
        }
    }
}
