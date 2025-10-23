using System.Text.Json.Serialization;

public class ReleaseResponse
{
    [JsonPropertyName("html_url")]
    public string Url { get; }

    [JsonPropertyName("tag_name")]
    public string TagName { get; }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonConstructor]
    public ReleaseResponse(string url, string tagName, string name)
    {
        Url = url;
        TagName = tagName;
        Name = name;
    }
}