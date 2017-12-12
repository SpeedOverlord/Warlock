using System;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace sis
{
    public class Player
    {
        public int ID = -1;
        public Unit FocusUnit = null;
        public TcpClient client;
        public IPair dead_pos = null;
        public EFExclamation exclamation = null;
        public void PushStop()
        {
            if (FocusUnit == null) return;
            FocusUnit.Stop();
        }
        public void PushMove(int x, int y)
        {
            if (FocusUnit == null) return;
            FocusUnit.Move(new IPair(x * GameDef.PIXEL_SCALE, y * GameDef.PIXEL_SCALE));
        }
        public void PushCast(Keys key, IPair pos)
        {
            if (FocusUnit == null && key != Keys.R) return;
            //  parse key
            switch (key)
            {
                case Keys.Q:
                    FocusUnit.Cast(0, pos);
                    break;
                case Keys.W:
                    FocusUnit.Cast(1, pos);
                    break;
                case Keys.E:
                    FocusUnit.Cast(2, pos);
                    break;
                case Keys.S:
                    FocusUnit.Cast(3, pos);
                    break;
                case Keys.D:
                    FocusUnit.Cast(4, pos);
                    break;
                case Keys.F:
                    FocusUnit.Cast(5, pos);
                    break;
                case Keys.R:
                    //  驚嘆號
                    if (exclamation != null)
                    {
                        //  remove first
                        if (Game.Effect_List.Contains(exclamation))
                        {
                            exclamation.Unregister();
                            exclamation = null;
                        }
                    }
                    if (FocusUnit != null)
                    {
                        exclamation = new EFExclamation(this, FocusUnit.Pos);
                    }
                    else if (dead_pos != null)
                    {
                        exclamation = new EFExclamation(this, dead_pos);
                    }
                    if (exclamation != null)
                    {
                        exclamation.Register();
                    }
                    break;
                default:
                    break;
            }
        }

        //  respawn
        Effect respawn_effect_ = null;
        public void Dead()
        {
            dead_pos = FocusUnit.Pos.Clone();
            FocusUnit.Stop();   //  中斷施法
            Game.Unit_List.Remove(FocusUnit);
            FocusUnit = null;
            respawn_effect_ = new EFRespawn(this);
            respawn_effect_.Register();
        }
        public void Spawn()
        {
            FocusUnit = new Unit(this,Game.GetRespawnPoint());
            Game.Unit_List.Add(FocusUnit);
            respawn_effect_ = null;
        }
        //  connect
        Thread recv_thread = null;
        public void StartRecv()
        {
            recv_thread = new Thread(Recv);
            recv_thread.Start();
        }
        public void Recv()
        {
            BinaryReader reader = new BinaryReader(client.GetStream());

            while (true)
            {
                try
                {
                    Byte type = reader.ReadByte();
                    if (type == 0 || type == 1)
                    {
                        int temp_int = reader.ReadInt32();
                        int y = temp_int & 0x7FF;
                        temp_int >>= 11;
                        int x = temp_int & 0x7FF;
                        short key = 0;
                        if (type == 1) key = reader.ReadInt16();
                        try
                        {
                            if (Game.UpdateMutex.WaitOne(50) == false)
                            {
                                ServerPT.Form.PushLog("recv jmp\n");
                            }
                            else
                            {
                                if (type == 0)
                                {
                                    PushMove(x, y);
                                }
                                else
                                {
                                    PushCast((Keys)key, new IPair(x, y).Mul(GameDef.PIXEL_SCALE));
                                }
                                Game.UpdateMutex.ReleaseMutex();
                            }
                        }
                        catch (Exception e)
                        {
                            StreamWriter stream = new StreamWriter("recv exception.log");
                            stream.WriteLine(e.ToString()); stream.Flush(); stream.Close(); stream.Dispose();
                            ServerPT.Form.PushLog("recv exception\n");
                        }
                    }
                    else
                    {
                        String text = reader.ReadString();
                    }
                }
                catch
                {
                    ServerPT.Form.PushLog("一個客戶端離線\n");

                    StreamWriter stream = new StreamWriter("disconnect.log"); stream.Close(); stream.Dispose();

                    Game.UpdateMutex.WaitOne();
                    Disconnect();
                    Game.UpdateMutex.ReleaseMutex();
                    break;
                }
            }
        }
        //  disconnect
        public void Disconnect()
        {
            if (respawn_effect_ != null) respawn_effect_.Unregister();
            if (FocusUnit != null)
            {
                Game.Unit_List.Remove(FocusUnit);
                FocusUnit = null;
            }
            Game.Player_List.Unregister(ID);
            client.Close();
        }

        //  sending buffering
        bool sending = false;
        Byte[] buffering_data, sending_data;
        Mutex send_mutex = new Mutex();
        public void PushSend(Byte[] data)
        {
            if (recv_thread == null) return;
            Byte[] temp_buffer = new Byte[data.Length];
            for (int i = 0; i < temp_buffer.Length; ++i) temp_buffer[i] = data[i];

            int cd_offset = temp_buffer.Length - 6;
            for (int i = 0; i < 6; ++i)
            {
                if (FocusUnit != null) temp_buffer[cd_offset + i] = (Byte)FocusUnit.GetCDRate(i);
            }

            send_mutex.WaitOne();
            buffering_data = temp_buffer;
            if (sending == false)
            {
                sending_data = buffering_data;
                sending = true;
                new Thread(SendProc).Start();
            }
            send_mutex.ReleaseMutex();
        }
        public void SendProc()
        {
            while (true)
            {
                try
                {
                    NetworkStream client_stream = client.GetStream();
                    client_stream.Write(sending_data, 0, sending_data.Length);
                    client_stream.Flush();
                    Byte[] test_length = new Byte[2];
                    test_length[0] = sending_data[0];
                    test_length[1] = sending_data[1];
                    MemoryStream mstream = new MemoryStream(test_length);
                    BinaryReader mreader = new BinaryReader(mstream);

                    short send_length = mreader.ReadInt16();
                    mreader.Dispose(); mstream.Dispose();
                }
                catch
                {

                }

                send_mutex.WaitOne();
                if (buffering_data != sending_data) sending_data = buffering_data;
                else break;
                send_mutex.ReleaseMutex();
            }
            sending = false;
            send_mutex.ReleaseMutex();
        }
    }
}
