using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace sis
{
    public class ClientPT
    {
        //  8897 as control flow
        volatile static TcpClient Server = null;
        volatile static NetworkStream ServerStream;
        volatile static BinaryReader ServerReader = null;
        volatile static Thread RecvThread = null;

        static String[] PlayerList = new String[GameDef.MAX_PLAYER];
        static void Close()
        {
            if (Connected()) Server.Close();
            if (RecvThread != null)
            {
                //RecvThread.Join();
                RecvThread = null;
            }

            Server = null;
        }
        public static bool StringEqual(String str1, String str2)
        {
            GameDef.Form.PushLog("Length " + str1.Length + ", " + str2.Length + "\n");
            if (str1.Length != str2.Length) return false;
            for (int i = 0; i < str1.Length; ++i)
            {
                GameDef.Form.PushLog("Char " + str1[i] + ", " + str2[i] + "\n");
                if (str1[i] != str2[i]) return false;
            }
            return true;
        }
        public static void PushString(String str)
        {
            if (str.Length == 0) return;
            if (str.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Close();
                GameDef.Form.Close();
                return;
            }
            if (str.Equals("close", StringComparison.OrdinalIgnoreCase))
            {
                Close();
                return;
            }
            if (Connected() == false)
            {
                if (Connect(str))
                {
                    GameDef.Form.PushLog("Connect to " + str + " success\n");
                }
                else
                {
                    GameDef.Form.PushLog("Connect to " + str + "fail\n");
                }
                return;
            }
            //  chat
            //PushControl(2, 0, 0, 0, str);
        }
        public static bool Connected()
        {
            return Server != null && Server.Connected;
        }
        public static bool Connect(String hostname)
        {
            if (Connected()) return true;
            Server = new TcpClient();
            try
            {
                Server.Connect(hostname, 8897);

                //  login
                Server.NoDelay = true;
                ServerStream = Server.GetStream();
                ServerReader = new BinaryReader(ServerStream);

                //  recv hash array
                GameDef.Form.PushLog("開始接收 hash string\n");
                Byte hash_length = ServerReader.ReadByte();
                GameDef.Form.PushLog("hash string 長度 = " + hash_length + "\n");
                Byte[] hash_data = ServerReader.ReadBytes(hash_length);
                GameDef.Form.PushLog("hash string 接收完成\n");

                //  hash

                //  send back
                GameDef.Form.PushLog("開始傳送 hash string\n");
                MemoryStream mstream = new MemoryStream();
                BinaryWriter mwriter = new BinaryWriter(mstream);
                mwriter.Write((Byte)hash_data.Length);
                mwriter.Write(hash_data,0,hash_data.Length);
                mwriter.Flush(); mstream.Flush();
                Byte[] buffer = mstream.ToArray();
                ServerStream.Write(buffer, 0, buffer.Length);
                mwriter.Dispose(); mstream.Dispose();

                //  recv id
                GameDef.Form.PushLog("開始接收玩家 ID\n");
                Byte id = ServerReader.ReadByte();
                CPlayer.ID = id;
                GameDef.Form.PushLog("玩家 ID = " + id + "\n");

                //  ack
                GameDef.Form.PushLog("開始傳送 ACK\n");
                buffer = new Byte[1];
                ServerStream.Write(buffer, 0, 1);

                GameDef.Form.PushLog("開啟接收 thread\n");
                RecvThread = new Thread(Recv);
                RecvThread.Start();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static void Recv()
        {
            short pkg_length;
            while (true)
            {
                try
                {
                    while (Server.Available < 2) ;
                    pkg_length = ServerReader.ReadInt16();
                    //GameDef.Form.PushLog("下個封包預期長度 = " + pkg_length + " at Recv\n");
                    while (Server.Available < pkg_length - 2) ;
                    Byte[] buffer = ServerReader.ReadBytes(pkg_length - 2);  //  減去原本封包長
                    //GameDef.Form.PushLog("接收資料成功 length = " + buffer.Length + " at Recv\n");
                    //  post to draw thread
                    DrawThread.PushData(buffer);
                }
                catch
                {
                    GameDef.Form.PushLog("接收資料失敗 at Recv\n");
                    GameDef.Form.ClientClose();
                    break;
                }
            }
        }
        public static void PushControl(int type, int x,int y, Keys key, String text)
        {
            if (Connected())
            {
                MemoryStream mstream = new MemoryStream();
                BinaryWriter mwriter = new BinaryWriter(mstream);

                mwriter.Write((Byte)type);
                if (type == 0 || type == 1)
                {
                    //  move order
                    int temp_int = x & 0x7FF;
                    temp_int <<= 11;
                    temp_int += y & 0x7FF;
                    mwriter.Write(temp_int);
                    if (type == 1)
                    {
                        mwriter.Write((short)key);
                    }
                }
                else
                {
                    mwriter.Write(text);
                }
                mwriter.Flush(); mstream.Flush();
                Byte[] buffer = mstream.ToArray();
                try
                {
                    ServerStream.Write(buffer, 0, buffer.Length);
                    ServerStream.Flush();
                }
                catch
                {
                    GameDef.Form.PushLog("傳輸資料失敗 at PushControl\n");
                    GameDef.Form.ClientClose();
                }
                mwriter.Dispose(); mstream.Dispose();

                //Thread.Sleep(18);
            }
        }
    }
}
