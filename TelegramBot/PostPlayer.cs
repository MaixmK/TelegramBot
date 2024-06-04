using System;
using System.Collections.Generic;

namespace PostPlayer
{
    public class PostPlayer
    {
        public List<Players> Players { get; set; }
    }

    public class Players
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public int Age { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Nationality { get; set; }
        public string Clubname { get; set; }
        public string Position { get; set; }
        public string Rating { get; set; }
        public int? Total { get; set; }
        public int? Assists { get; set; }
    }
}
