using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscordBot.util {
    internal class MojangApi {
        public static MojangApiResponse GetMojangInfo(string name) {
            try {
                string res = Network.HttpGet($"https://api.mojang.com/users/profiles/minecraft/{name}");
                return JsonSerializer.Deserialize<MojangApiResponse>(res) ?? new MojangApiResponse();
            }
            catch (NetworkException ex) {
                return ex.Code == HttpStatusCode.NotFound ? new MojangApiResponse("Player Not Found", "") : new MojangApiResponse($"Error On Request: {ex.Code}", "");
            }
        }

        internal class MojangApiResponse {
            [JsonInclude]
            [JsonPropertyName("name")]
            public string Name = "";
            [JsonInclude]
            [JsonPropertyName("id")]
            public string Id = "";

            public MojangApiResponse() { }
            public MojangApiResponse(string name, string id) {
                Name = name;
                Id = id;
            }
        }
    }
}
