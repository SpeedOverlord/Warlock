using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace sis
{
    public class DrawThread
    {
        public static Thread thread = null;
        public static Mutex mutex = new Mutex();
        static bool executing = false, enable = true;
        static Byte[] buffering_data = null;
        static Byte[] executing_data = null;

        public static void PushData(Byte[] data) {
            if(enable== false) return;
            mutex.WaitOne();
            buffering_data = data;
            if (executing == false)
            {
                executing = true;
                executing_data = buffering_data;
                thread = new Thread(DrawThreadProc);
                thread.Start();
            }
            mutex.ReleaseMutex();
        }
        public static void DrawThreadProc()
        {
            while (true)
            {

                //GameDef.Form.PushLog("開始畫畫 length = " + executing_data.Length + " at Recv\n");
                DrawCache.CacheFromPackage(executing_data);
                GameDef.Form.UpdateForm();

                mutex.WaitOne();
                if (enable == true && executing_data != buffering_data) executing_data = buffering_data;
                else break;
                mutex.ReleaseMutex();
            }
            executing = false;
            mutex.ReleaseMutex();
        }
        public static void Exit()
        {
            enable = false;
        }
    }
}
