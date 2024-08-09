/// <summary>
/// Конфигурация приложения и его компонентов.
/// </summary>
public class AppConfig
{
    public List<RssFeedConfig> RssFeeds { get; set; } = new List<RssFeedConfig>();
    public int UpdateIntervalSeconds { get; set; } = 100;
    public ProxySettings ProxySettings { get; set; } = new ProxySettings();
}

/// <summary>
/// Конфигурация RSS ленты.
/// </summary>
public class RssFeedConfig
{
    public string SiteName { get; set; }
    public string RssUrl { get; set; }
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// Конфигурация прокси.
/// </summary>
public class ProxySettings
{
    public string Address { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
