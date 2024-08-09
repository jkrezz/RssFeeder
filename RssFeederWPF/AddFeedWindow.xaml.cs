using System.Windows;

namespace RssFeederWPF
{
    /// <summary>
    /// Окно для добавления новой конфигурации RSS ленты.
    /// </summary>
    public partial class AddFeedWindow : Window
    {
        /// <summary>
        /// Получает или устанавливает заголовок записи.
        /// </summary>
        public string SiteName { get; private set; }
        /// <summary>
        /// Получает или устанавливает RSS ссылку записи.
        /// </summary>
        public string RssUrl { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр окна для добавления RSS ленты.
        /// </summary>
        public AddFeedWindow()
        {
            InitializeComponent();
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
