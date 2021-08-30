namespace TelegramBot.Databases
{
    public class User
    {
        public long TelegramId { get; set; }
        public int NumberOfWins { get; set; }
        public byte ConceivedNumber { get; set; }
        public int NumberOfAttempts { get; set; }

        public User(long telegramId, int numberOfWins, byte conceivedNumber)
        {
            TelegramId = telegramId;
            NumberOfWins = numberOfWins;
            ConceivedNumber = conceivedNumber;
            NumberOfAttempts = 0;
        }
    }
}