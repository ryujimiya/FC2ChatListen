using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net; // IpAddress
using System.Text.RegularExpressions; // Regex

namespace FC2ChatListen
{
    /// <summary>
    /// コメント受信デリゲート
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="userName"></param>
    /// <param name="cmntStr"></param>
    public delegate void RecvCmntDelegeate(object sender, string userName, string cmntStr);

    /// <summary>
    /// FC2ライブチャットのコメントを受信するサーバー
    /// </summary>
    public class ChatServer
    {
        /// <summary>
        /// IPアドレス getter/setter
        /// </summary>
        public string IpAddrStr
        {
            get;
            set;
        }

        /// <summary>
        /// ポート番号 getter/setter
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// コメント受信イベント
        /// </summary>
        public event RecvCmntDelegeate RecvCmnt = null;

        /// <summary>
        /// リッスンスレッド
        /// </summary>
        private Thread _ListenThread;
        /// <summary>
        /// 終了中？
        /// </summary>
        private bool _IsTerminating;
        /// <summary>
        /// Tcpリスナー
        /// </summary>
        private TcpListener _Listener = null;
        /// <summary>
        /// Tcpクライアント
        /// </summary>
        private TcpClient _Client = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ChatServer()
        {
            IpAddrStr = "127.0.0.1";
            Port = 8888;
        }

        /// <summary>
        /// リッスンを開始する
        /// </summary>
        public void StartListen()
        {
            System.Diagnostics.Debug.WriteLine("ChatServer::StartListen");
            _IsTerminating = false;
            _ListenThread = new Thread(new ThreadStart(ThreadProc));
            _ListenThread.Start();
            System.Diagnostics.Debug.WriteLine("ChatServer::StartListen end");
        }

        /// <summary>
        /// リッスンを終了する
        /// </summary>
        public void StopListen()
        {
            System.Diagnostics.Debug.WriteLine("ChatServer::StopListen");
            // 終了処理開始
            _IsTerminating = true;
            // Tcpクライアントを閉じる
            if (_Client != null)
            {
                _Client.Close();
            }
            // リッスンを終了する
            _Listener.Stop();
            //_ListenThread.Abort();
            _ListenThread.Join();
            System.Diagnostics.Debug.WriteLine("ChatServer::StopListen end");
        }

        private void ThreadProc()
        {
            System.Diagnostics.Debug.WriteLine("ChatServer::ThreadProc");
            // Ipアドレスに変換する
            IPAddress ipAddr = IPAddress.Parse(IpAddrStr);
            // TcpListnerを生成
            TcpListener listener = new TcpListener(ipAddr, Port);
            // リスナーを制御できるようにフィールドに格納する
            _Listener = listener;
            // リッスン開始
            listener.Start();

            System.Diagnostics.Debug.WriteLine("ThreadProc listen done");
            // リッスン中の処理(クライアントをacceptする)
            while (!_IsTerminating)
            {
                TcpClient client = null;
                // クライアントフィールドをクリア
                _Client = null;
                try
                {
                    // クライアントを受け入れる
                    //  Note:クライアントは１つだけ受け入れる
                    client = listener.AcceptTcpClient();
                    System.Diagnostics.Debug.WriteLine(
                        "ThreadProc accepted client:({0}:{1})",
                        ((IPEndPoint)client.Client.RemoteEndPoint).Address,
                        ((IPEndPoint)client.Client.RemoteEndPoint).Port
                        );
                    // クライアントフィールドに格納する
                    _Client = client;
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine("listener.AcceptTcpClient exception");
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                    // クライアントをクローズしたとき例外が発生する
                    if (_IsTerminating)
                    {
                        // 終了処理中なら抜ける
                        break;
                    }
                }

                // ネットワークストリームを取得
                NetworkStream ns = client.GetStream();
                // 読み込みタイムアウト10秒
                //ns.ReadTimeout = 10 * 1000;
                // 読み込みタイムアウトは無限
                //ns.ReadTimeout = Timeout.Infinite;

                // 切断された？
                bool isDisconneced = false;

                ////////////////////////
                // コメント受信処理
                ////////////////////////
                // 文字コード
                Encoding enc = Encoding.UTF8;
                //// 受信電文バッファ(1文字ずつ読み込む)
                //byte[] resBytes = new byte[1];
                // 受信電文バッファ 一気に読み込む
                byte[] resBytes = new byte[1024];
                // 受信電文サイズ
                int resSize = 0;

                System.Diagnostics.Debug.WriteLine("ThreadProc waiting for comment msg");
                // クライアントが接続されている間の処理
                while (!_IsTerminating
                        && !isDisconneced
                        /*&& ns.DataAvailable*/)
                {
                    // メモリーストリーム
                    var ms = new System.IO.MemoryStream();

                    while (!_IsTerminating
                            && !isDisconneced
                            /*&& ns.DataAvailable*/)
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine("ThreadProc read start");
                            // 電文を受信する
                            resSize = ns.Read(resBytes, 0, resBytes.Length);
                            System.Diagnostics.Debug.WriteLine("ThreadProc read done");
                        }
                        catch (Exception exception)
                        {
                            System.Diagnostics.Debug.WriteLine("ns.Read exception");
                            System.Diagnostics.Debug.WriteLine(exception.Message);
                            // リスナーをクローズしたとき例外が発生する
                            if (_IsTerminating)
                            {
                                // 終了処理中なら抜ける
                                break;
                            }
                        }
                        if (resSize == 0)
                        {
                            // 切断された
                            System.Diagnostics.Debug.WriteLine("socket is closed by client");
                            isDisconneced = true;
                            break;
                        }
                        System.Diagnostics.Debug.WriteLine("ThreadProc save to buffer");
                        // メモリーストリームに受信電文を蓄積する
                        ms.Write(resBytes, 0, resSize);

                        if (resBytes[resSize - 1] == 0)
                        {
                            // ソケットポリシーファイルリクエスト
                            break;
                        }
                        if (resBytes[resSize - 1] == '}')
                        {
                            // コメント受信完了

                            break;
                        }
                    }

                    if (isDisconneced)
                    {
                        System.Diagnostics.Debug.WriteLine("ThreadProc isDisconnected");
                        // 切断された場合
                        // 後片付け
                        ms.Close();
                        ms.Dispose();
                        break;
                    }

                    System.Diagnostics.Debug.WriteLine("ThreadProc receive msg");
                    // 受信した電文を文字列に変換
                    string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                    System.Diagnostics.Debug.WriteLine("Received:" + resMsg);
                    System.Diagnostics.Debug.WriteLine(" ");
                    ////////////////////////
                    // 電文解析
                    ////////////////////////
                    if (resMsg.IndexOf("<policy-file-request/>") >= 0)
                    {
                        // ソケットポリシーファイルの要求の場合
                        // 後片付け
                        ms.Close();
                        ms.Dispose();
                        // ソケットポリシーファイルの内容
                        string strToSend =
                            "<?xml version=\"1.0\"?>" 
                            + "<!DOCTYPE cross-domain-policy SYSTEM \"http://www.adobe.com/xml/dtds/cross-domain-policy.dtd\">"
                            + "<cross-domain-policy>"
                            + "<allow-access-from domain=\"*\" to-ports=\"" + Port.ToString() + "\" />"
                            + "</cross-domain-policy>";
                        // 送信する電文をバイト列に変換
                        byte[] bytesToSend = enc.GetBytes(strToSend);
                        // 送信する
                        ns.Write(bytesToSend, 0, bytesToSend.Length);
                        System.Diagnostics.Debug.WriteLine("Send:" + strToSend);

                        // クライアント接続を閉じる(必須）
                        client.Close();
                        isDisconneced = true;
                        break;

                    }
                    else
                    {
                        // コメント電文を解析する
                        string userName = "";
                        string cmntStr = "";
                        parseMsg(resMsg, ref userName, ref cmntStr);
                        if (userName.Length > 0)
                        {
                            // コメント受信イベントを発生させる
                            if (this.RecvCmnt != null)
                            {
                                this.RecvCmnt(this, userName, cmntStr);
                            }
                        }
                    }
                    // 後片付け
                    ms.Close();
                    ms.Dispose();
                    System.Diagnostics.Debug.WriteLine("ThreadProc receive msg done");
                }
                client.Close();
                System.Diagnostics.Debug.WriteLine("ThreadProc client close done");
            }
            // リッスンを終了する
            listener.Stop();
            System.Diagnostics.Debug.WriteLine("ThreadProc stop listen done");

            // リスナーのフィールドをクリア
            _Listener = null;
        }

        /// <summary>
        /// 電文を解析する
        /// </summary>
        /// <param name="resMsg"></param>
        /// <param name="userName"></param>
        /// <param name="cmntStr"></param>
        private void parseMsg(
            string resMsg,
            ref string userName,
            ref string cmntStr
            )
        {
            System.Diagnostics.Debug.WriteLine("parseMsg resMsg=" + resMsg);
            // 初期化
            userName = "";
            cmntStr = "";

            // resMsgには
            //  {"channel":"52305612","time":"1428450414967","username":"Something Else","comment":"コメントを送る","ng_flg":"0"}
            //DEBUG
            //resMsg = "{\"channel\":\"52305612\",\"time\":\"1428450414967\",\"username\":\"Something Else\",\"comment\":\"コメントを送る\",\"ng_flg\":\"0\"}";

            /*
            // ユーザー名、コメント
            //DEBUG これはＯＫ
            //MatchCollection matches = Regex.Matches(resMsg, "\"username\":\"(.+)\"");
            MatchCollection matches = Regex.Matches(resMsg, "\"username\":\"(.+)\",\"comment\":\"(.+)\",\"ng_flg\":");
            if (matches.Count > 0)
            {
                // ユーザー名
                // $1の箇所を取得
                userName = matches[0].Groups[1].Value;
                System.Diagnostics.Debug.WriteLine("userName:" + userName);
                // コメント
                // $1の箇所を取得
                cmntStr = matches[0].Groups[2].Value;
                System.Diagnostics.Debug.WriteLine("cmntStr:" + cmntStr);
            }
            */
            //"comment":"だからこのタイトルの付け方だと2部なのか3部なのかわかんねーんだよ","time":"1428470404146","ng_flg":"0","channel":"45362084","username":"匿名"}
            // ユーザー名
            MatchCollection matchesUserName = Regex.Matches(resMsg, "\"username\":\"([^,}]+)\"[,}]+");
            if (matchesUserName.Count > 0)
            {
                // ユーザー名
                // $1の箇所を取得
                userName = matchesUserName[0].Groups[1].Value;
                System.Diagnostics.Debug.WriteLine("userName:" + userName);
            }
            // コメント
            MatchCollection matchesComment = Regex.Matches(resMsg, "\"comment\":\"([^,}]+)\"[,}]+");
            if (matchesComment.Count > 0)
            {
                // コメント
                // $1の箇所を取得
                cmntStr = matchesComment[0].Groups[1].Value;
                System.Diagnostics.Debug.WriteLine("cmntStr:" + cmntStr);
            }
            // DEBUG
            //userName = "TEST";
            //cmntStr = resMsg;
        }


    }
}
