namespace API_Football.Models
{
    public class TeamStatisticsResponse
    {
        public Parameters Parameters { get; set; }
        public Response Response { get; set; }
    }

    public class Parameters
    {
        public string League { get; set; }
        public string Season { get; set; }
        public string Team { get; set; }
    }

    public class Response
    {
        public Biggest Biggest { get; set; }
        public Fixtures Fixtures { get; set; }
        public Goals Goals { get; set; }
        public League League { get; set; }
        public Team Team { get; set; }
    }

    public class Biggest
    {
        public GoalsFor Goals { get; set; }
        public Wins Wins { get; set; }
    }

    public class GoalsFor
    {
        public int Away { get; set; }
        public int Home { get; set; }
    }

    public class Wins
    {
        public string Away { get; set; }
        public string Home { get; set; }
    }

    public class Fixtures
    {
        public FixtureDetails Played { get; set; }
        public FixtureDetails Wins { get; set; }
        public FixtureDetails Draws { get; set; }
        public FixtureDetails Loses { get; set; }
    }

    public class FixtureDetails
    {
        public int Away { get; set; }
        public int Home { get; set; }
        public int Total { get; set; }
    }

    public class Goals
    {
        public GoalsFor Average { get; set; }
        public Total Total { get; set; }
    }

    public class Total
    {
        public int Away { get; set; }
        public int Home { get; set; }
    }

    public class League
    {
        public string Country { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Season { get; set; }
    }

    public class Team
    {
        public string Name { get; set; }
    }
}
