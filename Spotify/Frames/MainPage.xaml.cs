// MainPage

using Spotify.Controls;
using Spotify.Controls.Announcements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Spotify.Classes;
using System.Threading.Tasks;
using static Spotify.Frames.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Spotify.Frames
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public const double TV_SAFE_HORIZONTAL_MARGINS = 48;
        public const double TV_SAFE_VERTICAL_MARGINS = 27;

        public static ListViewItem currentNavSelection = new ListViewItem();
        public static bool returningFromMemoryReduction = false;
        public static List<long> loadingLocks = new List<long>();
        public static string errorMessage = "";
        public static List<UserControl> announcementItems = new List<UserControl>();
        public static bool closedAnnouncements = false;
        public static Browse browsePage;
        public static Profile profilePage;
        public static Search searchPage;
        public static Settings settingsPage;
        public static YourMusic yourMusicPage;

        /// <summary>
        /// The main page for the Spotify application
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            App.mainPage = this;
        }

        /// <summary>
        /// When the user navigates to the page
        /// </summary>
        /// <param name="e">The navigation event arguments</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // announcements
            if (announcementItems.Count > 0 && !closedAnnouncements)
            {
                ShowAnnouncements(announcementItems);
            }
            else
            {
                AnnouncementsContainer.Visibility = Visibility.Collapsed;
            }

            // settings
            if (Settings.theme == Settings.Theme.Light)
            {
                this.RequestedTheme = ElementTheme.Light;
            }
            else if (Settings.theme == Settings.Theme.Dark)
            {
                this.RequestedTheme = ElementTheme.Dark;
            }
            if (Settings.tvSafeArea)
            {
                SafeAreaOn();
            }
            else
            {
                SafeAreaOff();
            }

            CancelDialog.Visibility = Visibility.Collapsed;
            if (errorMessage != "")
            {
                Errors.Visibility = Visibility.Visible;
                ErrorMessage.Visibility = Visibility.Visible;
                ErrorMessage.Text = errorMessage;
            }
            else
            {
                ErrorMessages.Visibility = Visibility.Collapsed;
            }

            SpotifyLogo.Visibility = Visibility.Collapsed;
            SpotifyLoading.Visibility = Visibility.Collapsed;
            YouTubeLogo.Visibility = Visibility.Collapsed;
            YouTubeLoading.Visibility = Visibility.Collapsed;
            LoadersMessage.Visibility = Visibility.Collapsed;

            SelectHamburgerOption(App.hamburgerOptionToLoadTo, true);
            if (App.hamburgerOptionToLoadTo == "SettingsItem")
            {
                SettingsItem_Click(null, null);
            }
            UpdateUserUI();
            
            if (App.playbackService.showing)
            {
                PlaybackMenu.Visibility = Visibility.Visible;
                if (returningFromMemoryReduction)
                {
                    await PlaybackMenu.UpdateUI();
                    returningFromMemoryReduction = false;
                }
            }
            else
            {
                PlaybackMenu.Visibility = Visibility.Collapsed;
            }

            LoadUserPlaylists();

            // Back button in title bar
            Frame rootFrame = Window.Current.Content as Frame;

            string myPages = "";
            foreach (PageStackEntry page in rootFrame.BackStack)
            {
                myPages += page.SourcePageType.ToString() + "\n";
            }

            if (rootFrame.CanGoBack)
            {
                // Show UI in title bar if opted-in and in-app backstack is not empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else
            {
                // Remove the UI from the title bar if in-app back stack is empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
        }

        /// <summary>
        /// Show the user a list of announcements
        /// </summary>
        /// <param name="announcements"></param>
        public void ShowAnnouncements(List<UserControl> announcements)
        {
            closedAnnouncements = false;
            announcementItems = announcements;
            Announcements.Content = announcementItems[0];
            PreviousAnnouncement.Visibility = Visibility.Collapsed;
            if (announcementItems.Count == 1)
            {
                NextAnnouncement.Visibility = Visibility.Collapsed;
            }
            else
            {
                NextAnnouncement.Visibility = Visibility.Visible;
            }
            AnnouncementsContainer.Visibility = Visibility.Visible;
            Announcements.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Present the user with an announcement popup
        /// </summary>
        /// <param name="announcements"></param>
        /// <param name="settingsPage"></param>
        public void ShowAnnouncements(List<UserControl> announcements, Settings settingsPage)
        {
            MainPage.settingsPage = settingsPage;
            ShowAnnouncements(announcements);
        }

        /// <summary>
        /// Begin loading the Users Playlists
        /// </summary>
        public async void LoadUserPlaylists()
        {
            if (UserProfile.IsLoggedIn())
            {

                YourMusic.preEmptiveLoadPlaylists.Clear();
                if (yourMusicPage != null)
                {
                    yourMusicPage.ClearPlaylists();
                }
                YourMusic.refreshing = true;
                await YourMusic.SetPlaylists();
                YourMusic.refreshing = false;
            }
        }

        /// <summary>
        /// Make the playback controls visible
        /// </summary>
        public async void ShowPlaybackMenu()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MainContentFrame.Margin = new Thickness(0, 0, 0, 0);
                PlaybackMenu.SetRepeat(Settings.repeatEnabled);
                PlaybackMenu.SetShuffle(Settings.shuffleEnabled);
                PlaybackMenu.SetVolume(Settings.volume);

                PlaybackMenu.Visibility = Visibility.Visible;
            });
        }

        /// <summary>
        /// Set the visibility state of the PlaybackMenu progress ring
        /// </summary>
        /// <param name="visible"></param>
        public async void SetPlaybackMenu(bool active)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (PlaybackMenu != null)
                {
                    PlaybackMenu.SetLoadingActive(active);
                }
            });
        }

        /// <summary>
        /// Return the PlaybackMenu control. Needed for static App.playbackService class.
        /// </summary>
        /// <returns>The PlaybackMenu control</returns>
        public Playback GetPlaybackMenu()
        {
            return PlaybackMenu;
        }

        /// <summary>
        /// Toggle the hamburger menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hamburger_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        /// <summary>
        /// Return to the previous frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (MainContentFrame.CanGoBack)
            {
                if (MainContentFrame.BackStack.Count == 0)
                {
                    SelectHamburgerOption("BrowseItem", true);
                }
                else
                {
                    PageStackEntry page = MainContentFrame.BackStack.ElementAt(MainContentFrame.BackStack.Count - 1);
                    HamburgerOptions.SelectionChanged -= HamburgerOptions_SelectionChanged;
                    if (page.SourcePageType == typeof(Browse))
                    {
                        SelectHamburgerOption("BrowseItem", false);
                        Title.Text = "Browse";
                    }
                    else if (page.SourcePageType == typeof(YourMusic))
                    {
                        SelectHamburgerOption("YourMusicItem", false);
                        Title.Text = "Your Music";
                    }
                    else if (page.SourcePageType == typeof(Profile))
                    {
                        SelectHamburgerOption("ProfileItem", false);
                        Title.Text = "Profile";
                    }
                    else if (page.SourcePageType == typeof(Search))
                    {
                        SelectHamburgerOption("SearchItem", false);
                        Title.Text = "Search";
                    }
                    else if (page.SourcePageType == typeof(Settings))
                    {
                        SelectHamburgerOption("SettingsItem", false);
                        Title.Text = "Settings";
                    }
                    HamburgerOptions.SelectionChanged += HamburgerOptions_SelectionChanged;
                }
                MainContentFrame.GoBack();
            }
        }

        /// <summary>
        /// Set the selected option of the hamburger navigation menu
        /// </summary>
        /// <param name="option"></param>
        public void SelectHamburgerOption(string option, Boolean setFocus)
        {
            BrowseItemHighlight.Visibility = Visibility.Collapsed;
            YourMusicItemHighlight.Visibility = Visibility.Collapsed;
            ProfileItemHighlight.Visibility = Visibility.Collapsed;
            SearchItemHighlight.Visibility = Visibility.Collapsed;
            SettingsItemHighlight.Visibility = Visibility.Collapsed;
            BrowseItemExpandedHighlight.Visibility = Visibility.Collapsed;
            YourMusicItemExpandedHighlight.Visibility = Visibility.Collapsed;
            ProfileItemExpandedHighlight.Visibility = Visibility.Collapsed;
            SearchItemExpandedHighlight.Visibility = Visibility.Collapsed;
            SettingsItemExpandedHighlight.Visibility = Visibility.Collapsed;

            HamburgerOptions.SelectedIndex = -1;
            if (option == "SettingsItem")
            {
                SettingsItemHighlight.Visibility = Visibility.Visible;
                SettingsItemExpandedHighlight.Visibility = Visibility.Visible;
                if (setFocus)
                {
                    SettingsItem.Focus(FocusState.Programmatic);
                }
                HamburgerOptions.SelectedItem = null;
                currentNavSelection = null;
            }
            for (int i = 0; i < HamburgerOptions.Items.Count; i++)
            {
                ListViewItem item = (ListViewItem)HamburgerOptions.Items[i];
                if (option == item.Name)
                {
                    item.IsSelected = true;
                    HamburgerOptions.SelectedItem = item;
                    currentNavSelection = item;
                    if (item.Name == "BrowseItem")
                    {
                        BrowseItemHighlight.Visibility = Visibility.Visible;
                        BrowseItemExpandedHighlight.Visibility = Visibility.Visible;
                    }
                    else if (item.Name == "YourMusicItem")
                    {
                        YourMusicItemHighlight.Visibility = Visibility.Visible;
                        YourMusicItemExpandedHighlight.Visibility = Visibility.Visible;
                    }
                    else if (item.Name == "ProfileItem")
                    {
                        ProfileItemHighlight.Visibility = Visibility.Visible;
                        ProfileItemExpandedHighlight.Visibility = Visibility.Visible;
                    }
                    else if (item.Name == "SearchItem")
                    {
                        SearchItemHighlight.Visibility = Visibility.Visible;
                        SearchItemExpandedHighlight.Visibility = Visibility.Visible;
                    }
                    if (setFocus)
                    {
                        item.Focus(FocusState.Programmatic);
                    }
                    break;
                }
                else
                {
                    item.IsSelected = false;
                }
            }
        }

        /// <summary>
        /// User changes the page via the hamburger menu
        /// </summary>
        /// <param name="sender">The hamburger menu which was clicked</param>
        /// <param name="e">The selection changed event arguments</param>
        private void HamburgerOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainSplitView.IsPaneOpen = false;
            if (e.AddedItems.Count > 0)
            {
                ListViewItem selectedItem = (ListViewItem)e.AddedItems[e.AddedItems.Count - 1];
                if (currentNavSelection != null && currentNavSelection.Name == selectedItem.Name)
                {
                    return;
                }
                currentNavSelection = selectedItem;
                currentNavSelection.Focus(FocusState.Programmatic);
                foreach (ListViewItem item in HamburgerOptions.Items)
                {
                    if (item.Name != selectedItem.Name)
                    {
                        item.IsSelected = false;
                    }
                }
                BrowseItemHighlight.Visibility = Visibility.Collapsed;
                YourMusicItemHighlight.Visibility = Visibility.Collapsed;
                ProfileItemHighlight.Visibility = Visibility.Collapsed;
                SearchItemHighlight.Visibility = Visibility.Collapsed;
                SettingsItemHighlight.Visibility = Visibility.Collapsed;
                BrowseItemExpandedHighlight.Visibility = Visibility.Collapsed;
                YourMusicItemExpandedHighlight.Visibility = Visibility.Collapsed;
                ProfileItemExpandedHighlight.Visibility = Visibility.Collapsed;
                SearchItemExpandedHighlight.Visibility = Visibility.Collapsed;
                SettingsItemExpandedHighlight.Visibility = Visibility.Collapsed;
                if (BrowseItem.IsSelected)
                {
                    BrowseItemHighlight.Visibility = Visibility.Visible;
                    BrowseItemExpandedHighlight.Visibility = Visibility.Visible;
                    NavigateToPage(typeof(Browse));
                    Title.Text = "Browse";
                }
                else if (YourMusicItem.IsSelected)
                {
                    YourMusicItemHighlight.Visibility = Visibility.Visible;
                    YourMusicItemExpandedHighlight.Visibility = Visibility.Visible;
                    NavigateToPage(typeof(YourMusic));
                    Title.Text = "Your Music";
                }
                else if (ProfileItem.IsSelected)
                {
                    ProfileItemHighlight.Visibility = Visibility.Visible;
                    ProfileItemExpandedHighlight.Visibility = Visibility.Visible;
                    NavigateToPage(typeof(Profile));
                    Title.Text = "Profile";
                }
                else if (SearchItem.IsSelected)
                {
                    SearchItemHighlight.Visibility = Visibility.Visible;
                    SearchItemExpandedHighlight.Visibility = Visibility.Visible;
                    NavigateToPage(typeof(Search));
                    Title.Text = "Search";
                }
            }
            else if (e.RemovedItems.Count > 0)
            {
                ListViewItem selectedItem = (ListViewItem)e.RemovedItems[e.RemovedItems.Count - 1];
                if (currentNavSelection != null && currentNavSelection.Name == selectedItem.Name)
                {
                    selectedItem.IsSelected = true;
                }
            }
        }

        /// <summary>
        /// Updates the UI of the user information
        /// </summary>
        public void UpdateUserUI()
        {
            UserName.Text = UserProfile.displayName;
            UserPic.ImageSource = UserProfile.userPic;
            if (UserProfile.userId == "")
            {
                BlankUser.Text = "\uE77B";
                UserPicContainer.StrokeThickness = 2;
            }
            else
            {
                UserPicContainer.StrokeThickness = 0.5;
                if (UserProfile.userPic.PixelHeight != 0)
                {
                    UserPic.ImageSource = UserProfile.userPic;
                    BlankUser.Text = "";
                }
                else
                {
                    BlankUser.Text = "\uEA8C";
                }
            }
        }

        /// <summary>
        /// Remove the extra margin so content touches display edge
        /// </summary>
        public void SafeAreaOff()
        {
            NavLeftBorder.Visibility = Visibility.Collapsed;
            NavLeftBorderHamburgerExtension.Visibility = Visibility.Collapsed;
            Header.Margin = new Thickness(0, 0, 0, 0);
            MainSplitView.Margin = new Thickness(0, 0, 0, 0);
            HamburgerOptions.Margin = new Thickness(0, 0, 0, 0);
            MainContentFrame.Margin = new Thickness(0, 0, 0, 0);
            PlaybackMenu.SafeAreaOff();
        }

        /// <summary>
        /// Add extra margin to ensure content inside of TV safe area
        /// </summary>
        public void SafeAreaOn()
        {
            NavLeftBorder.Visibility = Visibility.Visible;
            NavLeftBorderHamburgerExtension.Visibility = Visibility.Visible;
            Header.Margin = new Thickness(TV_SAFE_HORIZONTAL_MARGINS, TV_SAFE_VERTICAL_MARGINS, TV_SAFE_HORIZONTAL_MARGINS, 0);
            MainSplitView.Margin = new Thickness(TV_SAFE_HORIZONTAL_MARGINS, 0, TV_SAFE_HORIZONTAL_MARGINS, 0);
            HamburgerOptions.Margin = new Thickness(0, 0, 0, TV_SAFE_VERTICAL_MARGINS);
            if (!App.playbackService.showing)
            {
                MainContentFrame.Margin = new Thickness(0, 0, 0, TV_SAFE_VERTICAL_MARGINS);
            }
            PlaybackMenu.SafeAreaOn();
        }

        /// <summary>
        /// When a user selects any of the user information elements
        /// </summary>
        /// <param name="sender">The user element that was pressed</param>
        /// <param name="e">The pointer routed event arguments</param>
        private void UserElement_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            SelectHamburgerOption("ProfileItem", true);
        }

        /// <summary>
        /// When key is pressed, check for special key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (AnnouncementsContainer.Visibility != Visibility.Visible)
            {
                if (e.Key == VirtualKey.GamepadView)
                {
                    MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
                }
                else if (e.Key == VirtualKey.GamepadY)
                {
                    SelectHamburgerOption("SearchItem", true);
                }
                else if (e.Key == VirtualKey.GamepadX)
                {
                    if (App.playbackService.showing)
                    {
                        PlaybackMenu.FocusPlayPause();
                    }
                }
                else if (e.Key == VirtualKey.GamepadRightThumbstickButton)
                {
                    if (App.playbackService.showing)
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
                }
                else if (e.Key == VirtualKey.GamepadRightThumbstickRight)
                {
                    if (App.playbackService.showing)
                    {
                        App.playbackService.NextTrack();
                    }
                }
                else if (e.Key == VirtualKey.GamepadRightThumbstickLeft)
                {
                    if (App.playbackService.showing)
                    {
                        App.playbackService.PreviousTrack();
                    }
                }
                else if (e.Key == VirtualKey.Down && e.OriginalSource is Button && ((Button)e.OriginalSource).Name == "Back")
                {
                    MainContentFrame.Focus(FocusState.Programmatic);
                }
                else if (e.Key == VirtualKey.Escape && e.OriginalSource is Slider && ((Slider)e.OriginalSource).Name == "VolumeSlider")
                {
                    PlaybackMenu.VolumeSlider_LostFocus(null, null);
                    PlaybackMenu.FocusOnVolume();
                }
                else if (e.Key == VirtualKey.Escape)
                {
                    Back_Click(null, null);
                }
            }
        }

        /// <summary>
        /// Navigates to the desired page, ensuring all old BackStack references to the page are removed
        /// </summary>
        /// <param name="type"></param>
        private void NavigateToPage(Type type)
        {
            IEnumerable<PageStackEntry> duplicatePages = MainContentFrame.BackStack.Where(page => page.SourcePageType == type);
            if (duplicatePages.Count() > 0)
            {
                foreach (PageStackEntry page in duplicatePages)
                {
                    MainContentFrame.BackStack.Remove(page);
                }
            }
            MainContentFrame.Navigate(type, this);
        }

        /// <summary>
        /// User selects the Settings option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsItem_Click(object sender, RoutedEventArgs e)
        {
            if (MainSplitView.IsPaneOpen)
            {
                MainSplitView.IsPaneOpen = false;
            }
            SelectHamburgerOption("SettingsItem", true);
            NavigateToPage(typeof(Settings));
            Title.Text = "Settings";
        }

        /// <summary>
        /// Grab control of loading to prevent stale UI changes
        /// </summary>
        /// <param name="newLock">The new lock that has sole permission to update loading UI</param>
        public static void AddLoadingLock(long newLock)
        {
            loadingLocks.Add(newLock);
        }

        /// <summary>
        /// Take out hold on loading UI
        /// </summary>
        /// <param name="expiredLock">The lock to remove UI update permissions for</param>
        public static void RemoveLoadingLock(long expiredLock)
        {
            loadingLocks.Remove(expiredLock);
        }

        /// <summary>
        /// Get current key able to update loading UI
        /// </summary>
        /// <returns>The current lock that has permissions to update the loading UI</returns>
        public static long CurrentLock()
        {
            return loadingLocks.Count > 0 ? loadingLocks.Last() : 0;
        }

        /// <summary>
        /// Set the error message displayed to the user
        /// </summary>
        /// <param name="message">The error message to be displayed to the user</param>
        public void SetErrorMessage(string message)
        {
            ErrorMessages.Visibility = Visibility.Visible;
            ErrorMessage.Visibility = Visibility.Visible;
            ErrorMessage.Text = message;
            errorMessage = message;
        }

        /// <summary>
        /// Set the error message displayed to the user
        /// </summary>
        /// <param name="message">The error message to be displayed to the user</param>
        /// <param name="localLock">The lock to ensure no stale updates</param>
        public async void SetErrorMessage(string message, long localLock)
        {
            if (!App.isInBackgroundMode && localLock == App.playbackService.GlobalLock)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (ErrorMessage != null)
                    {
                        ErrorMessages.Visibility = Visibility.Visible;
                        ErrorMessage.Visibility = Visibility.Visible;
                        ErrorMessage.Text = message;
                        errorMessage = message;
                    }
                });
            }
        }

        /// <summary>
        /// User decides to close errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorMessageClose_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessages.Visibility = Visibility.Collapsed;
            ErrorMessage.Text = "";
            errorMessage = "";
        }

        /// <summary>
        /// Show the cancel dialog to let the user cancel the long download
        /// </summary>
        /// <param name="localLock">The lock to ensure no stale updates</param>
        /// <param name="cancelToken">The download token to cancel</param>
        public async void ShowCancelDialog(long localLock, CancellationTokenSource cancelToken, string trackName)
        {
            if (!App.isInBackgroundMode && localLock == App.playbackService.GlobalLock)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CancelDialog.SetCancelToken(cancelToken);
                    CancelDialog.SetTrackName(trackName);
                    CancelDialog.Visibility = Visibility.Visible;
                });
            }
        }

        /// <summary>
        /// Hide the cancel dialog
        /// </summary>
        /// <param name="localLock">The lock to ensure no stale updates</param>
        public async void HideCancelDialog(long localLock)
        {
            if (!App.isInBackgroundMode && localLock == App.playbackService.GlobalLock)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CancelDialog.Visibility = Visibility.Collapsed;
                });
            }
        }

        /// <summary>
        /// Bring up the Spotify logo and loading bar
        /// </summary>
        private void BringUpSpotify()
        {
            YouTubeLogo.Visibility = Visibility.Collapsed;
            YouTubeLoading.Visibility = Visibility.Collapsed;
            LoadersMessage.SetValue(RelativePanel.AboveProperty, SpotifyLoading);
            if (LoadersMessage.Visibility == Visibility.Collapsed)
            {
                LoadersMessage.Visibility = Visibility.Visible;
                LoadersMessage.Text = "";
            }
            SpotifyLogo.Visibility = Visibility.Visible;
            SpotifyLoading.Visibility = Visibility.Visible;
            UserName.SetValue(RelativePanel.RightOfProperty, SpotifyLoading);
        }

        /// <summary>
        /// Bring up the YouTube logo and loading bar
        /// </summary>
        private void BringUpYouTube()
        {
            SpotifyLogo.Visibility = Visibility.Collapsed;
            SpotifyLoading.Visibility = Visibility.Collapsed;
            YouTubeLogo.Visibility = Visibility.Visible;
            YouTubeLoading.Visibility = Visibility.Visible;
            LoadersMessage.SetValue(RelativePanel.AboveProperty, YouTubeLoading);
            if (LoadersMessage.Visibility == Visibility.Collapsed)
            {
                LoadersMessage.Visibility = Visibility.Visible;
                LoadersMessage.Text = "";
            }
            UserName.SetValue(RelativePanel.RightOfProperty, YouTubeLoading);
        }

        /// <summary>
        /// Sets the loading bar progress
        /// </summary>
        /// <param name="source">Whether or not the loading is happening from Spotify or YouTube</param>
        /// <param name="value">The current value of the loading</param>
        /// <param name="max">The point when loading is done</param>
        /// <param name="loadingKey">The key to prevent stale UI</param>
        public void SetLoadingProgress(PlaybackSource source, double value, double max, long loadingKey)
        {
            if (!App.isInBackgroundMode && loadingKey == CurrentLock())
            {
                if (source == PlaybackSource.Spotify)
                {
                    SpotifyLoading.Maximum = max;
                    SpotifyLoading.Value = value;
                    if (SpotifyLogo.Visibility != Visibility.Visible || SpotifyLoading.Visibility == Visibility.Visible)
                    {
                        BringUpSpotify();
                    }
                }
                else if (source == PlaybackSource.YouTube)
                {
                    YouTubeLoading.Maximum = max;
                    YouTubeLoading.Value = value;
                    if (YouTubeLogo.Visibility != Visibility.Visible || YouTubeLoading.Visibility == Visibility.Visible)
                    {
                        BringUpYouTube();
                    }
                }
                
            }            
        }

        /// <summary>
        /// Sets the loading bar progress
        /// </summary>
        /// <param name="source">Whether or not the loading is happening from Spotify or YouTube</param>
        /// <param name="value">The current value of the loading</param>
        /// <param name="max">The point when loading is done</param>
        /// /// <param name="localLock">The lock for the playback session</param>
        /// <param name="loadingKey">The key to prevent stale UI</param>
        public void SetLoadingProgress(PlaybackSource source, double value, double max, long localLock, long loadingKey)
        {
            if (!App.isInBackgroundMode && localLock == App.playbackService.GlobalLock)
            {
                SetLoadingProgress(source, value, max, loadingKey);
            }
        }

        /// <summary>
        /// Set the message displayed above the source loading bar
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        /// <param name="localLock">The lock to ensure only the latest playback session is updating</param>
        /// <param name="loadingKey">The key to prevent stale messages</param>
        public async void SetLoadersMessage(String message, long localLock, long loadingKey)
        {
            if (LoadersMessage != null && !App.isInBackgroundMode && localLock == App.playbackService.GlobalLock && loadingKey == CurrentLock())
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    LoadersMessage.Text = message;
                    LoadersMessage.Visibility = Visibility.Visible;
                });
            }
        }

        /// <summary>
        /// Hide announcements without destroying them
        /// </summary>
        public void HideAnnouncements()
        {
            AnnouncementsContainer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// User wishes to close Announcements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CloseAnnouncements_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["AnnouncementsClosed"] = true;
            closedAnnouncements = true;
            AnnouncementsContainer.Visibility = Visibility.Collapsed;
            UnloadAnnouncements();
            MainContentFrame.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// User moves to the next announcement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NextAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = announcementItems.IndexOf(Announcements.Content as UserControl);
            if (currentIndex < announcementItems.Count - 1)
            {
                Announcements.Content = announcementItems[currentIndex + 1];
                PreviousAnnouncement.Visibility = Visibility.Visible;
                if (currentIndex + 1 == announcementItems.Count - 1)
                {
                    NextAnnouncement.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// User moves to previous announcement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PreviousAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = announcementItems.IndexOf(Announcements.Content as UserControl);
            if (currentIndex > 0)
            {
                Announcements.Content = announcementItems[currentIndex - 1];
                NextAnnouncement.Visibility = Visibility.Visible;
                if (currentIndex - 1 == 0)
                {
                    PreviousAnnouncement.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// User clicks button in announcement popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnnouncementsContainer_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;
            if (e.Key == VirtualKey.Escape)
            {
                CloseAnnouncements_Click(null, null);
            }
            if (e.Key == VirtualKey.GamepadRightShoulder)
            {
                NextAnnouncement_Click(null, null);
            }
            else if (e.Key == VirtualKey.GamepadLeftShoulder)
            {
                PreviousAnnouncement_Click(null, null);
            }
        }

        /// <summary>
        /// Free up memory from announcements
        /// </summary>
        private void UnloadAnnouncements()
        {
            while (announcementItems.Count > 0)
            {
                UserControl announcement = announcementItems.ElementAt(0);
                if (announcement is Welcome)
                {
                    Welcome welcome = announcement as Welcome;
                    announcementItems.Remove(welcome);
                    welcome.UserControl_Unloaded(null, null);
                    welcome.Unloaded -= welcome.UserControl_Unloaded;
                }
                else if (announcement is PlaybackMode)
                {
                    PlaybackMode playbackMode = announcement as PlaybackMode;
                    announcementItems.Remove(playbackMode);
                    playbackMode.UserControl_Unloaded(null, null);
                    playbackMode.Unloaded -= playbackMode.UserControl_Unloaded;
                }
                else if (announcement is ThemeMode)
                {
                    ThemeMode themeMode = announcement as ThemeMode;
                    announcementItems.Remove(themeMode);
                    themeMode.UserControl_Unloaded(null, null);
                    themeMode.Unloaded -= themeMode.UserControl_Unloaded;
                }
                else if (announcement is TvMode)
                {
                    TvMode tvMode = announcement as TvMode;
                    announcementItems.Remove(tvMode);
                    tvMode.UserControl_Unloaded(null, null);
                    tvMode.Unloaded -= tvMode.UserControl_Unloaded;
                }
                else if (announcement is PlaybackOptions)
                {
                    PlaybackOptions playbackOptions = announcement as PlaybackOptions;
                    announcementItems.Remove(playbackOptions);
                    playbackOptions.UserControl_Unloaded(null, null);
                    playbackOptions.Unloaded -= playbackOptions.UserControl_Unloaded;
                }
                else if (announcement is Shuffle)
                {
                    Shuffle shuffle = announcement as Shuffle;
                    announcementItems.Remove(shuffle);
                    shuffle.UserControl_Unloaded(null, null);
                    shuffle.Unloaded -= shuffle.UserControl_Unloaded;
                }
                else if (announcement is NewReleases)
                {
                    NewReleases newReleases = announcement as NewReleases;
                    announcementItems.Remove(newReleases);
                    newReleases.UserControl_Unloaded(null, null);
                    newReleases.Unloaded -= newReleases.UserControl_Unloaded;
                }
            }
        }

        /// <summary>
        /// Used when freeing memory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (App.isInBackgroundMode)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    this.KeyDown -= Page_KeyUp;
                    currentNavSelection = null;
                    loadingLocks.Clear();

                    // direct elements
                    UserName.PointerReleased -= UserElement_PointerReleased;
                    UserPicContainer.PointerReleased -= UserElement_PointerReleased;
                    BlankUser.PointerReleased -= UserElement_PointerReleased;
                    Hamburger.Click -= Hamburger_Click;
                    Back.Click -= Back_Click;
                    HamburgerOptions.SelectionChanged -= HamburgerOptions_SelectionChanged;
                    SettingsItem.Click -= SettingsItem_Click;

                    CancelDialog.Unload();
                    PlaybackMenu.UserControl_Unloaded(null, null);
                    PlaybackMenu.Unloaded -= PlaybackMenu.UserControl_Unloaded;

                    // dependant pages
                    await Task.Run(() =>
                    {
                        if (browsePage != null)
                        {
                            browsePage.Page_Unloaded(null, null);
                        }
                        if (profilePage != null)
                        {
                            profilePage.Page_Unloaded(null, null);
                        }
                        if (yourMusicPage != null)
                        {
                            yourMusicPage.Page_Unloaded(null, null);
                        }
                        if (searchPage != null)
                        {
                            searchPage.Page_Unloaded(null, null);
                        }
                        if (settingsPage != null)
                        {
                            settingsPage.Page_Unloaded(null, null);
                        }                        
                    });
                    if (browsePage != null)
                    {
                        browsePage.Unloaded -= browsePage.Page_Unloaded;
                    }
                    if (profilePage != null)
                    {
                        profilePage.Unloaded -= profilePage.Page_Unloaded;
                    }
                    if (yourMusicPage != null)
                    {
                        yourMusicPage.Unloaded -= yourMusicPage.Page_Unloaded;
                    }
                    if (searchPage != null)
                    {
                        searchPage.Unloaded -= searchPage.Page_Unloaded;
                    }
                    if (settingsPage != null)
                    {
                        settingsPage.Unloaded -= settingsPage.Page_Unloaded;
                    }

                    // announcements
                    Announcements.Content = null;
                    PreviousAnnouncement.Click -= PreviousAnnouncement_Click;
                    NextAnnouncement.Click -= NextAnnouncement_Click;
                    CloseAnnouncements.Click -= CloseAnnouncements_Click;
                    AnnouncementsContainer.KeyUp -= AnnouncementsContainer_KeyUp;
                });
            }
        }
    }
}
