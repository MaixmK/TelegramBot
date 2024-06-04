using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using API_Football.Models;
using GetPlayerModels;
using GetTopScorers;
using LiveMatches;
using PostPlayer;

namespace Football_Api_Bot
{
    public class Telegram_Bot
    {
        TelegramBotClient botClient = new TelegramBotClient("Your_Telegram_Token");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

        private int matchIndex = 0;
        private List<Result> matches = new List<Result>();

        public static async Task Main(string[] args)
        {
            Telegram_Bot bot = new Telegram_Bot();
            await bot.Start();
            Console.ReadKey();
        }

        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerErrorAsync, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Bot {botMe.Username} in progress");
            Console.ReadKey();
        }

        private Task HandlerErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Error in telegram bot API:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandlerMessageStartAsync(botClient, update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery.Data == "next")
            {
                await HandleNextMatches(update.CallbackQuery.Message);
            }
        }

        private async Task HandlerMessageStartAsync(ITelegramBotClient botClient, Message message)
        {
            try
            {
                if (message.Text == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Choose command from list of commands /keyboard");
                    return;
                }
                if (message.Text == "/keyboard")
                {
                    ReplyKeyboardMarkup replyKeyboardMarkup = new(
                        new[]
                        {
                            new KeyboardButton[] { "⚽ Player Info", "🏆 Team Info", "➕ Add Player" },
                            new KeyboardButton[] { "✏️ Update Player", "❌ Delete Player", "🗑️ Clear Database" },
                            new KeyboardButton[] { "📊 Standings", "🥇 Top Scorers", "🔍 Compare Players" },
                            new KeyboardButton[] { "📋 List Players", "⚽ Live Matches" }
                        })
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Choose a menu item:", replyMarkup: replyKeyboardMarkup);
                    return;
                }
                if (message.Text == "⚽ Player Info")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Write player club and name in format\n?clubname_playername");
                    return;
                }
                if (message.Text == "🏆 Team Info")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Write club in format\n!clubname");
                    return;
                }
                if (message.Text == "➕ Add Player")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Provide player's club and name in format\n+clubname_playername");
                    return;
                }
                if (message.Text == "✏️ Update Player")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Provide current player's lastname, new player's club and new player's lastname in format\n%currentLastname_newPlayerClub_newPlayerName");
                    return;
                }
                if (message.Text == "❌ Delete Player")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Provide player's lastname to delete in format\n-lastname");
                    return;
                }
                if (message.Text == "🗑️ Clear Database")
                {
                    await HandleClearDatabaseRequest(message);
                    return;
                }
                if (message.Text == "📊 Standings")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Provide club name to get standings in format\n#clubname");
                    return;
                }
                if (message.Text == "🥇 Top Scorers")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Provide club name to get top scorers in format\n$clubname");
                    return;
                }
                if (message.Text == "🔍 Compare Players")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Provide club name and player name to compare in format\n&clubname_playername");
                    return;
                }
                if (message.Text == "📋 List Players")
                {
                    await HandleListPlayersRequest(message);
                    return;
                }
                if (message.Text == "⚽ Live Matches")
                {
                    await HandleLiveMatchesRequest(message);
                    return;
                }

                if (message.Text.StartsWith("?"))
                {
                    await HandlePlayerInfoRequest(message);
                }
                else if (message.Text.StartsWith("!"))
                {
                    await HandleTeamInfoRequest(message);
                }
                else if (message.Text.StartsWith("+"))
                {
                    await HandleAddPlayerRequest(message);
                }
                else if (message.Text.StartsWith("-"))
                {
                    await HandleDeletePlayerRequest(message);
                }
                else if (message.Text.StartsWith("%"))
                {
                    await HandleUpdatePlayerRequest(message);
                }
                else if (message.Text.StartsWith("#"))
                {
                    await HandleStandingsRequest(message);
                }
                else if (message.Text.StartsWith("$"))
                {
                    await HandleTopScorersRequest(message);
                }
                else if (message.Text.StartsWith("&"))
                {
                    await HandlePlayerComparisonRequest(message);
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"An error occurred: {ex.Message}");
            }
        }

        private async Task HandlePlayerInfoRequest(Message message)
        {
            var input = message.Text.Substring(1).Split('_');
            if (input.Length < 2)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Invalid format. Use ?clubname_playername");
                return;
            }

            string clubName = input[0];
            string playerName = input[1];

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7088/PlayerInfo?ClubName={clubName}&PlayerName={playerName}";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var playerData = JsonConvert.DeserializeObject<GetPlayer>(jsonResponse);

                        if (playerData.Response.Count == 0)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Player not found.");
                        }
                        else
                        {
                            var player = playerData.Response[0].Player;
                            var statistics = playerData.Response[0].Statistics[0];

                            string responseText = $"<b>Player:</b> {player.Firstname} {player.Lastname}\n" +
                                                  $"<b>Age:</b> {player.Age}\n" +
                                                  $"<b>Nationality:</b> {player.Nationality}\n" +
                                                  $"<b>Height:</b> {player.Height}\n" +
                                                  $"<b>Weight:</b> {player.Weight}\n" +
                                                  $"<b>Team:</b> {statistics.Team.Name}\n" +
                                                  $"<b>League:</b> {statistics.League.Name}\n" +
                                                  $"<b>Games:</b> {statistics.Games.Appearences}\n" +
                                                  $"<b>Goals:</b> {statistics.Goals.Total}\n" +
                                                  $"<b>Assists:</b> {statistics.Goals.Assists}\n" +
                                                  $"<b>Yellow Cards:</b> {statistics.Cards.Yellow}\n" +
                                                  $"<b>Red Cards:</b> {statistics.Cards.Red}\n" +
                                                  $"<a href=\"{player.Photo}\">Photo</a>";

                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: responseText, parseMode: ParseMode.Html);
                        }
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Failed to retrieve player data. Status Code: {response.StatusCode}, Error: {errorResponse}");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"An error occurred: {ex.Message}");
                }
            }
        }

        private async Task HandleTeamInfoRequest(Message message)
        {
            var clubName = message.Text.Substring(1);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7088/TeamStat?teamName={clubName}";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var teamData = JsonConvert.DeserializeObject<TeamStatisticsResponse>(jsonResponse);

                        if (teamData.Response == null)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Team not found.");
                        }
                        else
                        {
                            var biggestWins = teamData.Response.Biggest?.Wins;
                            var biggestGoals = teamData.Response.Biggest?.Goals;
                            var fixtures = teamData.Response.Fixtures;
                            var goals = teamData.Response.Goals;
                            var team = teamData.Response.Team;
                            var league = teamData.Response.League;

                            string responseText = $"<b>Team:</b> {team.Name}\n" +
                                                  $"<b>League:</b> {league.Name} ({league.Country}) - Season {league.Season}-2024\n\n" +
                                                  $"<b>--- Fixtures ---</b>\n" +
                                                  $"<b>Played:</b> Total: {fixtures.Played?.Total ?? 0}, Home: {fixtures.Played?.Home ?? 0}, Away: {fixtures.Played?.Away ?? 0}\n" +
                                                  $"<b>Wins:</b> Total: {fixtures.Wins?.Total ?? 0}, Home: {fixtures.Wins?.Home ?? 0}, Away: {fixtures.Wins?.Away ?? 0}\n" +
                                                  $"<b>Draws:</b> Total: {fixtures.Draws?.Total ?? 0}, Home: {fixtures.Draws?.Home ?? 0}, Away: {fixtures.Draws?.Away ?? 0}\n" +
                                                  $"<b>Loses:</b> Total: {fixtures.Loses?.Total ?? 0}, Home: {fixtures.Loses?.Home ?? 0}, Away: {fixtures.Loses?.Away ?? 0}\n\n";

                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: responseText, parseMode: ParseMode.Html);
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Failed to retrieve team data.");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"An error occurred: {ex.Message}");
                }
            }
        }


        private async Task HandleAddPlayerRequest(Message message)
        {
            var input = message.Text.Substring(1).Split('_');
            if (input.Length < 2)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Invalid format. Use +clubname_playername");
                return;
            }

            string clubName = input[0];
            string playerName = input[1];

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7088/Player?FootballClub={clubName}&PlayerName={playerName}";
                    HttpResponseMessage response = await client.PostAsync(url, null);

                    if (response.IsSuccessStatusCode)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Player added successfully.");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Failed to add player.");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"An error occurred: {ex.Message}");
                }
            }
        }

        private async Task HandleUpdatePlayerRequest(Message message)
        {
            var input = message.Text.Substring(1).Split('_');
            if (input.Length < 3)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Invalid format. Use ?currentLastname_newPlayerClub_newPlayerName");
                return;
            }

            string currentLastname = input[0];
            string newPlayerClub = input[1];
            string newPlayerName = input[2];

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;

            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    string url = $"https://localhost:7088/Player/{currentLastname}/{newPlayerClub}/{newPlayerName}";
                    Console.WriteLine($"Sending PUT request to URL: {url}");
                    HttpResponseMessage response = await client.PutAsync(url, null);

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Status: {response.StatusCode}, Response Body: {responseBody}");

                    if (response.IsSuccessStatusCode)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Player updated successfully.");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Failed to update player. Error: {responseBody}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"An error occurred: {ex.Message}");
                }
            }
        }

        private async Task HandleDeletePlayerRequest(Message message)
        {
            var lastname = message.Text.Substring(1);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7088/Player/{lastname}";
                    HttpResponseMessage response = await client.DeleteAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Player deleted successfully.");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Failed to delete player.");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"An error occurred: {ex.Message}");
                }
            }
        }

        private async Task HandleClearDatabaseRequest(Message message)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7088/Player/ClearDatabase";
                    HttpResponseMessage response = await client.DeleteAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "All players deleted successfully.");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Failed to clear database.");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"An error occurred: {ex.Message}");
                }
            }
        }


        private async Task HandleStandingsRequest(Message message)
        {
            var clubName = message.Text.Substring(1);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7088/api/Standings/{clubName}";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var standingsData = JsonConvert.DeserializeObject<GetStandingsModels.ApiResponse>(jsonResponse);

                        if (standingsData.Response.Count == 0)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Standings not found.");
                        }
                        else
                        {
                            var standings = standingsData.Response[0].League.Standings[0];

                            StringBuilder responseText = new StringBuilder();
                            responseText.AppendLine("```");
                            responseText.AppendLine("Standings:");
                            responseText.AppendLine(string.Format("{0,-3} | {1,-17} | {2,-5} | {3,-3} | {4,-3} | {5,-3} | {6,-7} | {7,-6}", "№", "Team", "Games", "W", "D", "L", "GF-GA", "Points"));

                            foreach (var standing in standings)
                            {
                                responseText.AppendLine(string.Format("{0,-3} | {1,-17} | {2,-5} | {3,-3} | {4,-3} | {5,-3} | {6,-7} | {7,-6}",
                                    standing.Rank,
                                    standing.Team.Name,
                                    standing.All.Played,
                                    standing.All.Win,
                                    standing.All.Draw,
                                    standing.All.Lose,
                                    $"{standing.All.Goals.For}-{standing.All.Goals.Against}",
                                    standing.Points));
                            }

                            responseText.AppendLine("```");

                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: responseText.ToString(), parseMode: ParseMode.Markdown);
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Failed to retrieve standings.");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"An error occurred: {ex.Message}");
                }
            }
        }

        private async Task HandleTopScorersRequest(Message message)
        {
            var clubName = message.Text.Substring(1);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7088/TopScorers?TeamName={clubName}";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var topScorersData = JsonConvert.DeserializeObject<TopScorersResponse>(jsonResponse);

                        if (topScorersData.Players.Count == 0)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Top scorers not found.");
                        }
                        else
                        {
                            StringBuilder responseText = new StringBuilder();
                            responseText.AppendLine("```");
                            responseText.AppendLine($"{clubName}`s top league goalscorers::");
                            responseText.AppendLine("№  | Player             | Nationality     | Age | Goals | Assists | Team");

                            int rank = 1;
                            foreach (var player in topScorersData.Players)
                            {
                                responseText.AppendLine($"{rank,-2} | {player.Name,-18} | {player.Nationality,-15} | {player.Age,-3} | {player.Goals,-5} | {player.Assists,-7} | {player.Team}");
                                rank++;
                            }

                            responseText.AppendLine("```");

                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: responseText.ToString(), parseMode: ParseMode.Markdown);
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Failed to retrieve top scorers.");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"An error occurred: {ex.Message}");
                }
            }
        }


        private async Task HandlePlayerComparisonRequest(Message message)
        {
            var input = message.Text.Substring(1).Split('_');
            if (input.Length < 2)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Invalid format. Use &clubname_playername");
                return;
            }

            string clubName = input[0];
            string playerName = input[1];

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7088/PlayerComparison/ComparePlayer/{Uri.EscapeDataString(clubName)}/{Uri.EscapeDataString(playerName)}";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var comparisonData = JsonConvert.DeserializeObject<PlayerComparison.Player[]>(jsonResponse);

                        if (comparisonData == null || comparisonData.Length == 0)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Comparison data not found.");
                        }
                        else
                        {
                            StringBuilder responseText = new StringBuilder($"Comparison for {playerName} from {clubName}:\n");
                            foreach (var comp in comparisonData)
                            {
                                responseText.AppendLine($"<b>Player:</b> {comp.firstname} {comp.lastname}");
                                responseText.AppendLine($"<b>Position:</b> {comp.position}");
                                responseText.AppendLine($"<b>Goals:</b> {comp.total}");
                                responseText.AppendLine($"<b>Assists:</b> {comp.assists}");
                                responseText.AppendLine($"<b>Goals Comparison:</b> {comp.goalsComparison}");
                                responseText.AppendLine($"<b>Assists Comparison:</b> {comp.assistsComparison}");
                            }

                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: responseText.ToString(), parseMode: ParseMode.Html);
                        }
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Failed to retrieve comparison data. Status Code: {response.StatusCode}, Error: {errorResponse}");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"An error occurred: {ex.Message}");
                }
            }
        }

        private async Task HandleListPlayersRequest(Message message)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "https://localhost:7088/Player/ListPlayers";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var playersData = JsonConvert.DeserializeObject<List<Players>>(jsonResponse);

                        if (playersData.Count == 0)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "No players found in the database.");
                        }
                        else
                        {
                            StringBuilder responseText = new StringBuilder("List of favorite players:\n");
                            foreach (var player in playersData)
                            {
                                responseText.Append($"<b>Player:</b> {player.Firstname} {player.Lastname}\n");
                                responseText.Append($"<b>Age:</b> {player.Age}\n");
                                responseText.Append($"<b>Nationality:</b> {player.Nationality}\n");
                                responseText.Append($"<b>Club:</b> {player.Clubname}\n");
                                responseText.Append($"<b>Position:</b> {player.Position}\n\n");
                            }

                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: responseText.ToString(), parseMode: ParseMode.Html);
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Failed to retrieve list of players.");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"An error occurred: {ex.Message}");
                }
            }
        }


        private async Task HandleLiveMatchesRequest(Message message)
        {
            matchIndex = 0;
            await LoadMatches();

            if (matches.Count == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "No live matches found.");
                return;
            }

            await DisplayMatches(message.Chat.Id);
        }

        private async Task LoadMatches()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "https://localhost:7088/api/Soccer/live";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var soccerApiResponse = JsonConvert.DeserializeObject<SoccerApiResponse>(jsonResponse);
                        matches = soccerApiResponse.Result;
                    }
                    else
                    {
                        matches.Clear();
                    }
                }
                catch (Exception)
                {
                    matches.Clear();
                }
            }
        }

        private async Task DisplayMatches(long chatId)
        {
            int count = matches.Count;
            int endIndex = Math.Min(matchIndex + 5, count);

            if (matchIndex >= count)
            {
                await botClient.SendTextMessageAsync(chatId, "No more matches.");
                return;
            }

            var sb = new StringBuilder();
            for (int i = matchIndex; i < endIndex; i++)
            {
                var match = matches[i];
                sb.AppendLine($"<b>{match.Championship.Name}</b>");
                sb.AppendLine($"{match.TeamA.Name} vs {match.TeamB.Name}");
                sb.AppendLine($"<i>Date:</i> {match.Date}");
                sb.AppendLine($"<i>Timer:</i> {match.Timer}");
                sb.AppendLine($"<i>Score:</i> {match.TeamA.Score.F} - {match.TeamB.Score.F}");
                sb.AppendLine();
            }

            matchIndex = endIndex;

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Next", "next")
            });

            await botClient.SendTextMessageAsync(chatId, sb.ToString(), parseMode: ParseMode.Html, replyMarkup: inlineKeyboard);
        }

        private async Task HandleNextMatches(Message message)
        {
            await DisplayMatches(message.Chat.Id);
        }
    }
}