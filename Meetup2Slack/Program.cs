using Newtonsoft.Json;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Meetup2Slack
{
    class Program
    {
        static Uri _redisUrl = new Uri(ConfigurationManager.AppSettings["REDISTOGO_URL"]);
        static string _meetupNewsUrl = ConfigurationManager.AppSettings["MEETUP_NOTIFICATION_URL"];
        static string _slackWebhookUrl = ConfigurationManager.AppSettings["SLACK_WEBHOOK_URL"];

        static int _meetupGroupId = 18916580;
        static string _lastNotificationIDKey = "LastMeetupNotificationID";
        
        static HttpClient _http = new HttpClient();

        static void Main(string[] args) 
        {
            Trace.TraceInformation("MEETUP_NOTIFICATION_URL: {0}", _meetupNewsUrl);
            Trace.TraceInformation("SLACK_WEBHOOK_URL: {0}", _slackWebhookUrl);
            Trace.TraceInformation("REDISTOGO_URL: {0}", _redisUrl);
            
            while(true) 
            {
                using(var redis = new RedisClient(_redisUrl)) 
                {                        
                    var lastNotificationID = redis.Get<long>(_lastNotificationIDKey);
                    
                    var news = GetMeetupNotifications();
                                                
                    var freshNews = news.Where(n => n.id > lastNotificationID);
                        
                    if(freshNews.Any()) 
                    {
                        var relevantNews = freshNews.Where(n => n.target.group_id == _meetupGroupId);
                        
                        foreach(var item in relevantNews) { 
                            PostNotificationToSlack(item);
                        }
                               
                        redis.Set(_lastNotificationIDKey, news.Max(n => n.id));
                    }                     
                }

                Thread.Sleep(60000);
            }

        }



        static IEnumerable<MeetupNotification> GetMeetupNotifications() 
        {
            var httpResult = _http.GetAsync(_meetupNewsUrl).Result.Content.ReadAsStringAsync().Result;
            
            Trace.TraceInformation("From Meetup: {0}", httpResult);
            
            return JsonConvert.DeserializeObject<List<MeetupNotification>>(httpResult);
        }


        static void PostNotificationToSlack(MeetupNotification meetupNews) 
        {
            var content = new StringContent(
                                JsonConvert.SerializeObject(new { text = PrepareSlackMessage(meetupNews) }),
                                Encoding.UTF8,
                                "application/json");

            _http.PostAsync(_slackWebhookUrl, content).Wait();
        }

                      


        static string PrepareSlackMessage(MeetupNotification meetupNews) {
            var desc = Regex.Replace(meetupNews.text, @"<b>(.*?)<\/b>", @"*$1*");

            return string.Format("{0}\n[<{1}|link>]", desc, meetupNews.link);
        }



    }
}
