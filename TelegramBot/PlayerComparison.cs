using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerComparison
{
    public class PlayerComparison
    {
        public Player[] Property1 { get; set; }
    }

    public class Player
    {
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string position { get; set; }
        public int total { get; set; }
        public int assists { get; set; }
        public string goalsComparison { get; set; }
        public string assistsComparison { get; set; }
    }

}
