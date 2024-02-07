using System.ServiceModel.Syndication;
using System.Xml;

namespace PalServerTools.Utils
{
    public class RssUtil
    {
        public static List<SyndicationItem> ReadRss(string rssUrl)
        {
            List<SyndicationItem> items = new List<SyndicationItem>();

            using (XmlReader reader = XmlReader.Create(rssUrl))
            {
                SyndicationFeed feed = SyndicationFeed.Load(reader);

                foreach (SyndicationItem item in feed.Items)
                {
                    items.Add(item);
                }
            }

            return items;
        }
    }
}
