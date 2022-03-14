using System;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.AudioBypassService.Extensions;
using System.Threading;

namespace Vk
{
    class Program
    {
       
        static void Main(string[] args)
        {
        //цикл для того, чтобы новости кидались каждые 30 минут
            while (true)
            {
                //создали VkApi
                var api = CreateVkApi();

                var message = GetMessage();

                SendMessage(api, message);

                Thread.Sleep(1800000);
            }
        }

        private static VkApi CreateVkApi()
        {
            var api = new VkApi();
            //Авторизация по токену (токен получаем вконтакте)
            api.Authorize(new ApiAuthParams() { AccessToken = UserSettings.AccessToken });
            
            return api;
        }

        private static void SendMessage(VkApi api, string message)
        {
            api.Messages.Send(new MessagesSendParams
            {
                //Id кому отправляем новости (вписан мой)
                PeerId = 55132430,
                Message = message,
                RandomId = new Random().Next()
            });
        }

        private static string GetMessage()
        {
            //сохраняем адрес странички:
            var html = @"https://www.onliner.by/";
            
            //создаем объект HtmlWeb:
            var web = new HtmlWeb();
            
            //загружаем страничкуц
            var htmlDoc = web.Load(html);
            
            //получаем новости:
            var news = GetNews(htmlDoc);
            
            //сообщение для отправки вк
            var message = "Свежие новости: \n" + string.Join("\n", news);
            
            return message;
        }

        private static string[] GetNews(HtmlDocument htmlDoc)
        {
            //массив новостей
            var news = new string[6];
            var end = 3;
            var numberOfNews = 1;
            var index = 0;
            for (int i = 0; i < end; i++)
            {
                try
                {
                    //XPath названия новости:
                    news[index] = $"{numberOfNews}. " +
                                  htmlDoc.DocumentNode.SelectSingleNode($"//*[@id=\"widget-{i + 1}-1\"]/a[1]/h3/span")
                                      .InnerHtml;
                    news[index] = news[index].Replace("&laquo;", "\"");
                    news[index] = news[index].Replace("&raquo;", "\"");
                    var euro = (char) 8364;
                    news[index] = news[index].Replace("&euro;", euro.ToString());
                }
                catch (Exception)
                {
                    end++;
                    continue;
                }

                index++;
                
                //добавляем ссылку на новость
                AddLink(news, index, i + 1, htmlDoc);
                index++;
                numberOfNews++;
            }
            return news;
        }

        private static void AddLink(string[]news, int index, int i, HtmlDocument htmlDoc)
        {
            var tempLink= htmlDoc.DocumentNode.SelectSingleNode($"//*[@id=\"widget-{i}-1\"]/a[1]").OuterHtml;

            var link = "";
            for (var j = 1; j < tempLink.Length; j++)
            {
                if (tempLink[j - 1] == '"')
                {
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
}