namespace TelegramBot.Databases
{
    public class User
    {
        public string UserName{ get; set; }
        public int NumberOfWins { get; set; }
        public byte ConceivedNumber { get; set; }
        public int NumberOfAttempts { get; set; }

        public User(string userName, int numberOfWins = 0, byte conceivedNumber = 0)
        {
            UserName = userName;
            NumberOfWins = numberOfWins;
            ConceivedNumber = conceivedNumber;
            NumberOfAttempts = 0;
        }
    }
}