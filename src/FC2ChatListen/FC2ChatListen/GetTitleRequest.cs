using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;

namespace FC2ChatListen
{
    delegate void OnTitleParsedDelegate(object sender, string url, string title);
    /// <summary>
    /// タイトル取得リクエストクラス
    /// </summary>
    class GetTitleRequest
    {
        /// <summary>
        /// Url
        /// </summary>
        public string Url
        {
            get;
            private set;
        }

        /*
        /// <summary>
        /// 本当のURL
        /// </summary>
        public string RealUrl
        {
            get;
            private set;
        }
        */

        /// <summary>
        /// タイトル
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        public event OnTitleParsedDelegate OnTitleParsed;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="url"></param>
        public GetTitleRequest(string url)
        {
            this.Url = url;
            //this.RealUrl = this.Url;
        }

        /// <summary>
        /// コメント中のURLを取得する
        /// </summary>
        /// <param name="cmntStr"></param>
        /// <returns></returns>
        public static string GetUrlInComment(string cmntStr)
        {
            string url = "";
            var regEx = new Regex("http[s]?://[^ \r\n]+", RegexOptions.Singleline);
            MatchCollection matchs = regEx.Matches(cmntStr);
            if (matchs.Count > 0)
            {
                url = matchs[0].Value;
            }
            return url;
        }

        /// <summary>
        /// タスクを実行する
        /// </summary>
        public void StartReq()
        {
            doHttpRequest(this.Url);
        }

        /// <summary>
        /// タイトルを解析して取得する
        /// </summary>
        /// <param name="resultStr"></param>
        /// <returns></returns>
        public string parseTitle(string resultStr)
        {
            string title = "";

            var regEx = new Regex("<title>([^>]+)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection matches = regEx.Matches(resultStr);
            if (matches.Count > 0)
            {
                title = matches[0].Groups[1].Value;
            }
            return title;
        }

        /// <summary>
        /// FC2中間ページを解析してURLを取得する
        /// </summary>
        /// <param name="resultStr"></param>
        /// <returns></returns>
        public string parseUrlFromFC2TransPage(string resultStr)
        {
            string url = "";

            var bRegEx = new Regex("<b>((?:(?!</b>).)*)</b>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection bMatches = bRegEx.Matches(resultStr);
            if (bMatches.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("bMatches.Count:" + bMatches.Count);
                string bTagStr = bMatches[0].Groups[1].Value;
                System.Diagnostics.Debug.WriteLine("bTagStr:" + bTagStr);
                var hrefRegEx = new Regex("href=\"([^\"]+)\"");
                MatchCollection hrefMatches = hrefRegEx.Matches(bTagStr);
                if (hrefMatches.Count > 0)
                {
                    url = hrefMatches[0].Groups[1].Value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("parseUrlFromFC2TransPage aTag not found.");
                }

            }
            else
            {
                System.Diagnostics.Debug.WriteLine("parseUrlFromFC2TransPage bTag not found.");
            }

            System.Diagnostics.Debug.WriteLine("parseUrlFromFC2TransPage url:" + url);
            return url;
        }

        /// <summary>
        /// HTTPリクエスト
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private void doHttpRequest(string url)
        {
            WebClient client = new WebClient();
            System.Diagnostics.Debug.WriteLine("doHttpRequest url:" + url);
            try
            {
                client.Encoding = Encoding.UTF8;
                client.DownloadStringCompleted += client_DownloadStringCompleted;
                client.DownloadStringAsync(new Uri(url));
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                System.Diagnostics.Debug.WriteLine(exception.StackTrace);
            }
        }

        /// <summary>
        /// 文字列のダウンロードが完了した
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string resultStr = e.Result;
            //System.Diagnostics.Debug.WriteLine(resultStr);

            string title = parseTitle(resultStr);

            if (title.IndexOf("別のサイトにジャンプしようとしています。") == 0)
            {
                // FC2ライブの中間ページのとき
                string url = parseUrlFromFC2TransPage(resultStr);

                // 本当のURLへリクエストする
                doHttpRequest(url);
                return;
            }

            this.Title = title;
            //System.Diagnostics.Debug.WriteLine("Title:" + this.Title);


            // タイトル解析完了通知イベントを送信する
            OnTitleParsed(this, this.Url, this.Title);
        }
    }
}
