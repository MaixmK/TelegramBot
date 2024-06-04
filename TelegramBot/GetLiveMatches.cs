using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveMatches
{
    public class SoccerApiResponse
    {
        public int Status { get; set; }
        public object[] Errors { get; set; }
        public object[] Pagination { get; set; }
        public List<Result> Result { get; set; }
    }

    public class Result
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string Timer { get; set; }
        public bool EstETime { get; set; }
        public bool InPlay { get; set; }
        public Championship Championship { get; set; }
        public Team TeamA { get; set; }
        public Team TeamB { get; set; }
        public DominanceIndex[] DominanceIndex { get; set; }
    }

    public class Championship
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SName { get; set; }
        public string Country { get; set; }
    }

    public class Team
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Score Score { get; set; }
    }

    public class Score
    {
        public string F { get; set; }
        public string _1H { get; set; }
        public string _2H { get; set; }
    }

    public class DominanceIndex
    {
        public string Timer { get; set; }
        public double TeamA { get; set; }
        public double TeamB { get; set; }
    }
}
