namespace GetTopScorers
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Nationality { get; set; }
        public string Photo { get; set; }
        public int Age { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public string Team { get; set; }
    }

    public class TopScorersResponse
    {
        public List<Player> Players { get; set; }

        public TopScorersResponse()
        {
            Players = new List<Player>();
        }
    }
}
