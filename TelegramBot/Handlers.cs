using System;
using System.Net;
using System.Web;

using System.Linq;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    public static class Handlers
    {
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException =>
                    $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message),
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Пользователь '{message.Chat.Id}' отправил сообщение типа - {message.Type}");

            if (message.Type != MessageType.Text)
                return;

            Console.WriteLine("Текст сообщения: " + message.Text);
            
            var action = (message.Text?.Split(" ").First()) switch
            {
                "Сумма:" => SumOfNumbers(botClient, message),
                "Перевод:" => TranslateString(botClient, message),
                "Знаки:" => NumberOfSings(botClient, message),
                _ => Usage(botClient, message),
            };
            
            var sentMessage = await action;
            Console.WriteLine($"В ответ было отпавлено сообщене с идентификатором: {sentMessage.MessageId}");

            static async Task<Message> SumOfNumbers(ITelegramBotClient botClient, Message message)
            {
                int result;
                try
                {
                    result = message.Text.Split(" ")
                        .Skip(1)
                        .Select(str => Convert.ToInt32(str))
                        .Sum();
                }
                catch (FormatException)
                {
                    result = 0;
                }

                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"*Сумма равна: {result}*",
                    ParseMode.Markdown);
            }

            static async Task<Message> TranslateString(ITelegramBotClient botClient, Message message)
            {
                string result;
                try
                {
                    string forTranslation = message.Text?.Substring(message.Text.IndexOf(' '));
                    
                    string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=ru&dt=t&q={HttpUtility.UrlEncode(forTranslation)}";
                    result = new WebClient { Encoding = System.Text.Encoding.UTF8 }.DownloadString(url);
                    result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
                    
                }
                catch (Exception exc)
                {
                    result = "*Произошла ошибка при переводе!*";
                }

                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: result, 
                    ParseMode.MarkdownV2);
            }

            static async Task<Message> NumberOfSings(ITelegramBotClient botClient, Message message)
            {
                SortedDictionary<char, int> singsDictionary = new SortedDictionary<char, int>();

                for (int i = message.Text.IndexOf(' ') + 1; i < message.Text.Length; i++)
                {
                    if (singsDictionary.ContainsKey(message.Text[i]))
                    {
                        singsDictionary[message.Text[i]]++;
                    }
                    else
                    {
                        singsDictionary[message.Text[i]] = 1;
                    }
                }

                string result = "*Всего: " + singsDictionary.Values.Sum() + " шт.*\n\n";

                foreach (var singsDictionaryKey in singsDictionary.Keys)
                {
                    result += ((singsDictionaryKey == ' ') ? "Пробел" : singsDictionaryKey) + " - " + 
                              singsDictionary[singsDictionaryKey] + " шт.\n";
                }

                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: result,
                    ParseMode.Markdown);
            }

            static async Task<Message> Usage(ITelegramBotClient botClient, Message message)
            {
                const string usage = "*Команды:*\n\n" +
                                     "`Сумма: [числа через пробел]`*  -  сумма указаных чисел*\n\n" +
                                     "`Перевод: [фраза]`*  -  перевод указаного преложения*\n\n" +
                                     "`Знаки: [фраза]`*  -  детальный вывод количества знаков*";
                
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: usage,
                    ParseMode.Markdown);
            }
        }
    }
}