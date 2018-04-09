using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FC2ChatListen
{
    /// <summary>
    /// 表示するコメントデータクラス
    /// </summary>
    public class CmntData : INotifyPropertyChanged
    {
        /// <summary>
        /// ハンドル名 getter/setter
        /// </summary>
        public string HandleName
        {
            get { return _HandleName; }
            set { _HandleName = value; }
        }

        /// <summary>
        /// コメント文字列 getter/setter
        /// </summary>
        public string CmntStr
        {
            get { return _CmntStr; }
            set { _CmntStr = value; }
        }

        /// <summary>
        /// INotifyPropertyChangedインターフェースの実装
        /// </summary>
        /// <param name="pName"></param>
        protected void OnPropertyChanged(string pName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(pName));
            }
        }

        /// <summary>
        /// ハンドル名
        /// </summary>
        private string _HandleName;
        /// <summary>
        /// コメント文字列
        /// </summary>
        private string _CmntStr;
        /// <summary>
        /// INotifyPropertyChangedイベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 表示するコメントデータのコレクション
    /// </summary>
    public class CmntDataCollection : ObservableCollection<CmntData>
    {
        public CmntDataCollection()
        {
            // 初期データ(DEBUG)
            //this.Add(new CmntData() { HandleName = "Somthing Else", CmntStr = "こんにちは" });
            //this.Add(new CmntData() { HandleName = "Somthing Else", CmntStr = "FC2のコメビューをつくってます" });
        }
    }
}
