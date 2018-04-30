using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FC2ChatListen
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            // コンポーネントの初期化
            InitializeComponent();
        }

        /// <summary>
        /// ウィンドウが表示された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            // バージョンをタイトルに追加
            this.Title = this.Title + " ver " + MyUtilLib.MyUtil.getAppVersion();

            // ウィンドウを最前面に表示する/しない
            showTopMostOrNot();

            // データグリッドにデータコレクションをセットする
            this.CmntDG.DataContext = _dataCollection;

            // チャットサーバーのコメント受信イベントハンドラを登録する
            this._chatServer.RecvCmnt += _chatServer_RecvCmnt;

            // リッスン開始
            this._chatServer.StartListen();

            // 背景画像を指定する
            setBackgroundImage();

            // コメントファイル初期化（削除）
            removeCmntFile();

            // 棒読みちゃん開始
            startBouyomi();
        }

        /// <summary>
        /// コメントを受信した
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="userName"></param>
        /// <param name="cmntStr"></param>
        private void _chatServer_RecvCmnt(object sender, string userName, string cmntStr)
        {
            Dispatcher.Invoke(new Action(
                () =>
                {
                    // DataGridにデータバインディングしているデータコレクションにコメントデータを追加する
                    _dataCollection.Add(new CmntData { HandleName = userName, CmntStr = cmntStr });

                    // Daragrid のスクロールビューを取得し、
                    // 最下行にスクロールする
                    CmntDGScrollToEnd();

                    // 棒読みちゃんに話させる
                    bouyomiTalk(cmntStr);

                    // コメントをファイルに追記する
                    writeCmntToFile(userName, cmntStr);

                    if (userName != "URL")
                    {
                        // コメント中のURLのタイトルリクエスト開始
                        checkUrlInComment(cmntStr);
                    }

                }
                ));

        }

        /// <summary>
        /// 閉じられようとしている
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 棒読みちゃん停止
            stopBouyomi();

            // チャットサーバーのリッスンを終了する
            _chatServer.StopListen();
        }

        /// <summary>
        /// 背景画像を指定する
        /// </summary>
        private void setBackgroundImage()
        {
            // カレントディレクトリを取得
            string curDir = System.Environment.CurrentDirectory;

            // BitmapImageにファイルから画像を読み込む
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(curDir + @"\background.jpg");
            bitmap.EndInit();
            
            // BitmapImageをImageコントロールにセット
            this.BackgroundImage.Source = bitmap;
        }

        /// <summary>
        /// Daragrid のスクロールビューを取得し、
        /// 最下行にスクロールする
        /// </summary>
        private void CmntDGScrollToEnd()
        {
            if (this.CmntDG.Items.Count > 0)
            {
                // Daragrid のスクロールビューを取得し、
                // 最下行にスクロールする
                var border = VisualTreeHelper.GetChild(this.CmntDG, 0) as Decorator;
                if (border != null)
                {
                    // Daragrid のスクロールビューを取得する
                    var scroll = border.Child as ScrollViewer;
                    if (scroll != null)
                    {
                        scroll.ScrollToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// 設定ボタンがクリックされた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            // 設定ウィンドウ
            var settingsWindow = new SettingsWindow();
            // オーナーをセット
            settingsWindow.Owner = this;
            // モーダルダイアログとしてウィンドウをオープンする
            bool? result = settingsWindow.ShowDialog();
            /* 常に変更を保存するのでこの処理は削除
            if (result == true)
            {
                // アクティビティが受け入れられたとき
            }
            else if (result == false)
            {
                // アクティビティが取り消されたとき
            }
            else
            {
                // その他(ここにくることあるの？）
            }
            */

            // ウィンドウを最前面に表示する/しない
            showTopMostOrNot();
        }

        /// <summary>
        /// ウィンドウを最前面に表示する/しない
        /// </summary>
        private void showTopMostOrNot()
        {
            int intVal;

            // 設定ファイルの設定を読み込む
            // 最前面に表示する？
            bool isTopMost = false;
            intVal = Properties.Settings.Default.TopMost;
            isTopMost = (intVal == 1);

            if (isTopMost)
            {
                this.Topmost = true;
            }
            else
            {
                this.Topmost = false;
            }
        }

        /// <summary>
        /// 棒読みちゃん開始
        /// </summary>
        private void startBouyomi()
        {
            _Bouyomi = new FNF.Utility.BouyomiChanClient();
        }

        /// <summary>
        /// 棒読みちゃん停止
        /// </summary>
        private void stopBouyomi()
        {
            if (_Bouyomi == null)
            {
                return;
            }
            _Bouyomi.ClearTalkTasks();
            _Bouyomi.Dispose();
            _Bouyomi = null;
        }

        /// <summary>
        /// 棒読みちゃんに話させる
        /// </summary>
        /// <param name="cmntStr"></param>
        private void bouyomiTalk(string cmntStr)
        {
            if (_Bouyomi == null)
            {
                return;
            }
            try
            {
                _Bouyomi.AddTalkTask(cmntStr, -1, -1, FNF.Utility.VoiceType.Default);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
            }
        }

        /// <summary>
        /// コメントファイルパスを取得する
        /// </summary>
        /// <returns></returns>
        private string getCmntFilePath()
        {
            string dir = Environment.CurrentDirectory;
            string filePath = dir + "\\comment.txt";
            return filePath;
        }

        /// <summary>
        /// コメントファイルを削除する
        /// </summary>
        private void removeCmntFile()
        {
            string filePath = getCmntFilePath();
            System.IO.File.Delete(filePath);
        }

        /// <summary>
        /// コメントをファイルに追記する
        /// </summary>
        private void writeCmntToFile(string userName, string cmntStr)
        {
            string filePath = getCmntFilePath();
            Encoding enc = Encoding.GetEncoding("UTF-8");
            
            // 追加書き込みする
            bool append = true;
            var writer = new System.IO.StreamWriter(filePath, append, enc);
            writer.WriteLine(userName + "\t" + cmntStr);
            writer.Close();

        }

        /// <summary>
        /// コメント中にあるURLのタイトルリクエストを開始する
        /// </summary>
        /// <param name="cmntStr"></param>
        private void checkUrlInComment(string cmntStr)
        {
            //TEST
            //string cmntStr = "URLです http://www.yahoo.co.jp/\r\n正規表現";
            //string cmntStr = "URLです http://yahoo.co.jp/\r\n正規表現";
            // コメント中にURLがあるかチェックする
            string url = GetTitleRequest.GetUrlInComment(cmntStr);
            if (url.Length == 0)
            {
                return;
            }
            //System.Diagnostics.Debug.WriteLine("url:" + url);
            
            // タイトルリクエスト
            var getTitleReq = new GetTitleRequest(url);
            getTitleReq.OnTitleParsed += getTitleReq_OnTitleParsed;
            getTitleReq.StartReq();
        }

        /// <summary>
        /// Webページのタイトル解析が完了した
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="url"></param>
        /// <param name="title"></param>
        private void getTitleReq_OnTitleParsed(object sender, string url, string title)
        {
            string userName = "URL";
            string cmntStr =
                url
                + System.Environment.NewLine
                + title;
            System.Diagnostics.Debug.WriteLine("getTitleReq_OnTitleParsed: cmntStr=" + cmntStr);
            // コメント受信ハンドラを呼び出す
            _chatServer_RecvCmnt(null, userName, cmntStr);
        }


        /// <summary>
        /// コメントデータのコレクション
        /// </summary>
        private CmntDataCollection _dataCollection = new CmntDataCollection();
        /// <summary>
        /// FC2チャットサーバー
        /// </summary>
        private ChatServer _chatServer = new ChatServer();
        /// <summary>
        /// 棒読みちゃんクライアント
        /// </summary>
        private FNF.Utility.BouyomiChanClient _Bouyomi = null;

    }
}
