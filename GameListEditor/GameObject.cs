using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GameListEditor
{
    [Serializable]
    public class GameObject
    {
        public string path;
        public string name;
        public string desc;
        public string desc_original;
        public bool translated;
        public string image;
        public string releasedate;
        public string developer;
        public string publisher;
        public string favorite;
        public string genre;
        public string players;
        public string playcount;
        public string lastplayed;
    }
}
