using System;
using System.Collections.Generic;
using System.Text;

namespace AudioPlayer.Models
{
    public class Audio
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Url { get; set; }
        public string CoverImage { get; set; }
        public bool IsRecent { get; set; }
        public DateTime Date { get; set; }
    }
}
