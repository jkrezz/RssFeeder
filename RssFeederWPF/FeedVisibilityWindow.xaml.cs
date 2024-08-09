using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RssFeederWPF
{
    /// <summary>
    /// Окно для управление видимостью существующих RSS ленты.
    /// </summary>
    public partial class FeedVisibilityWindow : Window
    {
        private readonly List<RssFeedConfig> _feedVisibilityItems;


        /// <summary>
        /// Инициализирует окно управления видимостью лент RSS.
        /// </summary>
        /// <param name="rssFeeds">Список конфигураций существующих лент RSS</param>
        public FeedVisibilityWindow(List<RssFeedConfig> rssFeeds)
        {
            InitializeComponent();

            _feedVisibilityItems = rssFeeds.Select(feed => new RssFeedConfig
            {
                SiteName = feed.SiteName,
                IsVisible = feed.IsVisible
            }).ToList();

            FeedVisibilityListBox.ItemsSource = _feedVisibilityItems;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Получение обновленного статуса лент.
        /// </summary>
        /// <returns>Список объектов с обновленными статусами.</returns>
        public List<RssFeedConfig> GetUpdatedVisibility()
        {
            return _feedVisibilityItems;
        }
    }
}
