

using HtmlAgilityPack;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Front.Entities;
using NuGet.Protocol;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;

namespace Front.Services
{
    public class WikispeediaService
    {
        private readonly HttpClient _httpClient;
        public System.Timers.Timer timer;
          public event EventHandler OnGameOver;

        public class Game
        {
            public bool started = false;
            public TimeSpan timeElapsed = TimeSpan.Zero;
            public string start { get; set; }
            public string startextract { get; set; }
            public string end { get; set; }
            public string endextract { get; set; }

            public List<string> path { get; set; } = new List<string>();

        }

        public Game game;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private ProtectedLocalStorage _localStorage;

        public WikispeediaService(HttpClient httpClient, ProtectedLocalStorage localStorage, AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new System.Uri("http://localhost:5052");
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;

            game = new Game();
        }

        public async Task SearchStart()
        {
            //string apiUrl = $"https://en.wikipedia.org/w/api.php?action=query&generator=random&grnnamespace=0&exlimit=1&explaintext=1&exsentences=5&formatversion=2&prop=extracts&format=json";
            string pageTitle = "Wikipedia";
            string apiUrl = $"https://en.wikipedia.org/w/api.php?action=query&titles={Uri.EscapeDataString(pageTitle)}&exlimit=1&explaintext=1&exsentences=5&formatversion=2&prop=extracts&format=json";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string apiRes = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<WikipediaApiResponse>(apiRes);

                if (result?.Query?.Pages != null)
                {
                    var page = result.Query.Pages.First();

                    game.startextract = page.Extract;
                    game.start = page.Title;
                }
                else
                {
                    Console.WriteLine("No results found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task SearchEnd()
        {
            //string apiUrl = $"https://en.wikipedia.org/w/api.php?action=query&generator=random&grnnamespace=0&exlimit=1&explaintext=1&exsentences=5&formatversion=2&prop=extracts&format=json";
            string pageTitle = "Volunteering";  // United_States
            string apiUrl = $"https://en.wikipedia.org/w/api.php?action=query&titles={Uri.EscapeDataString(pageTitle)}&exlimit=1&explaintext=1&exsentences=5&formatversion=2&prop=extracts&format=json";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string apiRes = await response.Content.ReadAsStringAsync();
                
                var result = JsonConvert.DeserializeObject<WikipediaApiResponse>(apiRes);

                if (result?.Query?.Pages != null)
                {
                    var page = result.Query.Pages.First();

                    game.endextract = page.Extract;
                    game.end = page.Title;
                }
                else
                {
                    Console.WriteLine("No results found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public class WikipediaApiResponse
        {
            public bool BatchComplete { get; set; }
            public QueryResult Query { get; set; } = new QueryResult();
        }

        public class QueryResult
        {
            public List<Page> Pages { get; set; } = new List<Page>();
        }

        public class Page
        {
            public int PageId { get; set; }
            public int Namespace { get; set; }
            public string Title { get; set; }
            public string Extract { get; set; }
        }

        static string RemoveAfterExternalLinks(string input)
        {
            int externalLinksIndex = input.IndexOf("<h2><span class=\"mw-headline\" id=\"External_links\">External links</span></h2>", StringComparison.OrdinalIgnoreCase);

            if (externalLinksIndex != -1)
            {
                return input.Substring(0, externalLinksIndex);
            }

            return input;
        }



        static string RemoveLinksWithoutWiki(string input)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(input);
            string pattern = @"^(?!.*wikipedia\.org/wiki).*\/wiki\/[^\/]*$";

            var anchorNodes = htmlDocument.DocumentNode.SelectNodes("//a");
            if (anchorNodes != null)
            {
                foreach (var anchorNode in anchorNodes)
                {
                    var hrefAttribute = anchorNode.Attributes["href"];
                    if (hrefAttribute != null && !Regex.IsMatch(hrefAttribute.Value, pattern))
                    {
                        anchorNode.Remove();
                    }
                }
            }

            // Get the updated HTML
            string updatedHtml = htmlDocument.DocumentNode.OuterHtml;

            return updatedHtml;
        }

        static string RemoveTablesByClass(string input, string targetClass)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(input);
            var tableNodes = htmlDocument.DocumentNode.SelectNodes($"//table[contains(@class, '{targetClass}')]");
            if (tableNodes != null)
            {
                foreach (var tableNode in tableNodes)
                {
                    tableNode.Remove();
                }
            }
            string updatedHtml = htmlDocument.DocumentNode.OuterHtml;
            return updatedHtml;
        }
        static string RemoveDivsByClass(string input, string targetClass)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(input);

            var divNodes = htmlDocument.DocumentNode.SelectNodes($"//div[contains(@class, '{targetClass}')]");

            if (divNodes != null)
            {
                foreach (var divNode in divNodes)
                {
                    divNode.Remove();
                }
            }

            string updatedHtml = htmlDocument.DocumentNode.OuterHtml;
            return updatedHtml;
        }

        static string AddPointerEventsNone(string input)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(input);

            var imgNodes = htmlDocument.DocumentNode.SelectNodes("//img");

            if (imgNodes != null)
            {
                foreach (var imgNode in imgNodes)
                {
                    // Add or update the style attribute to include pointer-events: none
                    var styleAttribute = imgNode.GetAttributeValue("style", "");
                    styleAttribute += "; pointer-events: none;";
                    imgNode.SetAttributeValue("style", styleAttribute.TrimStart(';').Trim());
                }
            }

            string updatedHtml = htmlDocument.DocumentNode.OuterHtml;
            return updatedHtml;
        }
        public class ParseResult
        {
            public ParseInfo Parse { get; set; } = new ParseInfo();
        }

        public class ParseInfo
        {
            public string Title { get; set; }
            public int PageId { get; set; }
            public string Text { get; set; }
        }


        public async Task<string> FetchWikiArticle(string apiUrl)
        {

            string htmlContent = string.Empty;

            try
            {
                
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string ApiRes = await response.Content.ReadAsStringAsync();

                var parseResult = JsonConvert.DeserializeObject<ParseResult>(ApiRes);

                htmlContent = parseResult.Parse.Text;
                string newTitle = parseResult.Parse.Title;

                if (!string.IsNullOrEmpty(newTitle) &&
                    (game.path.Count == 0 || !string.Equals(newTitle, game.path.Last(), StringComparison.OrdinalIgnoreCase)))
                {
                    game.path.Add(newTitle);
                }

                if (game.path.Last() == game.end)
                {
                    StopGame();
                }


                //ApiRes = Regex.Replace(ApiRes, "<img[^>]*?>", "");
                htmlContent = htmlContent.Replace("\\\"", "'");
                htmlContent = htmlContent.Replace("'\"", "\"");
                htmlContent = RemoveLinksWithoutWiki(htmlContent);
                htmlContent = AddPointerEventsNone(htmlContent);
                htmlContent = htmlContent.Replace("<span class=\"mw-editsection\"><span class=\"mw-editsection-bracket\">[</span>", "");
                htmlContent = htmlContent.Replace("<span class=\"mw-editsection-bracket\">]</span>", "");
                htmlContent = htmlContent.Replace("<span class=\"mw-headline\" id=\"References\">References</span>", "");
                htmlContent = htmlContent.Replace("<span class=\"mw-headline\" id=\"Sources\">Sources</span>", "");
                htmlContent = htmlContent.Replace("<span class=\"mw-headline\" id=\"Further_reading\">Further reading</span>", "");
                htmlContent = htmlContent.Replace("<span class=\"mw-headline\" id=\"Citations\">Citations</span>", "");
                htmlContent = htmlContent.Replace("<span class=\"mw-headline\" id=\"Footnotes\">Footnotes</span>", "");
                htmlContent = htmlContent.Replace("<span class=\"mw-headline\" id=\"Notes\">Notes</span>", "");
                htmlContent = RemoveTablesByClass(htmlContent, "box-Multiple_issues plainlinks metadata ambox ambox-content ambox-multiple_issues compact-ambox");
                htmlContent = RemoveTablesByClass(htmlContent, "box-BLP_sources plainlinks metadata ambox ambox-content ambox-BLP_sources");

                htmlContent = RemoveDivsByClass(htmlContent, "reflist reflist-columns references-column-width");
                htmlContent = RemoveDivsByClass(htmlContent, "refbegin refbegin-hanging-indents refbegin-columns references-column-width");
                htmlContent = RemoveDivsByClass(htmlContent, "mw-references-wrap mw-references-columns");
                htmlContent = RemoveDivsByClass(htmlContent, "mw-references-wrap");
                htmlContent = RemoveDivsByClass(htmlContent, "navbox authority-control");





                htmlContent = Regex.Replace(htmlContent, "External Links", "");
                htmlContent = htmlContent.Replace("\\t", "");
                htmlContent = RemoveAfterExternalLinks(htmlContent);
                 
                
                // Use MarkupString to display raw HTML content

            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., display an error message)
                Console.WriteLine($"Error: {ex.Message}");
            }
            return htmlContent;
        }

        public void StartGame()
        {
            game.started = true;
            game.timeElapsed = TimeSpan.Zero;
            game.path.Clear();
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
            timer = new System.Timers.Timer(1000);
            timer.Elapsed -= Timer_Elapsed;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        public void StopGame()
        {
            game.started = false;
            timer.Stop();
            timer.Dispose();
            OnGameOver?.Invoke(this, EventArgs.Empty);

            sendGameToDB();

        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            game.timeElapsed = game.timeElapsed.Add(TimeSpan.FromSeconds(1));
        }

        public async Task sendGameToDB()
        {
            try
            {
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity.IsAuthenticated)
                {
                    var userId = user.FindFirst(ClaimTypes.NameIdentifier).Value;


                    if (userId != null)
                    {
                        using (var httpClientHistory = new HttpClient())
                        {
                            var jwtToken = await _localStorage.GetAsync<string>("jwt");

                            Console.WriteLine($"JWT TOKEN : {jwtToken.Value}");
                            Console.WriteLine($"sessionStorage: {_localStorage.ToJson()}");

                            httpClientHistory.BaseAddress = new Uri("http://127.0.0.1:5000/");
                            
                            Entry entry = new Entry()
                            {
                                UserId = userId, 
                                Start =  game.start,
                                End = game.end,
                                Path = game.path,
                                Timestamp = game.timeElapsed
                            };
                            Console.WriteLine($"Entry : {entry.ToJson()}");
                            httpClientHistory.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken.Value);

                            var response =await httpClientHistory.PostAsJsonAsync("/api/History/add", entry);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }

        }

    }
}
