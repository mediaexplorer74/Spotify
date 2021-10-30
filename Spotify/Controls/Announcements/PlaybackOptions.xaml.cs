// Playback Options

using Spotify.Frames;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Spotify.Controls.Announcements
{
    public sealed partial class PlaybackOptions : UserControl
    {
        public PlaybackOptions()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Construct the object with a pre-defined source
        /// </summary>
        /// <param name="repeatEnabled">Whether repeat is currently enabled or not</param>
        /// <param name="shuffleEnabled">Whether shuffle is currently enabled or not</param>
        public PlaybackOptions(bool repeatEnabled, bool shuffleEnabled) : this()
        {
            RepeatSwitch.Toggled -= RepeatSwitch_Toggled;
            ShuffleSwitch.Toggled -= ShuffleSwitch_Toggled;
            RepeatSwitch.IsOn = repeatEnabled;
            ShuffleSwitch.IsOn = shuffleEnabled;
            RepeatSwitch.Toggled += RepeatSwitch_Toggled;
            ShuffleSwitch.Toggled += ShuffleSwitch_Toggled;
        }

        /// <summary>
        /// User chooses to toggle on or off playback repeat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RepeatSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (MainPage.settingsPage != null)
            {
                MainPage.settingsPage.ToggleRepeat();
            }
            else
            {
                Settings.SetRepeat(RepeatSwitch.IsOn);
            }
        }

        /// <summary>
        /// User chooses to toggle on or off playback shuffle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShuffleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (MainPage.settingsPage != null)
            {
                MainPage.settingsPage.ToggleShuffle();
            }
            else
            {
                Settings.SetShuffle(ShuffleSwitch.IsOn);
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
                    RepeatSwitch.Toggled -= RepeatSwitch_Toggled;
                    ShuffleSwitch.Toggled -= ShuffleSwitch_Toggled;
                });
            }
        }
    }
}
