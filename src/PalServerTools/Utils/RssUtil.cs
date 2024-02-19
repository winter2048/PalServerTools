using System.ServiceModel.Syndication;
using System.Xml;

namespace PalServerTools.Utils
{
    public class RssUtil
    {
        public static List<SyndicationItem> ReadRss(string rssUrl)
        {
            List<SyndicationItem> items = new List<SyndicationItem>();

            try
            {
                using (XmlReader reader = XmlReader.Create(rssUrl))
                {
                    SyndicationFeed feed = SyndicationFeed.Load(reader);

                    foreach (SyndicationItem item in feed.Items)
                    {
                        items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"读取Rss({rssUrl})失败:{ex.Message}");
            }

            return items;
        }
    }
}
