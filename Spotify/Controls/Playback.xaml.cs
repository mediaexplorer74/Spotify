// Playback

using Spotify.Frames;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Spotify.Controls
{
    /// <summary>
    /// Used to control the playback of songs
    /// </summary>
    public sealed partial class Playback : UserControl
    {
        private DispatcherTimer uiUpdateTimer;
        private static bool loading = false;

        /// <summary>
        /// Main constructor
        /// </summary>
        public Playback()
        {
            this.InitializeComponent();
            TvSafeBottomBorder.Height = MainPage.TV_SAFE_VERTICAL_MARGINS;
            uiUpdateTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            uiUpdateTimer.Tick += UiUpdateTimer_Tick;
        }

        /// <summary>
        /// Updates the UI with the information of the currently playing track
        /// </summary>
        public async Task UpdateUI()
        {
            if (Settings.repeatEnabled)
            {
                Repeat.Visibility = Visibility.Collapsed;
                RepeatEnabled.Visibility = Visibility.Visible;
            }
            if (Settings.shuffleEnabled)
            {
                Shuffle.Visibility = Visibility.Collapsed;
                ShuffleEnabled.Visibility = Visibility.Visible;
            }
            if (App.playbackService.currentlyPlayingItem != null)
            {
                MediaItemDisplayProperties displayProperties = App.playbackService.currentlyPlayingItem.GetDisplayProperties();
                TrackName.Text = displayProperties.MusicProperties.Title;
                TrackArtist.Text = displayProperties.MusicProperties.AlbumTitle;
                if (displayProperties.Thumbnail != null)
                {
                    IRandomAccessStreamWithContentType thumbnail = await displayProperties.Thumbnail.OpenReadAsync();
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(thumbnail);
                    AlbumArt.Source = bitmapImage;
                }
                if (App.playbackService.Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    Play.Visibility = Visibility.Collapsed;
                    uiUpdateTimer.Start();
                }
                else
                {
                    Pause.Visibility = Visibility.Collapsed;
                }
                UpdateProgressUI();
                LoadingTrack.IsActive = false;
            }
            else if (loading)
            {
                SetLoadingActive(true);
            }
            else
            {
                LoadingTrack.IsActive = false;
            }
        }

        /// <summary>
        /// Set the currently playing track image
        /// </summary>
        /// <param name="image">The image of the currently playing song</param>
        public void SetTrackImage(BitmapImage image)
        {
            AlbumArt.Source = image;
        }

        /// <summary>
        /// Set the currently playing track name
        /// </summary>
        /// <param name="name">The name of the currently playing song</param>
        public void SetTrackName(String name)
        {
            TrackName.Text = name;
        }

        /// <summary>
        /// Set the currently playing track album name
        /// </summary>
        /// <param name="name">The name of the album of the currently playing song</param>
        public void SetArtistName(String name)
        {
            TrackArtist.Text = name;
        }

        /// <summary>
        /// Adjust the UI according to play/pause state
        /// </summary>
        /// <param name="state">The current playing state</param>
        public async void SetActionState(MediaPlaybackState state)
        {
            if (state == MediaPlaybackState.Playing)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    uiUpdateTimer.Start();
                    bool manualClick = Play.FocusState == FocusState.Keyboard;
                    Play.Visibility = Visibility.Collapsed;
                    Pause.Visibility = Visibility.Visible;
                    if (manualClick)
                    {
                        Pause.Focus(FocusState.Programmatic);
                    }
                });
            }
            else
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    uiUpdateTimer.Stop();
                    bool manualClick = Pause.FocusState == FocusState.Keyboard;
                    Play.Visibility = Visibility.Visible;
                    Pause.Visibility = Visibility.Collapsed;
                    if (manualClick)
                    {
                        Play.Focus(FocusState.Programmatic);
                    }
                });
            }
        }

        /// <summary>
        /// Remove the extra margin so content touches display edge
        /// </summary>
        public void SafeAreaOff()
        {
            TvSafeBottomBorder.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Add extra margin to ensure content inside of TV safe area
        /// </summary>
        public void SafeAreaOn()
        {
            TvSafeBottomBorder.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Update the UI with playback progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UiUpdateTimer_Tick(object sender, object e)
        {
            if (App.playbackService.Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                UpdateProgressUI();
            }
        }

        /// <summary>
        /// Update the UI elements showing track playback progress
        /// </summary>
        private void UpdateProgressUI()
        {
            Progress.Maximum = App.playbackService.Player.PlaybackSession.NaturalDuration.TotalSeconds;
            Progress.Value = App.playbackService.Player.PlaybackSession.Position.TotalSeconds;

            TimeSpan valueTime = TimeSpan.FromSeconds(Progress.Value);
            if (valueTime.TotalHours < 1)
            {
                CurrentTime.Text = (valueTime).ToString(@"mm\:ss");
            }
            else
            {
                CurrentTime.Text = Math.Floor(valueTime.TotalHours).ToString() + ":" + (valueTime).ToString(@"mm\:ss");
            }
            TimeSpan maxTime = TimeSpan.FromSeconds(Progress.Maximum - App.playbackService.Player.PlaybackSession.Position.TotalSeconds);
            if (maxTime.TotalHours < 1)
            {
                Duration.Text = maxTime.ToString(@"mm\:ss");
            }
            else
            {
                Duration.Text = (Math.Floor(maxTime.TotalHours)).ToString() + ":" + (maxTime).ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// When user selects to play or pause the current song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (App.playbackService.Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                App.playbackService.Player.Pause();
            }
            else
            {
                App.playbackService.Player.Play();
            }
        }
        
        /// <summary>
        /// User selects to skip to the previous song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Previous_Click(object sender, RoutedEventArgs e)
        {
            App.playbackService.PreviousTrack();
            if (App.playbackService.Player.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MediaPlaybackItem newTrack = null;
                    while (newTrack == null)
                    {
                        newTrack = App.playbackService.queue.CurrentItem;
                    }
                    Progress.Maximum = newTrack.Source.Duration.Value.TotalSeconds;
                    Progress.Value = 0;
                    CurrentTime.Text = (TimeSpan.FromSeconds(0)).ToString(@"h\:mm\:ss");
                    TimeSpan maxTime = TimeSpan.FromSeconds(Progress.Maximum);
                    if (maxTime.TotalHours < 1)
                    {
                        Duration.Text = maxTime.ToString(@"mm\:ss");
                    }
                    else
                    {
                        Duration.Text = (Math.Floor(maxTime.TotalHours)).ToString() + ":" + (maxTime).ToString(@"mm\:ss");
                    }
                });
            }
        }

        /// <summary>
        /// User selects to skip to the next song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            App.playbackService.NextTrack();
            if (App.playbackService.Player.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MediaPlaybackItem newTrack = null;
                    while (newTrack == null)
                    {
                        newTrack = App.playbackService.queue.CurrentItem;
                    }
                    Progress.Maximum = newTrack.Source.Duration.Value.TotalSeconds;
                    Progress.Value = 0;

                    CurrentTime.Text = (TimeSpan.FromSeconds(0)).ToString(@"h\:mm\:ss");
                    TimeSpan maxTime = TimeSpan.FromSeconds(Progress.Maximum);
                    if (maxTime.TotalHours < 1)
                    {
                        Duration.Text = maxTime.ToString(@"mm\:ss");
                    }
                    else
                    {
                        Duration.Text = (Math.Floor(maxTime.TotalHours)).ToString() + ":" + (maxTime).ToString(@"mm\:ss");
                    }
                });
            }
        }

        /// <summary>
        /// User selects to toggle repeating of the playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            bool repeatOn = App.playbackService.ToggleRepeat();
            if (repeatOn)
            {
                Repeat.Visibility = Visibility.Collapsed;
                RepeatEnabled.Visibility = Visibility.Visible;
                RepeatEnabled.Focus(FocusState.Programmatic);
            }
            else
            {
                Repeat.Visibility = Visibility.Visible;
                RepeatEnabled.Visibility = Visibility.Collapsed;
                Repeat.Focus(FocusState.Programmatic);
            }
        }

        /// <summary>
        /// Sets whether or not the playlist automatically repeats
        /// </summary>
        /// <param name="enabled">True to make the playlist automatically repeat, false otherwise</param>
        public void SetRepeat(bool enabled)
        {
            if (enabled)
            {
                Repeat.Visibility = Visibility.Collapsed;
                RepeatEnabled.Visibility = Visibility.Visible;
            }
            else
            {
                Repeat.Visibility = Visibility.Visible;
                RepeatEnabled.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// User selects to toggle shuffling of the playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            bool shuffleOn = App.playbackService.ToggleShuffle();
            if (shuffleOn)
            {
                Shuffle.Visibility = Visibility.Collapsed;
                ShuffleEnabled.Visibility = Visibility.Visible;
                ShuffleEnabled.Focus(FocusState.Programmatic);
            }
            else
            {
                Shuffle.Visibility = Visibility.Visible;
                ShuffleEnabled.Visibility = Visibility.Collapsed;
                Shuffle.Focus(FocusState.Programmatic);
            }
        }

        /// <summary>
        /// Sets whether or not the playlist automatically shuffles
        /// </summary>
        /// <param name="enabled">True to make the playlist automatically shuffle, false otherwise</param>
        public void SetShuffle(bool enabled)
        {
            if (enabled)
            {
                Shuffle.Visibility = Visibility.Collapsed;
                ShuffleEnabled.Visibility = Visibility.Visible;
            }
            else
            {
                Shuffle.Visibility = Visibility.Visible;
                ShuffleEnabled.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// User selects to change volume
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Volume_Click(object sender, RoutedEventArgs e)
        {
            VolumeSlider.Value = App.playbackService.Player.Volume * 100;
            Volume.Visibility = Visibility.Collapsed;
            if (Settings.repeatEnabled)
            {
                RepeatEnabled.Visibility = Visibility.Collapsed;
            }
            else
            {
                Repeat.Visibility = Visibility.Collapsed;
            }
            if (Settings.shuffleEnabled)
            {
                ShuffleEnabled.Visibility = Visibility.Collapsed;
            }
            else
            {
                Shuffle.Visibility = Visibility.Collapsed;
            }
            VolumeSlider.Visibility = Visibility.Visible;
            VolumeSlider.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Sets the volume for playback
        /// </summary>
        /// <param name="volume">The volume, 0 to 100</param>
        public void SetVolume(double volume)
        {
            App.playbackService.Player.Volume = volume;
            VolumeSlider.Value = volume;
            if (VolumeSlider.Value == 0)
            {
                Volume.Content = "\uE74F";
            }
            else if (VolumeSlider.Value > 0 && VolumeSlider.Value <= 33)
            {
                Volume.Content = "\uE993";
            }
            else if (VolumeSlider.Value > 30 && VolumeSlider.Value <= 66)
            {
                Volume.Content = "\uE994";
            }
            else
            {
                Volume.Content = "\uE995";
            }
        }

        /// <summary>
        /// User leaves the volume slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void VolumeSlider_LostFocus(object sender, RoutedEventArgs e)
        {
            Volume.Visibility = Visibility.Visible;
            VolumeSlider.Visibility = Visibility.Collapsed;
            RepeatEnabled.Visibility = Visibility.Visible;
            if (Settings.repeatEnabled)
            {
                RepeatEnabled.Visibility = Visibility.Visible;
                Repeat.Visibility = Visibility.Collapsed;
            }
            else
            {
                Repeat.Visibility = Visibility.Visible;
                RepeatEnabled.Visibility = Visibility.Collapsed;
            }
            if (Settings.shuffleEnabled)
            {
                ShuffleEnabled.Visibility = Visibility.Visible;
                Shuffle.Visibility = Visibility.Collapsed;
            }
            else
            {
                Shuffle.Visibility = Visibility.Visible;
                ShuffleEnabled.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Set focus on the volume button
        /// </summary>
        public void FocusOnVolume()
        {
            Volume.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// User changes the volume level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VolumeSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            App.playbackService.Player.Volume = VolumeSlider.Value / 100;
            if (VolumeSlider.Value == 0)
            {
                Volume.Content = "\uE74F";
            }
            else if (VolumeSlider.Value > 0 && VolumeSlider.Value <= 33)
            {
                Volume.Content = "\uE993";
            }
            else if (VolumeSlider.Value > 30 && VolumeSlider.Value <= 66)
            {
                Volume.Content = "\uE994";
            }
            else
            {
                Volume.Content = "\uE995";
            }
            Settings.volume = VolumeSlider.Value;
            Settings.SaveSettings();
        }

        /// <summary>
        /// Move UI focus to the Play/Pause button
        /// </summary>
        public void FocusPlayPause()
        {
            if (Play.Visibility == Visibility.Visible)
            {
                Play.Focus(FocusState.Programmatic);
            }
            else if (Pause.Visibility == Visibility.Visible)
            {
                Pause.Focus(FocusState.Programmatic);
            }
        }

        /// <summary>
        /// Set the visibility of the loading progress ring
        /// </summary>
        /// <param name="visible"></param>
        public void SetLoadingActive(bool active)
        {
            loading = active;
            LoadingTrack.IsActive = active;
            if (active)
            {
                uiUpdateTimer.Stop();
                Next.Click -= Next_Click;
                Previous.Click -= Previous_Click;
                Play.Click -= PlayPause_Click;
                Pause.Click -= PlayPause_Click;
                Play.Visibility = Visibility.Visible;
                Pause.Visibility = Visibility.Collapsed;
                TrackName.Text = "";
                TrackArtist.Text = "";
                Progress.Value = 0;
                CurrentTime.Text = "00:00";
                Duration.Text = "00:00";
                AlbumArt.Source = new BitmapImage();
            }
            else
            {
                uiUpdateTimer.Start();
                Play.Visibility = Visibility.Collapsed;
                Pause.Visibility = Visibility.Visible;
                Next.Click += Next_Click;
                Previous.Click += Previous_Click;
                Play.Click += PlayPause_Click;
                Pause.Click += PlayPause_Click;
            }
        }

        /// <summary>
        /// Free up memory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (App.isInBackgroundMode)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    uiUpdateTimer.Tick -= UiUpdateTimer_Tick;
                    Repeat.Click -= Repeat_Click;
                    RepeatEnabled.Click -= Repeat_Click;
                    Volume.Click -= Volume_Click;
                    VolumeSlider.LostFocus -= VolumeSlider_LostFocus;
                    VolumeSlider.ValueChanged -= VolumeSlider_ValueChanged;
                    Play.Click -= PlayPause_Click;
                    Pause.Click -= PlayPause_Click;
                    Previous.Click -= Previous_Click;
                    Next.Click -= Next_Click;
                });
            }
        }
    }
}
