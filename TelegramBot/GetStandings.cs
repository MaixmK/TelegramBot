namespace GetStandingsModels
{
    public class Team
    {
        public string Name { get; set; }
    }

    public class Standings
    {
        public int Rank { get; set; }
        public Team Team { get; set; }
        public int Points { get; set; }
        public MatchStats All { get; set; }
    }

    public class MatchStats
    {
        public int Played { get; set; }
        public int Win { get; set; }
        public int Draw { get; set; }
        public int Lose { get; set; }
        public Goals Goals { get; set; }
    }

    public class Goals
    {
        public int For { get; set; }
        public int Against { get; set; }
    }

    public class League
    {
        public List<List<Standings>> Standings { get; set; }
    }

    public class ApiResponse
    {
        public List<Response> Response { get; set; }
    }

    public class Response
    {
        public League League { get; set; }
    }
}
