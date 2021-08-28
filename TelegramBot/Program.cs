using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;


namespace TelegramBot
{
    static class Program
    {
        private static TelegramBotClient? Bot;

        public static async Task Main()
        {
            Bot = new TelegramBotClient(Configuration.BotToken);

            var me = await Bot.GetMeAsync();
            if (me.Username != null) Console.Title = me.Username;

            using var cts = new CancellationTokenSource();
            
            Bot.StartReceiving(new DefaultUpdateHandler(Handlers.HandleUpdateAsync, Handlers.HandleErrorAsync), cancellationToken: cts.Token);
            
            Console.WriteLine($"Сервер запущен! Моё айди - {me.Id}, а также моё имя - {me.FirstName}.");
            Console.ReadLine();
            
            cts.Cancel();
        }
    }
}