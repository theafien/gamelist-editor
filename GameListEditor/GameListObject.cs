using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace GameListEditor
{
    [Serializable]
    [XmlRoot(ElementName = "gameList")]
    public class GameListObject
    {

        [XmlElement("game")]
        public List<GameObject> gameList;

        public static GameListObject LoadFromFile(string filename)
        {
            try
            {
                FileStream xmlStream = File.OpenRead(filename);
                XmlSerializer xml = new XmlSerializer(typeof(GameListObject));
                GameListObject gameListObject = xml.Deserialize(xmlStream) as GameListObject;
                xmlStream.Close();
                return gameListObject;

            }
            catch { }

            return null;
        }

        public bool SaveFile(string filename)
        {
            try
            {
                FileStream xmlStream = File.OpenWrite(filename);
                XmlSerializer xml = new XmlSerializer(typeof(GameListObject));
                xml.Serialize(xmlStream, this);
                xmlStream.Close();
                return true;

            }
            catch { }
            return false;
        }


    }
}
