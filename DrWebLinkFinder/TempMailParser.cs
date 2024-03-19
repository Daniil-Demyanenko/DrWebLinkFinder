using System.Text.RegularExpressions;
using SmorcIRL.TempMail;
using SmorcIRL.TempMail.Models;

namespace TempMail;

public class TempMailParser : IDisposable
{
    public readonly string Email;
    private readonly MailClient _client;

    private TempMailParser(string email, MailClient client)
    {
        Email = email;
        _client = client;
    }

    public static async Task<TempMailParser> Create()
    {
        var client = new MailClient();
        var domain = await client.GetFirstAvailableDomainName();
        string email = GenerateAddress(domain);
        await client.Register(email, "12345678");

        return new TempMailParser(email, client);
    }

    private static string GenerateAddress(string domain)
        => String.Join("", Guid.NewGuid().ToString().Take(12)) + "@" + domain;

    public async Task<MessageSource> WaitForEmail(CancellationToken cancellationToken = new CancellationToken())
    {
        while (true)
        {
            if (cancellationToken.CanBeCanceled) return null;

            var messages = await _client.GetAllMessages();

            foreach (var message in messages)
            {
                if (!message.Seen && message.From.Address.Contains("drweb.com"))
                {
                    message.Seen = true;
                    return await _client.GetMessageSource(message.Id);
                }
            }
            
            await Task.Delay(1000, cancellationToken);
        }
    }

    public string ParseLinkFromMessage(MessageSource message)
    {
        var data = MailContentDecoder.Decode(message.Data);
        
        Match match = Regex.Match(data, @"\=""(https://free\.drweb\.ru/download\+cureit\+free/\?ph=[\d\w]+)""", RegexOptions.Multiline | RegexOptions.CultureInvariant);
        string link = match.Groups[1].Value;
        
        return link;
    }

    public void Dispose()
    {
        _client.DeleteAccount();
    }
}