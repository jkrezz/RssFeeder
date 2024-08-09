using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using HtmlAgilityPack;

/// <summary>
/// Сервис для получения и обработки RSS и Atom лент.
/// </summary>
public class RssService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса/>.
    /// </summary>
    public RssService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Получает элементы RSS или Atom ленты из указанного URL.
    /// </summary>
    /// <param name="url">URL RSS или Atom ленты.</param>
    /// <returns>Список элементов ленты типа RssItem.</returns>
    public async Task<List<RssItem>> GetRssFeedAsync(string url)
    {
        var items = new List<RssItem>();

        try
        {
            var response = await _httpClient.GetStringAsync(url);
            XDocument rssFeed = XDocument.Parse(response);

            if (rssFeed.Root.Name.LocalName == "feed") // Atom format
            {
                var feedItems = rssFeed.Descendants().Where(d => d.Name.LocalName == "entry");
                foreach (var item in feedItems)
                {
                    var descriptionHtml = item.Descendants().FirstOrDefault(d => d.Name.LocalName == "summary")?.Value;
                    var cleanDescription = RemoveHtmlTags(descriptionHtml);

                    items.Add(new RssItem
                    {
                        Title = item.Descendants().FirstOrDefault(d => d.Name.LocalName == "title")?.Value,
                        Description = cleanDescription,
                        PublishDate = DateTime.Parse(item.Descendants().FirstOrDefault(d => d.Name.LocalName == "updated")?.Value),
                        Link = item.Descendants().FirstOrDefault(d => d.Name.LocalName == "link")?.Attribute("href")?.Value
                    });
                }
            }
            else // RSS format
            {
                var feedItems = rssFeed.Descendants("item");
                foreach (var item in feedItems)
                {
                    var descriptionHtml = item.Element("description")?.Value;
                    var cleanDescription = RemoveHtmlTags(descriptionHtml);

                    items.Add(new RssItem
                    {
                        Title = item.Element("title")?.Value,
                        Description = cleanDescription,
                        PublishDate = DateTime.Parse(item.Element("pubDate")?.Value),
                        Link = item.Element("link")?.Value
                    });
                }
            }
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Ошибка при получении RSS ленты: {ex.Message}", ex);
        }

        return items;
    }

    private string RemoveHtmlTags(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc.DocumentNode.InnerText;
    }
}
