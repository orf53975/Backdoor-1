using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Backdoor
{
    /// <summary>
    /// SelectPayload.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SelectPayload : Window
    {
        public SelectPayload()
        {
            InitializeComponent();
        }

        private void KeyLogger_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.PayloadPath = System.Windows.Forms.Application.StartupPath + @"\Keylog.exe";
            MainWindow.ClientPath = "KeyLogger";
            Close();
        }

        private void ShellOpener_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.PayloadPath = System.Windows.Forms.Application.StartupPath + @"\ShellOpener.exe";
            MainWindow.ClientPath = "ShellOpener";
            Close();
        }

        /// <summary>
        /// 사용자 설정 페이로드를 로드
        /// </summary>
        private void CustomPayload_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.PayloadPath = CustomPathBox.Text;
            MainWindow.ClientPath = PayloadClientPathBox.Text;
            Close();
        }
    }
}
