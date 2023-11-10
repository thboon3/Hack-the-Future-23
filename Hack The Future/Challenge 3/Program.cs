using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace Challenge3
{
    class Point
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    internal class Program
    {
        static readonly HttpClient client = new();

        static async Task Main()
        {
            ConfigureHttpClient();

            var response = await client.GetAsync("/api/challenges/pathfinder?isTest=false");

            dynamic respJson = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

            var maze = ParseMaze(respJson.pathfinderData);
            var startpoint = JsonConvert.DeserializeObject<Point>(respJson.startpoint.ToString());
            var endpoint = JsonConvert.DeserializeObject<Point>(respJson.endpoint.ToString());

            var solution = SolveMaze(maze, startpoint, endpoint);

            Console.WriteLine(JsonConvert.SerializeObject(new { answer = solution }));

            var postResponse = await client.PostAsJsonAsync("/api/challenges/pathfinder", new { answer = solution });
            postResponse.EnsureSuccessStatusCode();

            string response2 = await postResponse.Content.ReadAsStringAsync();
        }

        static void ConfigureHttpClient()
        {
            client.BaseAddress = new Uri("https://exs-htf-2023.azurewebsites.net/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "Team 5cd1d9fe-e26c-4c74-a5c9-9429af9320e2");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        static List<List<string>> ParseMaze(dynamic mazeRows)
        {
            var parsedMaze = new List<List<string>>();

            foreach (var row in mazeRows)
            {
                List<string> typedRow = new List<string>();

                foreach (var cell in row)
                {
                    typedRow.Add(cell.ToString());
                }

                parsedMaze.Add(typedRow);
            }

            return parsedMaze;
        }

        static string[] SolveMaze(List<List<string>> maze, Point startpoint, Point endpoint)
        {
            var path = new List<string>();
            if (DFS(maze, startpoint, endpoint, path))
            {
                return path.ToArray();
            }

            return new[] { "No solution found" };
        }

        static bool DFS(List<List<string>> maze, Point current, Point target, List<string> path)
        {
            if (current.x == target.x && current.y == target.y)
            {
                return true;
            }

            var directions = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };

            foreach (var (dx, dy) in directions)
            {
                var newX = current.x + dx;
                var newY = current.y + dy;

                if (IsValidMove(maze, newX, newY))
                {
                    maze[newY][newX] = "V";

                    path.Add(GetMoveDirection(dx, dy));

                    if (DFS(maze, new Point { x = newX, y = newY }, target, path))
                    {
                        return true;
                    }

                    path.RemoveAt(path.Count - 1);
                }
            }

            return false;
        }

        static bool IsValidMove(List<List<string>> maze, int x, int y)
        {
            var hasRows = maze.Count > 0;
            var withinXBounds = hasRows && x >= 0 && x < maze[0].Count;
            var withinYBounds = hasRows && y >= 0 && y < maze.Count;
            var validCell = withinXBounds && withinYBounds && (maze[y][x] == "0" || maze[y][x] == "E");

            return validCell;
        }

        static string GetMoveDirection(int dx, int dy)
        {
            return (dx, dy) switch
            {
                (0, -1) => "U",
                (0, 1) => "D",
                (-1, 0) => "L",
                (1, 0) => "R",
                _ => throw new InvalidOperationException("Invalid move direction"),
            };
        }
    }
}
