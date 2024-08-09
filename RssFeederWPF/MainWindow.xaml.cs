using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace RssFeederWPF
{
    /// <summary>
    /// Основное окно приложения.
    /// </summary>
    public partial class MainWindow : Window
    {
        private RssService _rssService;
        private DispatcherTimer _updateTimer;
        private AppConfig _config;
        private readonly Dictionary<string, dynamic> _tabItemsData = new Dictionary<string, dynamic>();
        private int tabCount = 0;

        /// <summary>
        /// Инициализирует новый экземпляр окна.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            LoadConfiguration();
            InitializeHttpClient();
            LoadRssFeedAsync();
            InitializeUpdateTimer();
        }

        private void LoadConfiguration()
        {
            _config = ConfigLoader.LoadConfig("config.json");
        }

        private void InitializeHttpClient()
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();

            // Проверка настроек прокси
            if (_config.ProxySettings != null && !string.IsNullOrEmpty(_config.ProxySettings.Address)) 
            {
                WebProxy proxy = new WebProxy(_config.ProxySettings.Address);

                // Проверка учетных записей
                if (!string.IsNullOrEmpty(_config.ProxySettings.Username))
                {
                    proxy.Credentials = new NetworkCredential(_config.ProxySettings.Username, _config.ProxySettings.Password);
                }
                httpClientHandler.Proxy = proxy;
            }

            var httpClient = new HttpClient(httpClientHandler);

            _rssService = new RssService(httpClient);
        }

        private void InitializeUpdateTimer()
        {
            // Создание таймера обновления 
            _updateTimer = new DispatcherTimer();

            _updateTimer.Tick += async (s, e) => await LoadRssFeedAsync();

            if (_config.UpdateIntervalSeconds < 7 || _config.UpdateIntervalSeconds > 300)
            {
                _config.UpdateIntervalSeconds = 30;
            }
            _updateTimer.Interval = TimeSpan.FromSeconds(_config.UpdateIntervalSeconds);

            _updateTimer.Start();
        }

        private async Task LoadRssFeedAsync()
        {
            var notifications = new List<string>();

            try
            {
                if (tabCount == 0)
                {
                    CreateTabs();
                }
                await UpdateTabsAsync(notifications);
            }
            catch (Exception ex)
            {
                notifications.Add($"Ошибка при загрузке RSS ленты: {ex.Message}");
            }
            finally
            {
                NotificationTextBlock.Text = string.Join(Environment.NewLine, notifications);
            }
        }

        private void CreateTabs()
        {
            // Создание вкладки
            if (_config?.RssFeeds != null)
            {
                foreach (var feed in _config.RssFeeds)
                {
                    string tabName = $"Tab{tabCount}";

                    TabItem newTabItem = CreateTabItem(tabName, feed.SiteName);

                    MainTabControl.Items.Add(newTabItem);

                    tabCount++; // Счетчик элементов 
                }
            }
        }

        private TabItem CreateTabItem(string tabName, string header)
        {
            var tabItem = new TabItem { Name = tabName, Header = header };

            var grid = CreateTabGrid(tabName);

            tabItem.Content = grid;

            return tabItem;
        }

        private Grid CreateTabGrid(string tabName)
        {
            // Новая лента
            var grid = new Grid();

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var listBox = new ListBox { Name = $"ListBox{tabName}", Margin = new Thickness(10), DisplayMemberPath = "Title" };

            listBox.SelectionChanged += RssListBox_SelectionChanged;
            listBox.MouseDoubleClick += RssListBox_MouseDoubleClick;

            Grid.SetRow(listBox, 1);

            var dateTextBlock = new TextBlock { Name = $"DateTextBlock{tabName}", Margin = new Thickness(10), FontWeight = FontWeights.Bold };
            Grid.SetRow(dateTextBlock, 2);

            var descriptionTextBlock = new TextBlock { Name = $"DescriptionTextBlock{tabName}", Margin = new Thickness(10), TextWrapping = TextWrapping.Wrap };
            Grid.SetRow(descriptionTextBlock, 3);

            grid.Children.Add(listBox);
            grid.Children.Add(dateTextBlock);
            grid.Children.Add(descriptionTextBlock);

            _tabItemsData[tabName] = new { ListBox = listBox, DateTextBlock = dateTextBlock, DescriptionTextBlock = descriptionTextBlock };

            return grid;
        }

        private async Task UpdateTabsAsync(List<string> notifications)
        {
            // Обновление ленты, при соблюдении условий
            if (_config?.RssFeeds != null)
            {
                int index = 0;
                foreach (var feed in _config.RssFeeds)
                {
                    if (_tabItemsData.TryGetValue($"Tab{index}", out var tabData))
                    {
                        try
                        {
                            ClearTabContent(tabData);

                            // Загрузить новые данные
                            List<RssItem> items = await _rssService.GetRssFeedAsync(feed.RssUrl);

                            if (items == null || items.Count == 0)
                            {
                                notifications.Add($"Лента от сайта '{feed.SiteName}' пуста или не удалось загрузить данные.");
                                ClearTabContent(tabData);
                            }
                            else
                            {
                                tabData.ListBox.ItemsSource = items;
                                ClearErrors();
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            ClearTabContent(tabData);
                            tabData.ListBox.ItemsSource = null;
                            notifications.Add($"Ошибка при загрузке ленты от сайта '{feed.SiteName}': {ex.Message}");
                        }
                    }
                    index++;
                }
            }
        }

        private void ClearTabContent(dynamic tabData) // Очистка ленты (без заголовков)
        {
            tabData.ListBox.ItemsSource = null;
            tabData.DateTextBlock.Text = string.Empty;
            tabData.DescriptionTextBlock.Text = string.Empty;
        }

        private void ClearErrors() // Очистка ошибок
        {
            NotificationTextBlock.Text = string.Empty;
        }

        private void RssListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is RssItem selectedItem)
            {
                var tabItem = (TabItem)MainTabControl.SelectedItem;
                if (tabItem != null && _tabItemsData.TryGetValue(tabItem.Name, out var tabData))
                {
                    tabData.DateTextBlock.Text = $"Дата: {selectedItem.PublishDate}";
                    tabData.DescriptionTextBlock.Text = selectedItem.Description;
                }
            }
            else
            {
                var tabItem = (TabItem)MainTabControl.SelectedItem;
                if (tabItem != null && _tabItemsData.TryGetValue(tabItem.Name, out var tabData))
                {
                    tabData.DateTextBlock.Text = string.Empty;
                    tabData.DescriptionTextBlock.Text = string.Empty;
                }
            }
        }

        private void RssListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is RssItem selectedItem)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = selectedItem.Link,
                    UseShellExecute = true
                });
            }
        }

        private async void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(_config);

            settingsWindow.UpdateIntervalChanged += async (interval) =>
            {
                _config.UpdateIntervalSeconds = interval;
                _updateTimer.Interval = TimeSpan.FromSeconds(interval);

                await LoadRssFeedAsync();
            };

            if (settingsWindow.ShowDialog() == true)
            {
                InitializeHttpClient();
                await LoadRssFeedAsync();  // Обновление после закрытия настроек
            }
        }

        private async void AddFeed_Click(object sender, RoutedEventArgs e)
        {
            AddFeedWindow addFeedWindow = new AddFeedWindow();
            if (addFeedWindow.ShowDialog() == true)
            {
                var newFeed = new RssFeedConfig { SiteName = addFeedWindow.SiteName, RssUrl = addFeedWindow.RssUrl };
                _config.RssFeeds.Add(newFeed);

                string tabName = $"Tab{tabCount}";
                TabItem newTabItem = CreateTabItem(tabName, newFeed.SiteName);
                MainTabControl.Items.Add(newTabItem);
                tabCount++;

                await UpdateTabsAsync(new List<string>());
                ConfigLoader.SaveConfig("config.json", _config); // Сохранение изменений
            }
        }

        private async void RemoveFeed_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedItem is TabItem selectedTab)
            {
                var feedToRemove = _config.RssFeeds[MainTabControl.SelectedIndex];
                _config.RssFeeds.Remove(feedToRemove);
                MainTabControl.Items.Remove(selectedTab);

                await LoadRssFeedAsync();  // Перезагрузка оставшихся лент
                ConfigLoader.SaveConfig("config.json", _config); // Сохранение изменений
                tabCount--;
            }
        }

        private async void EditFeed_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedItem is TabItem selectedTab)
            {
                var feedToEdit = _config.RssFeeds[MainTabControl.SelectedIndex];
                EditFeedWindow editFeedWindow = new EditFeedWindow(feedToEdit);
                if (editFeedWindow.ShowDialog() == true)
                {
                    feedToEdit.SiteName = editFeedWindow.SiteName;
                    feedToEdit.RssUrl = editFeedWindow.RssUrl;

                    selectedTab.Header = editFeedWindow.SiteName;

                    await UpdateTabsAsync(new List<string>()); 
                    ConfigLoader.SaveConfig("config.json", _config);
                }
            }
        }

        private void ToggleFeed_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedItem is TabItem selectedTab)
            {
                selectedTab.Visibility = selectedTab.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ManageFeedsVisibility_Click(object sender, RoutedEventArgs e)
        {
            var visibilityWindow = new FeedVisibilityWindow(_config.RssFeeds);
            if (visibilityWindow.ShowDialog() == true)
            {
                var updatedVisibilityItems = visibilityWindow.GetUpdatedVisibility();
                for (int i = 0; i < _config.RssFeeds.Count; i++)
                {
                    _config.RssFeeds[i].IsVisible = updatedVisibilityItems[i].IsVisible;
                }

                UpdateTabVisibility();
                ConfigLoader.SaveConfig("config.json", _config);
            }
        }

        private void UpdateTabVisibility()
        {
            for (int i = 0; i < MainTabControl.Items.Count; i++)
            {
                if (MainTabControl.Items[i] is TabItem tabItem)
                {
                    tabItem.Visibility = _config.RssFeeds[i].IsVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

    }
}
