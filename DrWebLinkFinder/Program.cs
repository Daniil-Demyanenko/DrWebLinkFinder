using TempMail;

var client = new HttpClient();

using var parser = await TempMailParser.Create();
await SendDownloadLink(parser.Email);
var message = await parser.WaitForEmail();
var link = parser.ParseLinkFromMessage(message);

Console.WriteLine($"Ваша ссылка на скачивание: {link}");


async Task SendDownloadLink(string email)
{
    using var request = new HttpRequestMessage(HttpMethod.Post, "https://free.drweb.ru/download+cureit+free/");
    request.Headers.Add("User-Agent", DrWebHacks.GetUserAgent());

    var content = new Dictionary<string, string>()
    {
        { "name", DrWebHacks.GetName() },
        { "surname", DrWebHacks.GetName() },
        { "email", email },
        { "i_agree", "1" }
    };
    request.Content = new FormUrlEncodedContent(content);

    using var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();
    Console.WriteLine(await response.Content.ReadAsStringAsync());
}