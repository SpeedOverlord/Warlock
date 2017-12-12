using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace sis
{
    public class ServerPT
    {
        public static WLServer.ServerForm Form;
        static Thread ListenThread = null;
        static bool exit = false;
        public static void StartListen()
        {
            ListenThread = new Thread(ListenProc);
            ListenThread.Start();
        }
        public static void ListenProc()
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, 8897);
                listener.Start();
                Form.PushLog("Start Listening on 8897\n");

                while (exit == false)
                {
                    while (exit == false && listener.Pending() == false) Thread.Sleep(200);

                    if (exit == true)
                    {
                        listener.Stop();
                        break;
                    }
                    TcpClient client = listener.AcceptTcpClient();

                    Thread client_thread = new Thread(() => ClientLoginProc(client));
                    client_thread.Start();
                }
            }
            catch
            {
            }
        }
        public static void Exit()
        {
            exit = true;
            ListenThread.Join();
        }
        public static void ClientLoginProc(TcpClient client)
        {
            Player player = null;
            try
            {
                if (Game.Player_List.Count >= GameDef.MAX_PLAYER) throw new IOException();

                NetworkStream client_stream = client.GetStream();
                client.NoDelay = true;

                Byte[] hash_data = new Byte[10];
                for (int i = 0; i < 10; ++i) hash_data[i] = (Byte)i;

                MemoryStream mstream = new MemoryStream();
                BinaryWriter mwriter = new BinaryWriter(mstream);
                //  send hash
                Form.PushLog("開始傳送 hash string, length = 11\n");
                mwriter.Write((Byte)hash_data.Length);
                mwriter.Write(hash_data, 0, hash_data.Length);
                mwriter.Flush(); mstream.Flush();
                Byte[] buffer = mstream.ToArray();
                client_stream.Write(buffer, 0, buffer.Length);
                mwriter.Dispose(); mstream.Dispose();

                //  recv hash
                Form.PushLog("開始接收 hash string\n");
                while (client.Available < 11) Thread.Sleep(200);
                buffer = new Byte[11];
                client_stream.Read(buffer, 0, 11);
                mstream = new MemoryStream(buffer);
                BinaryReader mreader = new BinaryReader(mstream);
                Byte length = mreader.ReadByte();
                Form.PushLog("hash string 長度 = " + length + "\n");
                if (length != 10) throw new IOException();
                for (int i = 0; i < 10; ++i) if (buffer[i + 1] != i) throw new IOException();
                Form.PushLog("hash string 驗證成功\n");

                player = new Player();
                player.client = client;
                Game.UpdateMutex.WaitOne();
                if (Game.Player_List.Count >= GameDef.MAX_PLAYER)
                {
                    Game.UpdateMutex.ReleaseMutex();
                    throw new IOException();
                }
                player.ID = Game.Player_List.Register(player);
                Game.UpdateMutex.ReleaseMutex();

                mstream = new MemoryStream();
                mwriter = new BinaryWriter(mstream);
                mwriter.Write((Byte)player.ID);
                mwriter.Flush(); mstream.Flush();
                buffer = mstream.ToArray();
                Form.PushLog("傳送 Player ID = " + player.ID + "\n");
                client_stream.Write(buffer, 0, 1);
                Form.PushLog("傳送 Player ID 完成\n");
                mwriter.Dispose(); mstream.Dispose();
                
                while (client.Available < 1) Thread.Sleep(200);
                buffer = new Byte[1];
                Form.PushLog("開始接收 ACK\n");
                client_stream.Read(buffer, 0, 1);

                Form.PushLog("ACK 接收完成\n");
                player.StartRecv();
                Game.UpdateMutex.WaitOne();
                player.Spawn();
                Game.UpdateMutex.ReleaseMutex();
            }
            catch
            {
                Form.PushLog("一個用戶在驗證中失敗\n");
                if (player != null)
                {
                    if (player.ID != -1) Game.Player_List.Unregister(player.ID);
                }
                client.Close();
                return;
            }
            Form.PushLog("一個用戶已登入\n");
        }
    }
}
