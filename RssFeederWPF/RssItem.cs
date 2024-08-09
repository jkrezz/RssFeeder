/// <summary>
/// Элемент RSS ленты.
/// </summary>
public class RssItem
{
    /// <summary>
    /// Получает или устанавливает заголовок записи.
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// Получает или устанавливает описание.
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// Получает или устанавливает дату статьи.
    /// </summary>
    public DateTime PublishDate { get; set; }
    /// <summary>
    /// Получает или устанавливает RSS ссылку.
    /// </summary>
    public string Link { get; set; }
}
