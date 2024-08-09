using System;
using System.Net;
using System.Windows;

namespace RssFeederWPF
{
    /// <summary>
    /// Окно для редактирования конфигурации существующей RSS ленты.
    /// </summary>
    public partial class EditFeedWindow : Window
    {
        public string SiteName { get; private set; }
        public string RssUrl { get; private set; }

        /// <summary>
        /// Создает окно для редактирования конфигурации RSS ленты.
        /// </summary>
        /// <param name="feed">Объект конфигурации RSS ленты, предназначенный для редактирования.</param>
        public EditFeedWindow(RssFeedConfig feed)
        {
            InitializeComponent();
            SiteNameTextBox.Text = feed.SiteName;
            RssUrlTextBox.Text = feed.RssUrl;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SiteName = SiteNameTextBox.Text;
            RssUrl = RssUrlTextBox.Text;

            if (!IsValidUrl(RssUrl))
            {
                MessageBox.Show("Введите действительный URL", "Неверный URL", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
