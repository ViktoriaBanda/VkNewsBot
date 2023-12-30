using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using System.Threading;

namespace Vk
{
    class Program
    {
        private const int WAITING_TIME = 1800000;
        private const int RECEIVER_ID = 55132430;
        private const int NEWS_COUNT = 3;
        private static readonly string _newsUrl = "https://www.onliner.by/";

        static void Main(string[] args)
        {
            while (true)
            {
                var api = CreateVkApi();

                var message = GetMessage();
                SendMessage(api, message);

                Thread.Sleep(WAITING_TIME);
            }
        }

        private static VkApi CreateVkApi()
        {
            var api = new VkApi();
            // Авторизация по токену (токен получаем вконтакте)
            api.Authorize(new ApiAuthParams
            {
                AccessToken = UserSettings.AccessToken
            });

            return api;
        }

        private static void SendMessage(VkApi api, string message)
        {
            api.Messages.Send(new MessagesSendParams
            {
                PeerId = RECEIVER_ID,
                Message = message,
                RandomId = new Random().Next()
            });
        }

        private static string GetMessage()
        {
            var web = new HtmlWeb();
            var htmlDoc = web.Load(_newsUrl);
            var news = GetNews(htmlDoc);

            var message = "Свежие новости: \n" + string.Join("\n", news);
            return message;
        }

        private static List<string> GetNews(HtmlDocument htmlDoc)
        {
            var news = new List<string>();
            var end = NEWS_COUNT;
            var numberOfNews = 1;
            var index = 0;
            for (var i = 0; i < end; i++)
            {
                try
                {
                    ParseNews(htmlDoc, news, index, numberOfNews, i);
                }
                catch (Exception)
                {
                    end++;
                    continue;
                }

                index++;

                AddLink(news, index, i + 1, htmlDoc);
                index++;
                numberOfNews++;
            }

            return news;
        }

        private static void ParseNews(HtmlDocument htmlDoc, List<string> news, int index, int numberOfNews, int i)
        {
            news[index] = $"{numberOfNews}. " +
                          htmlDoc.DocumentNode.SelectSingleNode($"//*[@id=\"widget-{i + 1}-1\"]/a[1]/h3/span")
                              .InnerHtml;
            news[index] = news[index].Replace("&laquo;", "\"");
            news[index] = news[index].Replace("&raquo;", "\"");
            var euro = (char)8364;
            news[index] = news[index].Replace("&euro;", euro.ToString());
        }

        private static void AddLink(IList<string> news, int index, int i, HtmlDocument htmlDoc)
        {
            var tempLink = htmlDoc.DocumentNode.SelectSingleNode($"//*[@id=\"widget-{i}-1\"]/a[1]").OuterHtml;

            var link = "";
            for (var j = 1; j < tempLink.Length; j++)
            {
                if (tempLink[j - 1] != '"')
                {
                    continue;
                }
                while (true)
                {
                    if (tempLink[j] == '"')
                    {
                        news[index] = link;
                        return;
                    }

                    link += tempLink[j];
                    j++;
                }
            }
        }
    }
}