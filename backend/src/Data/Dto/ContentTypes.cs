namespace backend.Data.Dto;
public static class ContentTypes
{
    public const string ApplicationJavascript = "application/javascript";
    public const string ApplicationOctet = "application/octet-stream";
    public const string ApplicationXHtml = "application/xhtml+xml";
    public const string ApplicationJson = "application/json";
    public const string ApplicationXml = "application/xml";
    public const string TextCsv = "text/csv";
    public const string TextHtml = "text/html";
    public const string TextJavascript = "text/javascript";
    public const string TextPlain = "text/plain";
    public const string TextXml = "text/xml";
    public static readonly string[] ToArray = [ 
        ApplicationJavascript,
        ApplicationOctet,
        ApplicationXHtml,
        ApplicationJson,
        ApplicationXml,
        TextCsv,
        TextHtml,
        TextPlain,
        TextJavascript,
        TextXml
    ];

}