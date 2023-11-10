using System;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace Challenge1
{
    internal class Program
    {
        private const string Url = "https://exs-htf-2023.azurewebsites.net/api/challenges/find-the-jeep";
        private const string TeamId = "5cd1d9fe-e26c-4c74-a5c9-9429af9320e2";

        public static async Task Main(string[] args)
        {
            await RunChallenge();
        }

        private static async Task RunChallenge()
        {
            using (var httpClient = new HttpClient())
            {
                ConfigureHttpClient(httpClient);

                var responseContent = await MakeGetApiRequest(Url + "?isTest=false", httpClient);
                var coords = JsonConvert.DeserializeObject<Dictionary<string, Coords>>(responseContent);

                var you = coords["you"];
                var volcano = coords["volcano"];
                var mountain = coords["mountain"];

                var result = Coords.CalcDiff(you, mountain);
                var length = new Coords(result.x, result.y);

                var unknownCoords = Coords.GetUnknownCoords(volcano, length);

                var answer = new { answer = unknownCoords };

                var response = await MakePostApiRequest(Url, answer, httpClient);
            }
        }

        private static void ConfigureHttpClient(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Team", TeamId);
        }

        private static async Task<string> MakeGetApiRequest(string url, HttpClient httpClient)
        {
            var response = await httpClient.GetStringAsync(url);
            return response;
        }

        private static async Task<string> MakePostApiRequest(string url, object answer, HttpClient httpClient)
        {
            var response = await httpClient.PostAsJsonAsync(url, answer);
            return await response.Content.ReadAsStringAsync();
        }

        public class Coords
        {
            public double x { get; set; }
            public double y { get; set; }

            public Coords() { }

            public Coords(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

            public static Coords CalcDiff(Coords you, Coords other)
            {
                return new Coords
                {
                    x = other.x - you.x,
                    y = other.y - you.y
                };
            }

            public static Coords GetUnknownCoords(Coords otherOther, Coords length)
            {
                return new Coords
                {
                    x = Math.Round(otherOther.x + length.x, 2),
                    y = Math.Round(otherOther.y + length.y, 2)
                };
            }
        }
    }
}