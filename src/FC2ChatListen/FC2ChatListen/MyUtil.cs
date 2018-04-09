using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices; //DllImport
using System.Collections; //Hashtable
using System.Text.RegularExpressions; // Regex
using System.Drawing; // Color

namespace MyUtilLib
{
    class MyUtil
    {
        /// <summary>
        /// コマンドライン引数解析  
        ///  /key0 /Key1 Value1 /Key2 Value2 Value3
        ///    key0の値はnull
        ///    key1の値はValue1
        /// </summary>
        /// <param name="keyPrefix">(初期値: '/')</param>
        /// <returns>コマンドライン情報</returns>
        public class CmdLineInfo
        {
            public string[] Args = null;                   // コマンドライン引数配列 System.Environment.GetCommandLineArgs()を格納
            public string AppPath = "";                    // コマンドライン引数の先頭パラメータ(アプリケーションパス)
            public Hashtable ParamHash = new Hashtable();  // key: パラメータキー value:パラメータ値
            public string[] ParamValues = null;             // k
        }
        public static CmdLineInfo GetCmdLineInfo(char keyPrefix = '/')
        {
            CmdLineInfo cmdLineInfo = new CmdLineInfo();

            //コマンドラインを配列で取得する
            string[] args = System.Environment.GetCommandLineArgs();
            cmdLineInfo.Args = args;

            // アプリケーションパス
            cmdLineInfo.AppPath = args[0];
            // 引数解析
            if (args.Length > 1)
            {
                Hashtable paramHash = cmdLineInfo.ParamHash;
                List<string> paramValueList = new List<string>();
                string paramKey = "";
                string paramValue = "";
                for (int i = 1; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (arg == null || arg.Length == 0)
                    {
                        continue;
                    }
                    if (arg[0] == keyPrefix)
                    {
                        // キー
                        paramKey = arg;
                        paramHash[paramKey] = null;
                    }
                    else
                    {
                        // 値
                        paramValue = arg;
                        if (paramKey != "")
                        {
                            // キー付きパラメータ値
                            paramHash[paramKey] = paramValue;
                            paramKey = "";
                        }
                        else
                        {
                            // キーなしパラメータ値
                            paramValueList.Add(paramValue);
                        }
                    }
                }
                if (paramValueList.Count > 0)
                {
                    cmdLineInfo.ParamValues = paramValueList.ToArray();
                }
            }
            return cmdLineInfo;
        }

        /// <summary>
        /// バージョン名を取得
        /// </summary>
        /// <returns></returns>
        public static string getAppVersion()
        {
            string versionStr = "";
            /*
            //（AssemblyInformationalVersion属性）
            versionStr = Application.ProductVersion;
            */

            // Wpf版
            //自分自身のAssemblyを取得
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            //バージョンの取得
            System.Version ver = asm.GetName().Version;

            versionStr = ver.ToString();
            return versionStr;
        }

        /*
        /// <summary>
        /// 製品名（AssemblyProduct属性）を取得
        /// </summary>
        /// <returns></returns>
        public static string getAppProductName()
        {
            return Application.ProductName;
        }

        /// <summary>
        /// 会社名（AssemblyCompany属性）を取得
        /// </summary>
        /// <returns></returns>
        public static string getAppCompanyName()
        {
            return Application.CompanyName;
        }
        */

    }

}
