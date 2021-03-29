using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Personal_Newspaper
{
    public interface IScraper
    {
        void Scrape();
    }

    public interface ILinkGenerator
    {
        abstract string GenerateAddress();
        abstract string GenerateKeywords(string inputKeywords);
    }

    public interface IFileGenerator
    {
        public static async Task<HtmlDocument> GenerateHtmlDocument(string path)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            using (StreamReader sr = new StreamReader(path)) { htmlDoc.LoadHtml(sr.ReadToEndAsync().Result); }
            return htmlDoc;
        }

        abstract void GetLinks(HtmlDocument sourceDocument);

        public static XmlDocument GenerateXmlDocument(string nUri, List<string> titleList, List<string> linkList)
        {
            var xDoc = new XmlDocument();
            XmlDeclaration xDeclaration = xDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlElement root = xDoc.DocumentElement;
            xDoc.InsertBefore(xDeclaration, root);
            var main = xDoc.CreateNode("element", "WebpageContainer", string.Empty);
            xDoc.AppendChild(main);

            for (int i = 0; i < linkList.Count; i++)
            {
                var newPage = xDoc.CreateNode(XmlNodeType.Element, "Webpage", nUri);
                main.AppendChild(newPage);
                XmlElement nLink = xDoc.CreateElement("element", "url");
                XmlElement nTitle = xDoc.CreateElement("element", "title");
                XmlElement nPage = xDoc.CreateElement("element", "website");
                newPage.AppendChild(nTitle);
                newPage.AppendChild(nPage);
                newPage.AppendChild(nLink);
                nLink.InnerText = linkList[i];
                nTitle.InnerText = titleList[i];

                string pageTitle = linkList[i].Substring(linkList[i].Length - (linkList[i].Length - linkList[i].IndexOf(".") - 1), linkList[i].Length - linkList[i].IndexOf(".")-1);
                pageTitle = pageTitle.Substring(0, pageTitle.IndexOf("/"));
                nPage.InnerText = pageTitle;
            }

            using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + nUri + ".xml",FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read)) { xDoc.Save(fs); }
            return xDoc;
        }

        public static void DeleteHtmlDocument(string filePath) { File.Delete(filePath); }
    }

    public interface ILogger
    {
        void TextErrorLog();
        void MessageBoxErrorLog();
        void EmailErrorLog();
    }
}
