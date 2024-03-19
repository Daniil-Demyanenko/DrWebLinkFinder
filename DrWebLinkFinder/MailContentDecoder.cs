using System.Text.RegularExpressions;

namespace TempMail;

public static class MailContentDecoder
{
    public static string Decode(string text)
    {
        var boundary = FindBoundaryValue(text);
        var encodedContent = GetContent(text, boundary);
        return Base64Decode(encodedContent);
    }

    private static string FindBoundaryValue(string text)
    {
        Match match = Regex.Match(text, @"boundary=""([^""]+)""");
        string boundaryValue = match.Groups[1].Value;
        return "\n--" + boundaryValue;
    }

    private static string GetContent(string text, string boundary)
    {
        var startIndex = text.IndexOf(boundary, StringComparison.Ordinal);
        var endIndex = text.LastIndexOf(boundary, StringComparison.Ordinal);
        
        string extractedMessage = text.Substring(startIndex, endIndex - startIndex).Trim();
        
        Match match = Regex.Match(extractedMessage, @"(Date: [\w\d\.,\s:]+\+[\d]+)", RegexOptions.Multiline | RegexOptions.CultureInvariant);
        string dateString = match.Groups[1].Value;

        startIndex = extractedMessage.IndexOf(dateString, StringComparison.Ordinal);
        var extractedText = extractedMessage.Substring(startIndex + dateString.Length ).Trim();
        
        return extractedText;
    }
    
    public static string Base64Decode(string base64Data) 
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64Data);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
}