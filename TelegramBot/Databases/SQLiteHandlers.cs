using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SQLite;

namespace TelegramBot.Databases
{
    public static class SqLiteHandlers
    {
        private static string _databaseName = "Users.db";
        private static string _databasePath = Directory.GetCurrentDirectory() + "/Databases/";
        private static string _fullPathToDatabase = _databasePath + _databaseName;

        public static Dictionary<long, User> Users { get; private set; } = new Dictionary<long, User>();

        /// <summary>
        /// Standard location for the directory with the database <b><i>{Path to the program}/Database/Users.db</i></b>
        /// </summary>
        /// <returns></returns>
        public static bool DatabaseExist() => File.Exists(_fullPathToDatabase);

        private static async Task<int> ExecuteNonQueryCommandAsync(this SQLiteCommand command)
        {
            int number = -1;

            try
            {
                using SQLiteConnection connection = new SQLiteConnection("DataSource = " + _fullPathToDatabase + ";");
                command.Connection = connection;
                connection.Open();
                number = await command.ExecuteNonQueryAsync();
                connection.Close();
            }
            catch (SQLiteException exc)
            {
                Console.WriteLine(exc.Message);
            }

            return number;
        }

        public static void CreateDatabaseIfMissingAsync()
        {
            if (DatabaseExist())
                return;

            try
            {
                Directory.CreateDirectory(_databasePath);
                SQLiteConnection.CreateFile(_fullPathToDatabase);
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception);
                return;
            }

            string commandText =
                "CREATE TABLE USERS (TelegramId INTEGER NOT NULL PRIMARY KEY UNIQUE, Username TEXT DEFAULT '', NumberOfWins INTEGER DEFAULT 0, ConceivedNumber INTEGER DEFAULT 0)";

            SQLiteCommand command = new SQLiteCommand(commandText);
            command.ExecuteNonQueryCommandAsync();
        }

        public static void UpdateUserList()
        {
            Users.Clear();

            using (SQLiteConnection connection = new SQLiteConnection("DataSource = " + _fullPathToDatabase + ";"))
            {
                connection.Open();

                string commandText =
                    "SELECT * FROM USERS ORDER BY NumberOfWins DESC;";

                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string username = reader["Username"].ToString() ?? "";
                            Users.Add(reader.GetInt64(0), 
                                new User(username, reader.GetInt32(2), reader.GetByte(3)));
                        }
                    }
                }

                connection.Close();
            }
        }

        public static async void AddUserToDatabaseAsync(long telegramId, string username)
        {
            if (Users.ContainsKey(telegramId))
                return;

            await AddUser(telegramId, username);

            static async Task AddUser(long telegramId, string username)
            {
                Users.Add(telegramId, new User(username));

                string commandText =
                    "INSERT INTO USERS (TelegramId, Username) VALUES (@telegramId, @username)";

                SQLiteCommand command = new SQLiteCommand(commandText);
                command.Parameters.AddWithValue("@telegramId", telegramId);
                command.Parameters.AddWithValue("@username", username);

                await command.ExecuteNonQueryCommandAsync();
                
                Console.WriteLine($"Пользователь {telegramId} добавлен в базу данных.");
            }
        }

        public static async void UpdateUserDataAsync(long telegramId, User user)
        {
            await UpdateUserValue(telegramId, user);
            
            static async Task UpdateUserValue(long telegramId, User user)
            {
                string commandText =
                    "UPDATE USERS SET Username = @username, NumberOfWins = @numberOfWins, ConceivedNumber = @conceivedNumber WHERE TelegramId = @telegramId";

                SQLiteCommand command = new SQLiteCommand(commandText);
                command.Parameters.AddWithValue("@username", user.UserName);
                command.Parameters.AddWithValue("@numberOfWins", user.NumberOfWins);
                command.Parameters.AddWithValue("@conceivedNumber", user.ConceivedNumber);
                command.Parameters.AddWithValue("@telegramId", telegramId);

                await command.ExecuteNonQueryCommandAsync();
            }
        }
    }
}