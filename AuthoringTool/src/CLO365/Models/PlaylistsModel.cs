using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLO365.Models
{
    public class PlaylistsModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Level { get; set; }
        public string Audience { get; set; }
        public string Technology { get; set; }
        public string Subject { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Source { get; set; }
        public string CatId { get; set; }
        public string Description { get; set; }
        public string StatusTag { get; set; }
        public List<string> Assets { get; set; }
        public PlaylistsModel()
        {
            Assets = new List<string>();
        }

    }
}
