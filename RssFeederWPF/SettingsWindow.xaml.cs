using System;
using System.Windows;
using System.Text.RegularExpressions;

namespace RssFeederWPF
{
    /// <summary>
    /// Окно настроек приложения.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Получает или устанавливает конфигурацию приложения.
        /// </summary>
        public AppConfig Config { get; private set; }

        /// <summary>
        /// Событие для уведомления изменения интервала.
        /// </summary>
        public event Action<int> UpdateIntervalChanged;

        private static Regex ProxyAddressRegex = new Regex(
            @"^(http(s)?://)?(\d{1,3}\.){3}\d{1,3}(:\d{1,5})?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Инициализирует новый экземпляр окна настроек.
        /// </summary>
        /// <param name="config">Конфигурация приложения для настройки.</param>
        public SettingsWindow(AppConfig config)
        {
            InitializeComponent();
            Config = config;

            EnableProxyCheckBox.IsChecked = !string.IsNullOrEmpty(Config.ProxySettings?.Address);
            ProxySettingsPanel.Visibility = EnableProxyCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            UseProxyAuthCheckBox.IsChecked = !string.IsNullOrEmpty(Config.ProxySettings?.Username);
            ProxyAuthPanel.Visibility = UseProxyAuthCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            ProxyAddressTextBox.Text = Config.ProxySettings?.Address;
            ProxyUsernameTextBox.Text = Config.ProxySettings?.Username;
            ProxyPasswordBox.Password = Config.ProxySettings?.Password;
            if (Config.UpdateIntervalSeconds < 5 || Config.UpdateIntervalSeconds > 300)
            {
                Config.UpdateIntervalSeconds = 30;
            }

            UpdateIntervalTextBox.Text = Config.UpdateIntervalSeconds.ToString();
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (EnableProxyCheckBox.IsChecked == true)
            {
                if (IsValidProxyAddress(ProxyAddressTextBox.Text))
                {
                    // Создание объекта ProxySettings, если флажок включен
                    Config.ProxySettings = new ProxySettings
                    {
                        Address = ProxyAddressTextBox.Text,
                        Username = UseProxyAuthCheckBox.IsChecked == true ? ProxyUsernameTextBox.Text : null,
                        Password = UseProxyAuthCheckBox.IsChecked == true ? ProxyPasswordBox.Password : null
                    };
                }
                else
                {
                    MessageBox.Show("Неверный формат адреса прокси.", "Ошибка формата", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                Config.ProxySettings = null;
            }

            if (int.TryParse(UpdateIntervalTextBox.Text, out int interval))
            {
                // Проверка, что значение интервала в допустимом диапазоне
                if (interval >= 7 && interval <= 300)
                {
                    // Обновление конфигурации
                    Config.UpdateIntervalSeconds = interval;
                    ConfigLoader.SaveConfig("config.json", Config);

                    UpdateIntervalChanged?.Invoke(interval);

                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Интервал обновления должен быть от 7 до 300 секунд.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите корректное число интервала обновления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidProxyAddress(string address)
        {
            return ProxyAddressRegex.IsMatch(address);
        }
        private void EnableProxyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ProxySettingsPanel.Visibility = Visibility.Visible;
        }

        private void EnableProxyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ProxySettingsPanel.Visibility = Visibility.Collapsed;
        }

        private void UseProxyAuthCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ProxyAuthPanel.Visibility = Visibility.Visible;
        }

        private void UseProxyAuthCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ProxyAuthPanel.Visibility = Visibility.Collapsed;
        }
    }
}
