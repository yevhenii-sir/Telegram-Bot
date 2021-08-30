using System;
using System.Linq;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using TelegramBot.Databases;

namespace TelegramBot
{
    public static class Game
    {
        private static byte NumberOfAttempts { get; set; } = 3;
        
        static InlineKeyboardMarkup _inlineGameKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("1"),
                InlineKeyboardButton.WithCallbackData("2"),
                InlineKeyboardButton.WithCallbackData("3"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("4"),
                InlineKeyboardButton.WithCallbackData("5"),
                InlineKeyboardButton.WithCallbackData("6"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("7"),
                InlineKeyboardButton.WithCallbackData("8"),
                InlineKeyboardButton.WithCallbackData("9"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("0"), 
            }
        });
        
        public static async Task<Message> SendInlineGameKeyboard(ITelegramBotClient botClient, Message message)
        {
            var currentUser = SqLiteHandlers.UsersList.First(user => user.TelegramId == message.From.Id);

            currentUser.NumberOfAttempts = NumberOfAttempts;
            currentUser.ConceivedNumber = (byte)new Random().Next(10);
            SqLiteHandlers.UpdateUserDataAsync(currentUser);
            
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "<b><i>Угадай загаданную цыфру от 0 до 9.</i></b>",
                replyMarkup: _inlineGameKeyboard, 
                parseMode: ParseMode.Html);
        }
        
        public static async Task BotOnCallbackQueryReceivedAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var currentUser = SqLiteHandlers.UsersList.First(user => user.TelegramId == callbackQuery.From.Id);

            byte callbackQueryData = Convert.ToByte(callbackQuery.Data);

            if (--currentUser.NumberOfAttempts > 0)
            {
                if (callbackQueryData == currentUser.ConceivedNumber)
                {
                    currentUser.NumberOfAttempts = 0;
                    currentUser.NumberOfWins++;
                    SqLiteHandlers.UpdateUserDataAsync(currentUser);
                    
                    Console.WriteLine($"Игрок {currentUser.TelegramId} выирал!");

                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: $"<b><i>Вы угадали! Загаданная цифра была {callbackQueryData}. \n" +
                              $"Всего заработанных баллов: {currentUser.NumberOfWins} шт.</i></b>",
                        ParseMode.Html);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: $"<b><i>Вы не угадали. Попробуйте еще раз!. Осталось попыток: {currentUser.NumberOfAttempts} шт.</i></b>",
                        replyMarkup: _inlineGameKeyboard,
                        parseMode: ParseMode.Html);
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: $"<b><i>Вы завершили текущую игру, начните заново!!!\n" +
                          $"Команда: /game, удачи!</i></b>",
                    ParseMode.Html);
            }
        }

        public static async Task<Message> ShowUserRatings(ITelegramBotClient botClient, Message message)
        {
            SqLiteHandlers.UpdateUserList();
            string rating = "<b>Рейтинг:\n" +
                            $"{"ID", -22}Баллы</b>\n";

            foreach (var user in SqLiteHandlers.UsersList)
            {
                if (user.NumberOfWins == 0) break;

                rating += $"<i>{user.TelegramId, -14}{user.NumberOfWins}</i>\n";
            }

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: rating,
                ParseMode.Html);
        }
    }
}