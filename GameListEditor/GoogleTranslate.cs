using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace GameListEditor
{
    public class GoogleTranslate
    {

        public class GoogleTranslateResponse
        {
            public class GoogleTranslateTranslatedText
            {
                public string translatedText;
            }
            public class GoogleTranslateData
            {
                public List<GoogleTranslateTranslatedText> translations;
            }

            public GoogleTranslateData data;
        }
        public static string Translate(string q, string source, string target)
        {

            //string URL_PROXY = "http://cxl-services.appspot.com/proxy";
            //string URL_API = "https://translation.googleapis.com/language/translate/v2/";

            string URL_ENCODED = "http://cxl-services.appspot.com/proxy?url=https%3A%2F%2Ftranslation.googleapis.com%2Flanguage%2Ftranslate%2Fv2%2F%3F%26source%3Den%26target%3Dpt";

            StringBuilder queryString = new StringBuilder(URL_ENCODED);

            string[] qSplit = q.Split(new char[] { '\n' });

            foreach (string item in qSplit)
            {
                queryString.Append("%26q%3D");
                queryString.Append(HttpUtility.UrlEncode(item).Replace("%2b", " ").Replace("%20", " ").Replace("%27", "'").Replace("+", " ").Replace("%2c", ","));
            }

            string urlProxy = queryString.ToString();


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlProxy);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0";
            request.Accept = "*/*";


            try
            {


                // response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string result;
                using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
                {
                    result = rdr.ReadToEnd();
                }

                JavaScriptSerializer js = new JavaScriptSerializer();
                GoogleTranslateResponse jsonData = js.Deserialize(result, typeof(GoogleTranslateResponse)) as GoogleTranslateResponse;

                List<string> translatedTexts = new List<string>();


                if (jsonData != null)
                {
                    foreach (GoogleTranslateResponse.GoogleTranslateTranslatedText text in jsonData.data.translations)
                    {
                        translatedTexts.Add(text.translatedText);
                    }
                }

                return HttpUtility.HtmlDecode(string.Join(Environment.NewLine, translatedTexts));
            }
            catch
            {

            }
            return String.Empty;

        }
    }
}
