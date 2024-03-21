using Microsoft.Extensions.Logging;
using System.ServiceModel.Syndication;
using System.Xml;

namespace PalServerTools.Utils
{
    public class RssUtil
    {
        public static async Task<List<SyndicationItem>> ReadRss(string rssUrl)
        {
            List<SyndicationItem> items = new List<SyndicationItem>();

            try
            {
                using (var httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(rssUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // 从HttpResponseMessage中读取流
                        Stream stream = await response.Content.ReadAsStreamAsync();

                        // 创建一个XML阅读器用于解析载入的流
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            // 使用SyndicationFeed解析RSS
                            SyndicationFeed feed = SyndicationFeed.Load(reader);

                            foreach (SyndicationItem item in feed.Items)
                            {
                                items.Add(item);
                            }
                        }
                    }
                    else
                    {
                        AppUtil.Logger.LogError("Error retrieving content.");
                    }
                }
            }
            catch (Exception ex)
            {
                AppUtil.Logger.LogError($"读取Rss({rssUrl})失败:{ex.Message}");
                throw new Exception($"读取Rss({rssUrl})失败:{ex.Message}");
            }

            return items;
        }
    }
}
