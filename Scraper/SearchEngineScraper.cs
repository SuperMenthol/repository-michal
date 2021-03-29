using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Personal_Newspaper
{
    public struct SearchInformation
    {
        string keywordString;
        int startAt;
        int endAt;
        string searchDomain;
        string searchRegion;
        public string KeywordString { get { return keywordString; } }
        public int StartAt { get { return startAt; } }
        public int EndAt { get { return endAt; } }
        public string SearchDomain { get { return searchDomain; } }
        public string SearchRegion { get { return searchRegion; } }

        public SearchInformation(string keywords, int startRecord = 0, int endRecord = 0, string domain = "", string region = "")
        {
            keywordString = keywords;
            startAt = startRecord;
            endAt = endRecord;
            searchDomain = domain;
            searchRegion = region;
        }
    }

    public abstract class SearchEngineScraper : IScraper, ILinkGenerator, ITechnical, IFileGenerator
    {
        protected SearchInformation srcInfo;
        protected string siteUrl;
        public virtual string SiteUrl
        {
            get { return siteUrl; }
            set { if (value.StartsWith("http")) { siteUrl = value; } else { siteUrl = null; } }
        }
        public virtual async void Scrape() { }
        public abstract string GenerateKeywords(string inputString);
        public abstract string GenerateAddress();
        public abstract void GetLinks(HtmlDocument sourceDocument);
    }

    public class Google : SearchEngineScraper
    {
        public override string SiteUrl
        {
            get { return siteUrl; }
            set { if (value.StartsWith("https://") && value.Contains("google")) { siteUrl = value; } else { siteUrl = null; } }
        }

        public Google(List<object> modifierString)
        {
            srcInfo = new SearchInformation((string)modifierString[0],(int)modifierString[1],(int)modifierString[2]);
            SiteUrl = GenerateAddress();
        }

        public override string GenerateAddress()
        {
            string retString = string.Empty;
            retString += "https://www.google.com/search?q=" + GenerateKeywords(srcInfo.KeywordString);
            if (srcInfo.SearchDomain != "") { retString += string.Format("&sitesearch={0}.", srcInfo.SearchDomain); }
            if (srcInfo.SearchRegion != "") { retString += string.Format("&lr=lang_{0}", srcInfo.SearchRegion); }
            //if (srcInfo.StartAt > 0) { retString += StartClause(srcInfo.StartAt); }

            return retString;
        }

        public override string GenerateKeywords(string inputString) { return inputString; }

        public override async void Scrape()
        {
            int recordStartIndex = ITechnical.RecalculateRecordLimiter(srcInfo.StartAt);
            int recordEndIndex = ITechnical.RecalculateRecordLimiter(srcInfo.EndAt);

            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage responseMessage = await httpClient.GetAsync(siteUrl); //throws exception here on incorrect connection, handle it here
            string fPath = AppDomain.CurrentDomain.BaseDirectory + ITechnical.GenerateFileName("google");

            using (StreamWriter sw = new StreamWriter(fPath))
            {
                while (recordStartIndex <= recordEndIndex)
                {
                    SiteUrl += StartClause(recordStartIndex);
                    responseMessage = await httpClient.GetAsync(SiteUrl);

                    await sw.WriteLineAsync(responseMessage.Content.ReadAsStringAsync().Result);
                    cancellationToken.Token.ThrowIfCancellationRequested();
                    ITechnical.ResultTextPrompt("Page downloaded to file");

                    recordStartIndex += 10;
                }
            }

            HtmlDocument htmlDoc = IFileGenerator.GenerateHtmlDocument(fPath).Result;
            GetLinks(htmlDoc);
            IFileGenerator.DeleteHtmlDocument(fPath);
        }

        public override void GetLinks(HtmlDocument sourceDocument)
        {
            var hNodes = sourceDocument.DocumentNode.SelectNodes("//a");
            List<string> titleList = new List<string>();
            List<string> linkList = new List<string>();

            foreach (var item in hNodes)
            {
                if (item.Attributes["href"].Value.Contains("url"))
                {
                    string link = item.Attributes["href"].Value.Substring(item.Attributes["href"].Value.IndexOf("=") + 1, item.Attributes["href"].Value.IndexOf("&amp") - 7);

                    if (!linkList.Contains(link) && linkList.Where(x => x.Contains(link.Split(".")[1])).Count() == 0 && item.ChildNodes[0].InnerText.Length > 0)
                    {
                        titleList.Add(item.ChildNodes[0].InnerText);
                        linkList.Add(link);
                    }
                }
            }
            //var a = (MainWindow)Application.Current.MainWindow;
            //a.ChangeResultBox(linkList);
            IFileGenerator.GenerateXmlDocument("google", titleList, linkList);
        }

        string StartClause(int startIndex) { return string.Format("&start={0}", startIndex.ToString()); }
    }

    public class Qwant : SearchEngineScraper
    {
        public override string SiteUrl
        {
            get { return siteUrl; }
            set { if (value.StartsWith("https://") && value.Contains("qwant")) { siteUrl = value; } else { siteUrl = null; } }
        }

        public Qwant(List<object> modifierString)
        {
            srcInfo = new SearchInformation((string)modifierString[0]);
            SiteUrl = GenerateAddress();
        }

        public override string GenerateAddress()
        {
            string retString = string.Empty;
            retString += "https://www.qwant.com/?q=" + GenerateKeywords(srcInfo.KeywordString) + "&t=web";
            if (srcInfo.SearchRegion != "") { retString += string.Format("&r={0}", srcInfo.SearchRegion); }

            return retString;
        }

        public override string GenerateKeywords(string inputString) { return inputString; }

        public override async void Scrape()
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage responseMessage = await httpClient.GetAsync(siteUrl); //throws exception here on incorrect connection, handle it here
            string fPath = AppDomain.CurrentDomain.BaseDirectory + ITechnical.GenerateFileName("qwant");

            using (StreamWriter sw = new StreamWriter(fPath))
            {
                responseMessage = await httpClient.GetAsync(SiteUrl);

                await sw.WriteLineAsync(responseMessage.Content.ReadAsStringAsync().Result);
                cancellationToken.Token.ThrowIfCancellationRequested();
                ITechnical.ResultTextPrompt("Page downloaded to file");
            }

            HtmlDocument htmlDoc = IFileGenerator.GenerateHtmlDocument(fPath).Result;
            GetLinks(htmlDoc);
        }

        public override void GetLinks(HtmlDocument sourceDocument)
        {
            var hNodes = sourceDocument.DocumentNode.SelectNodes("//a");
            List<string> titleList = new List<string>();
            List<string> linkList = new List<string>();

            foreach (var item in hNodes)
            {
                if (item.Attributes["href"].Value.Contains("url"))
                {
                    string link = item.Attributes["href"].Value.Substring(item.Attributes["href"].Value.IndexOf("=") + 1, item.Attributes["href"].Value.IndexOf("&amp") - 7);

                    if (!linkList.Contains(link) && linkList.Where(x => x.Contains(link.Split(".")[1])).Count() == 0 && item.ChildNodes[0].InnerText.Length > 0)
                    {
                        titleList.Add(item.ChildNodes[0].InnerText);
                        linkList.Add(link);
                    }
                }
            }
            var a = (MainWindow)Application.Current.MainWindow;
            a.ChangeResultBox(linkList);
        }

        string StartClause(int startIndex) { return string.Format("&start={0}", startIndex.ToString()); }
    }
}
