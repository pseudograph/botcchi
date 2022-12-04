using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using JikanDotNet;

namespace Botcchi.Commands;

public class AnimeCommands : ApplicationCommandModule
{
    [SlashCommand("nextep", "Time till next episode")]
    public async Task NextEp(InteractionContext ctx, [Option("title", "Title of series")]
        string title)
    {
        IJikan jikan = new Jikan();
        var animeSearchResult = await jikan.SearchAnimeAsync(title);
        IEnumerable<string> rawSearchResultTitles = GetSearchResultTitles(animeSearchResult);
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        Console.WriteLine("===MAL QUERY RESULTS===");
        
        foreach (var anime in animeSearchResult.Data)
        {
            Console.WriteLine("Title: " + anime.Titles.First().Title + '\n');
        }
        var backupFirstResult = animeSearchResult.Data.First();
        CleanResults(ref animeSearchResult, title);
        if (animeSearchResult.Data.Count == 0) animeSearchResult.Data.Add(backupFirstResult);
        string response = string.Empty;
        
        if (!animeSearchResult.Data.First().Airing || animeSearchResult.Data.Count == 0)
        {
            response += "No results. Try entering the title in full.";
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(response));
            return;
        }

        try
        {
            var firstResult = animeSearchResult.Data.First();
            response += "Title: " + firstResult.Titles.First().Title + "\n";
            response += "Schedule: " + firstResult.Broadcast.Day + " " + firstResult.Broadcast.Time + "\n";
            var timeGap = GetTimeGap(firstResult, DateTime.Now);

            response += "Next episode in: " + timeGap.Days + " days, " + timeGap.Hours + " hours, and " +
                        timeGap.Minutes + " minutes.";

            Console.WriteLine("===TITLE===" + title);
            Console.WriteLine("===BROADCAST===" + response);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(response));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            response = rawSearchResultTitles.Aggregate("Can't pick an airing anime that matches what you entered. Help me choose by entering the title in full.\n" +
                                                       "Unfiltered MAL query results (what you're looking for might be in here!):\n",
                                                       (current, anime) => current + (anime + '\n'));
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(response));
        }
    }

    private static IEnumerable<string> GetSearchResultTitles(BaseJikanResponse<ICollection<Anime>> animeSearchResult)
    {
        return animeSearchResult.Data.Select(anime => anime.Titles.First().Title).ToList();
    }

    private static void CleanResults(ref PaginatedJikanResponse<ICollection<Anime>> animeSearchResult, string title)
    {
        string[] keywords = title.Split(' ');
        foreach (var anime in animeSearchResult.Data.ToList())
        {
            if (!anime.Airing) animeSearchResult.Data.Remove(anime);
            if (!KeywordsInTitle(anime, keywords))
            {
                Console.WriteLine("Removing anime: " + anime.Titles.First().Title);
                animeSearchResult.Data.Remove(anime);
            }
        }
    }

    private static bool KeywordsInTitle(Anime anime, string[] keywords)
    {
        foreach (var title in anime.Titles)
        {
            bool titleContainsKeywords = true;
            foreach (string keyword in keywords)
            {
                if (!title.Title.ToLower().Contains(keyword.ToLower()))
                {
                    titleContainsKeywords = false;
                }
            }

            if (titleContainsKeywords) return true;
        }

        return false;
    }

    private static TimeSpan GetTimeGap(Anime firstResult, DateTime now)
    {
        DayOfWeek day = firstResult.Broadcast.Day switch
        {
            "Sundays" => DayOfWeek.Sunday,
            "Mondays" => DayOfWeek.Monday,
            "Tuesdays" => DayOfWeek.Tuesday,
            "Wednesdays" => DayOfWeek.Wednesday,
            "Thursdays" => DayOfWeek.Thursday,
            "Fridays" => DayOfWeek.Friday,
            "Saturdays" => DayOfWeek.Saturday,
            _ => 0
        };

        int dayGap = (day - now.DayOfWeek);
        if (dayGap <= 0) dayGap += 7;
        string hour = firstResult.Broadcast.Time;
        string[] hourMinute = hour.Split(':');
        TimeZoneInfo.ConvertTimeToUtc(now);
        var broadcast = new DateTime(now.Year, now.Month, now.Day + dayGap, Convert.ToInt32(hourMinute[0]), Convert.ToInt32(hourMinute[1]), 0);
        return broadcast - now;
    }
    
}