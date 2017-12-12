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
        //  8898 as data flow
        public static String Name = null;
        static TcpClient Server = null;
        static NetworkStream ServerStream;
        static BinaryReader ServerReader = null;
        static Thread RecvThread = null;

        static String[] PlayerList = new String[GameDef.MAX_PLAYER];
        static void Close()
        {
            if (Connected()) Server.Close();
            if (RecvThread != null)
            {
                RecvThread.Join();
                RecvThread = null;
            }

            Server = null;
        }
        public static void PushString(String str)
        {
            if (str.Length == 0) return;
            if (String.Compare("exit", str, true) == 0)
            {
                Close();
                GameDef.Form.Close();
                return;
            }
            if (String.Compare("close", str, true) == 0)
            {
                Close();
                return;
            }
            if (Name == null)
            {
                //  setting name
                Name = str;
                GameDef.Form.PushLog("Set Name as " + Name + "\n");
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
            }
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
                Byte hash_length = ServerReader.ReadByte();
                Byte[] hash_data = ServerReader.ReadBytes(hash_length);

                //  hash

                //  send back
                MemoryStream mstream = new MemoryStream();
                BinaryWriter mwriter = new BinaryWriter(mstream);
                mwriter.Write(hash_data.Length);
                mwriter.Write(hash_data,0,hash_data.Length);
                mwriter.Flush(); mstream.Flush();
                mwriter.Dispose();
                Byte[] buffer = mstream.ToArray();
                ServerStream.Write(buffer, 0, buffer.Length);
                mstream.Dispose();


                

                //  send name
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(Name);
                writer.Flush(); stream.Flush();
                Byte[] buf = stream.ToArray();
                ServerStream.Write(buf, 0, buf.Length);

                //  secv player list
                while(Server.Available==0) Thread.Sleep(200);
                buf = new Byte[Server.Available];
                ServerStream.Read(buf, 0, buf.Length);

                stream = new MemoryStream(buf);
                BinaryReader reader = new BinaryReader(stream);
                Name = reader.ReadString();
                GameDef.Form.PushLog("Login as " + Name);
                Byte player_count = reader.ReadByte(), player_id;
                for (Byte i = 0; i < player_count; ++i)
                {
                    player_id = reader.ReadByte();
                    PlayerList[player_id] = reader.ReadString();
                    if (String.Compare(PlayerList[player_id], Name, false) == 0)
                    {
                        CPlayer.ID = player_id;
                    }
                }
                //  send ACK
                stream = new MemoryStream();
                writer = new BinaryWriter(stream);
                writer.Write((Byte)0);  //  ACK
                writer.Flush(); stream.Flush();
                buf = stream.ToArray();
                ServerStream.Write(buf, 0, 1);

                ControlRecvThread = new Thread(ControlRecv);
                ControlRecvThread.Start();
            }
            catch (SocketException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            return true;
        }
        public static void ControlRecv()
        {
            BinaryReader reader = new BinaryReader(ServerStream);
            Byte control_type = 16;
            while (true)
            {
                try
                {

                    while (Server.Available == 0) Thread.Sleep(200);



                }
                catch (IOException)
                {
                    GameDef.Form.ClientClose();
                    break;
                }
                catch (SocketException)
                {
                    GameDef.Form.ClientClose();
                    break;
                }
            }
        }
        public static void PushControl(int type, int x,int y, Keys key)
        {
            if (ControlRecvThread != null)
            {

            }
        }
    }
}
