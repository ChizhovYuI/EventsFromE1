using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace EventsInEkb
{
    public class EventsParser
    {
        public EventsParser(string url, DateTime startDate, DateTime endDate)
        {
            Url = url;
            StartDate = startDate;
            EndDate = endDate;
        }

        public string Url { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public List<Event> GetEventsForSeveralDaysParrallel()
        {
            var countDays = (EndDate - StartDate).Days + 1;
            var result = new List<Event>[countDays];
            try
            {
                return Parallel.For(0, countDays, i => result[i] = GetEventsForOneDay(StartDate.AddDays(i))).IsCompleted 
                    ? result.SelectMany(x => x).ToList() 
                    : new List<Event>();
            }
            catch (AggregateException ae)
            {
                foreach (WebException e in ae.InnerExceptions.OfType<WebException>())
                {
                    throw e;
                }
                return new List<Event>();
            }
        }

        private List<Event> GetEventsForOneDay(DateTime date)
        {
            var prefix = date.ToString("/yyyy/MM/dd", CultureInfo.InvariantCulture);
            try
            {
                var content = GetRequest(Url + prefix);
                if (content == String.Empty) return null;
                var htmlString = Regex.Split(content, @"<!-- \d{4}-\d{2}-\d{2} -->")[1];
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlString);
                var timeList = doc.DocumentNode.SelectNodes("//td[@valign='middle']").Where(n => n.ChildNodes[0].InnerText == "Начало")
                    .Select(n =>DateTime.Parse(Regex.Replace(Regex.Match(n.ChildNodes[1].InnerText, @"[\d]{2}[^\d][\d]{2}").Value,@"[^\d]", ":"))).ToList();
                var nameList = doc.DocumentNode.SelectNodes("//b[@class='big_orange']").Select(n => n.InnerText).ToList();
                var addressList = doc.DocumentNode.SelectNodes("//span[@class='small']").Select(n => n.ChildNodes[0].InnerText.Trim().TrimEnd(',')).ToList();
                var placeList = doc.DocumentNode.SelectNodes("//td[@bgcolor='#F4F4F4']").Select(n => n.ChildNodes[0].ChildNodes[1].InnerText.Trim()).ToList();
                return timeList.Select((t, i) => new Event(date.Add(t.TimeOfDay), placeList[i], addressList[i], nameList[i])).ToList();
            }
            catch (WebException)
            {
                throw;
            }
            catch (Exception)
            {
                return new List<Event>();
            }
        }

        private static string GetRequest(string url)
        {
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
            httpWebRequest.AllowAutoRedirect = false;
            try
            {
                using (var httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse())
                {
                    using (var stream = httpWebResponse.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream, Encoding.GetEncoding(httpWebResponse.CharacterSet)))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException)
            {
                throw;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}