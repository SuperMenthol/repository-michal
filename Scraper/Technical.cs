using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Personal_Newspaper
{
    interface ITechnical
    {
        public static string GenerateFileName(string website)
        {
            return String.Format("{0}{1}{2}-{3}.xml", new object[4] { DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year, website });
        }

        protected static void ResultTextPrompt(string prompt) { System.Windows.MessageBox.Show(prompt, "Result", System.Windows.MessageBoxButton.OK); }
        protected static int RecalculateRecordLimiter(int inputNumber) { return (inputNumber - 1) * 10; }
    }

    class Technical
    {
        StreamReader streamUsed;
        public StreamReader StreamUsed { get { return streamUsed; } set { streamUsed = value; } }

        public async Task<bool> ValidateUrl(string inputUrl)
        {
            try
            {
                HttpResponseMessage responseMessage = await new HttpClient().GetAsync(inputUrl);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public int TotalStreamLines()
        {
            int i = 0;
            while (streamUsed.ReadLine() != null) { i++; }
            return i;
        }

        public List<string> GetAllLinks()
        {
            int lines = TotalStreamLines();
            streamUsed.BaseStream.Position = 0;

            List<string> linkArray = new List<string>();

            //for (int i = 0; i < lines; i++)
            while (!streamUsed.EndOfStream)
            {
                if (streamUsed.ReadLine().Contains("https:"))
                {
                    linkArray.Add(streamUsed.ReadLine().Substring(streamUsed.ReadLine().IndexOf("https:"), 5));
                }
            }

            return linkArray;
        }
    }
}
