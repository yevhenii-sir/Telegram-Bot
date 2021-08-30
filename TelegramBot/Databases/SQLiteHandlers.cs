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
        
        private static int ExecuteNonQueryCommand(this SQLiteCommand command)
        {
            int number = -1;

            try
            {
                using var connection = new SQLiteConnection("DataSource = " + _fullPathToDatabase + ";");
                command.Connection = connection;
                connection.Open();
                number = command.ExecuteNonQuery();
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
                    "CREATE TABLE USERS (TelegramId INTEGER NOT NULL PRIMARY KEY UNIQUE, NumerOfWins INTEGER DEFAULT 0, ConceivedNumber INTEGER DEFAULT 0)";

                SQLiteCommand command = new SQLiteCommand(commandText);
                command.ExecuteNonQueryCommand();
            }
        }
    }
}