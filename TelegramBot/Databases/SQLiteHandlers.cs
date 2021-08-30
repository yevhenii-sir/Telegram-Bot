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

        public static List<Users> UsersList { get; private set; } = new List<Users>();

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

        public static async void CreateDatabaseIfMissingAsync()
        {
            if (DatabaseExist())
                return;

            await Task.Run(CreateDatabase);
            
            static void CreateDatabase()
            {
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
                    "CREATE TABLE USERS (TelegramId INTEGER NOT NULL PRIMARY KEY UNIQUE, NumberOfWins INTEGER DEFAULT 0, ConceivedNumber INTEGER DEFAULT 0)";

                SQLiteCommand command = new SQLiteCommand(commandText);
                command.ExecuteNonQueryCommandAsync();
            }
        }
        
        public static void UpdateUserList()
        {
            UsersList.Clear();

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
                            UsersList.Add(new Users(reader.GetInt64(0), 
                                reader.GetInt32(1),
                                reader.GetByte(2)));
                        }
                    }
                }
                
                connection.Close();
            }
        }

        public static async void AddUserToDatabaseAsync(long telegramId)
        {
            if (UsersList.Exists((user) => user.TelegramId == telegramId))
                return;

            await AddUser(telegramId);

            static async Task AddUser(long telegramId)
            {
                UsersList.Add(new Users(telegramId, 0, 0));
                
                string commandText =
                    "INSERT INTO USERS (TelegramId) VALUES (@telegramId)";

                SQLiteCommand command = new SQLiteCommand(commandText);
                command.Parameters.AddWithValue("@telegramId", telegramId);
                
                await command.ExecuteNonQueryCommandAsync();
            };
        }

        public static async void UpdateUserDataAsync(Users user)
        {
            await UpdateUserValue(user);
            
            static async Task UpdateUserValue(Users user)
            {
                string commandText =
                    "UPDATE USERS SET NumberOfWins = @numberOfWins, ConceivedNumber = @conceivedNumber WHERE TelegramId = @telegramId";

                SQLiteCommand command = new SQLiteCommand(commandText);
                command.Parameters.AddWithValue("@numberOfWins", user.NumberOfWins);
                command.Parameters.AddWithValue("@conceivedNumber", user.ConceivedNumber);
                command.Parameters.AddWithValue("@telegramId", user.TelegramId);

                await command.ExecuteNonQueryCommandAsync();
            }
        }
    }
}