namespace TelegramBot.Databases
{
    public class Users
    {
        public long TelegramId { get; set; }
        public int NumberOfWins { get; set; }
        public byte ConceivedNumber { get; set; }

        public Users(long telegramId, int numberOfWins, byte conceivedNumber)
        {
            TelegramId = telegramId;
            NumberOfWins = numberOfWins;
            ConceivedNumber = conceivedNumber;
        }
    }
}