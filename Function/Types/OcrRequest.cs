namespace Function.Types;

public class OcrRequest(string mimeType, string[] languageCodes, string content)
{
    public string MimeType { get; set; } = mimeType;

    public string[] LanguageCodes { get; set; } = languageCodes;

    public string Content { get; set; } = content;
}