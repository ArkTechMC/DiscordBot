using System.Net;
using System.Net.Http.Headers;

namespace DiscordBot.util;

internal abstract class Network {
    public static string HttpGet(string url) {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response = client.GetAsync(url).Result;
        if (response.StatusCode != HttpStatusCode.OK) throw new NetworkException(response.StatusCode);
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().Result;
    }
}

public class NetworkException : Exception {
    public readonly HttpStatusCode Code;
    public NetworkException(HttpStatusCode code) {
        Code = code;
    }
}
