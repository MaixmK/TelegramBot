using System;
namespace GetPlayerModels
{
    public class GetPlayer
    {
        public List<ResponsePlayer> Response { get; set; }
        public GetPlayer()
        {
            Response = new List<ResponsePlayer>();
        }
    }

    public class ResponsePlayer
    {
        public Player Player { get; set; }
        public List<Statistics> Statistics { get; set; }
        public ResponsePlayer()
        {
            Statistics = new List<Statistics>();
            Player = new Player();
        }
    }

    public class Player
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public int Age { get; set; }
        public string Nationality { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Photo { get; set; }
    }

    public class Statistics
    {
        public TeamPlayer Team { get; set; }
        public League League { get; set; }
        public Games Games { get; set; }
        public Shots Shots { get; set; }
        public Goals Goals { get; set; }
        public Cards Cards { get; set; }
    }

    public class TeamPlayer
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
    }

    public class League
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Logo { get; set; }
        public string Flag { get; set; }
        public int? Season { get; set; }
    }

    public class Games
    {
        public int? Appearences { get; set; }
        public string Position { get; set; }
        public string Rating { get; set; }
    }
    public class Shots
    {
        public int? Total { get; set; }
        public int? On { get; set; }
    }
    public class Goals
    {
        public int? Total { get; set; }
        public int? Assists { get; set; }
    }

    public class Cards
    {
        public int? Yellow { get; set; }
        public int? Yellowred { get; set; }
        public int? Red { get; set; }
    }
}

