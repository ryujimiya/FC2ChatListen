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
using System.Windows.Shapes;

namespace FC2ChatListen
{
    /// <summary>
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ウィンドウが表示された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            // Guiへデータをセットする
            setDataToGui();
        }

        /// <summary>
        /// ウィンドウが閉じられようとしている
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Guiからデータを取得する
            getDataFromGui();
            // 設定を保存する
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Guiにデータをセットする
        /// </summary>
        private void setDataToGui()
        {
            int intVal;

            // 最前面に表示する？
            bool isTopMost = false;
            intVal = Properties.Settings.Default.TopMost;
            isTopMost = (intVal == 1);
            TopMostCheckBox.IsChecked = isTopMost;

            // カレントディレクトリの表示
            CurDirTextBox.Text = Environment.CurrentDirectory;
        }

        /// <summary>
        /// Guiからデータを取得する
        /// </summary>
        private void getDataFromGui()
        {
            int intVal;

            // 最前面に表示する？
            bool? isTopMost;
            isTopMost = TopMostCheckBox.IsChecked;
            intVal = ((isTopMost == true)? 1 : 0);
            Properties.Settings.Default.TopMost = intVal;
        }


    }
}
