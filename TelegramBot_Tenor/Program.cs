using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using TenorSharp;
using RestSharp;
using TenorSharp.SearchResults;
using System.Runtime.InteropServices.JavaScript;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace TelegramBotExperiments
{

    public class Result
    {
        public string id { get; set; }
        public string title { get; set; }
        public MediaFormats media_formats { get; set; }
        public double created { get; set; }
        public string content_description { get; set; }
        public string itemurl { get; set; }
        public string url { get; set; }
        public List<string> tags { get; set; }
        public List<object> flags { get; set; }
        public bool hasaudio { get; set; }
        public string content_description_source { get; set; }
    }

    public class Root
    {
        public List<Result> results { get; set; }
        public string next { get; set; }
    }
    public class MediaFormats
    {
        public Gif gif { get; set; }
    }

    public class Gif
    {
        public string url { get; set; }
        public double duration { get; set; }
        public string preview { get; set; }
        public List<int> dims { get; set; }
        public int size { get; set; }
    }

    class TelegramBot
    {
        const string api_key = "AIzaSyBN6yzIzb98wDw4mmhgDEqWu3JlkANiDTs";
        static ITelegramBotClient bot = new TelegramBotClient("7506024037:AAHO1S7cn_cmFdQZQbzeeaYWrKWk70RUiUY");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update) + "\n");
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text?.ToLower() == "/start")
                {
                    await botClient.SendMessage(message.Chat.Id, "Введите ключевые слова для поиска GIF.");
                    return;
                }
                var request = "https://tenor.googleapis.com/v2/search?q=" + message.Text + "&key=" + api_key + "&limit=1" + "&random=true";
                //Console.WriteLine(request);
                using (var webClient = new HttpClient())
                {
                    HttpResponseMessage response = await webClient.GetAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var root = JsonSerializer.Deserialize<Root>(result);

                        if (root?.results != null && root.results.Count > 0)
                        {
                            string gifUrl = root.results[0].media_formats.gif.url;
                            if (!string.IsNullOrEmpty(gifUrl))
                                await botClient.SendMessage(message.Chat.Id, "Вот ваша гифка: \n" + gifUrl);
                            else
                                await botClient.SendMessage(message.Chat.Id, "Гифка не найдена.");
                        }
                        else
                            await botClient.SendMessage(message.Chat.Id, "Сообщение не принято.");
                    }
                    else
                        await botClient.SendMessage(message.Chat.Id, "Ошибка при получении данных.");
                }
            }
        }
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine("Error:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(exception) + "\n");
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMe().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}
