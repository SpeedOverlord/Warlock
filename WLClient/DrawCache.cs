using System;
using System.IO;
using System.Drawing;
using System.Threading;

namespace sis
{
    public class DrawCache
    {
        public const int ID_COUNT = 8;
        static Bitmap[] Cache = new Bitmap[ID_COUNT];
        static int[] CacheRadius = new int[ID_COUNT];
        static Brush id3_brush = new SolidBrush(Color.FromArgb(255, 255, 127, 39)); //  make fire ball

        static SolidBrush Hp_Bar_Back = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
        static SolidBrush[] Hp_Bar_Front;

        public static void Initialize()
        {
            CacheRadius[0] = GameDef.UNIT_RADIUS;
            CacheRadius[1] = GameDef.GROUND_RADIUS;
            CacheRadius[2] = GameDef.FIREBALL_RADIUS;
            CacheRadius[3] = CacheRadius[2];
            CacheRadius[4] = GameDef.REDEMPTION_RADIUS;
            CacheRadius[5] = GameDef.GRAVITY_RADIUS;
            CacheRadius[6] = GameDef.TRACEBALL_RADIUS;

            Color[] color = new Color[ID_COUNT];
            color[0] = Color.FromArgb(255,255,255,255);

            color[2] = Color.FromArgb(255, 255, 127, 39);

            color[4] = Color.FromArgb(168, 255, 255, 0);
            color[5] = Color.FromArgb(128, 64, 255, 64);
            color[6] = Color.FromArgb(255, 25, 255, 255);

            Hp_Bar_Front = new SolidBrush[4];
            Hp_Bar_Front[0] = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
            Hp_Bar_Front[1] = new SolidBrush(Color.FromArgb(255, 255, 133, 11));
            Hp_Bar_Front[2] = new SolidBrush(Color.FromArgb(255, 255, 255, 0));
            Hp_Bar_Front[3] = new SolidBrush(Color.FromArgb(255, 54, 245, 64));

            for (int i = 0; i < ID_COUNT; ++i)
            {
                if (i == 3) continue;
                if (i != 7) Cache[i] = new Bitmap(CacheRadius[i] << 1, CacheRadius[i] << 1);
                else Cache[i] = new Bitmap(GameDef.WIDTH, GameDef.HEIGHT);
                Graphics g = Graphics.FromImage(Cache[i]);
                Rectangle rect = new Rectangle(0, 0, Cache[i].Width, Cache[i].Height);
                Brush brush;
                switch (i)
                {
                    case 1:
                        brush = new TextureBrush(WLClient.Properties.Resources.bricks);
                        break;
                    case 7:
                        brush = new TextureBrush(WLClient.Properties.Resources.lavas);
                        break;
                    default:
                        brush = new SolidBrush(color[i]);
                        break;
                }
                if (i == 7) g.FillRectangle(brush, rect);
                else g.FillEllipse(brush, rect);
                g.Dispose();
            }
        }
        public static void Draw(Graphics g, int id, int arg, int x,int y)
        {
            if (id >= ID_COUNT && id != 10) return;
            if (id == 3)
            {
                //  dynamic draw
                int radius = arg * GameDef.FIREBALL_RADIUS / GameDef.FIREBALL_CAST_TIME;

                Rectangle rect = new Rectangle(x - radius, y - radius, radius << 1, radius << 1);
                g.FillEllipse(id3_brush, rect);
            }
            else if (id == 7)
            {
                g.DrawImage(Cache[id], 0, 0);
            }
            else if (id == 10)
            {
                g.DrawImage(WLClient.Properties.Resources.Exclamation, x, y);
            }
            else
            {
                g.DrawImage(Cache[id], x - CacheRadius[id], y - CacheRadius[id]);
            }
        }
        public static void DrawHpBar(Graphics g, int hp, int x,int y)
        {
            y -= GameDef.UNIT_RADIUS + 5 + 5;
            if (y < 0) y += ((GameDef.UNIT_RADIUS + 5) << 1) + 5;

            Rectangle rect = new Rectangle(x - GameDef.UNIT_RADIUS, y, GameDef.UNIT_RADIUS << 1, 5);

            g.FillRectangle(Hp_Bar_Back, rect);

            if (hp < 0) hp = 0;
            int rate = hp * 100 / (GameDef.UNIT_MAX_HP * GameDef.PIXEL_SCALE);
            if (hp > 0 && rate == 0) rate = 1;

            rect.Width = rect.Width * rate / 100;
            if (hp > 0 && rect.Width == 0) rect.Width = 1;

            Brush brush = null;
            if (rate >= 80) brush = Hp_Bar_Front[3];
            else if (rate >= 50) brush = Hp_Bar_Front[2];
            else if (rate >= 25) brush = Hp_Bar_Front[1];
            else brush = Hp_Bar_Front[0];

            g.FillRectangle(brush, rect);
        }

        public static Mutex GameCacheMutex = new Mutex();
        public static Bitmap GameCache = null;
        public static int GameCacheNumber = -1;
        const int GameCacheNumberMod = GameDef.PackMod;

        public static void CacheFromPackage(Byte[] data)
        {
            //  已將長度頭去掉之 data
            Bitmap bitmap = new Bitmap(Cache[7]);

            Graphics g = Graphics.FromImage(bitmap);

            Draw(g, 1, 0, GameDef.WIDTH >> 1, GameDef.HEIGHT >> 1);

            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);

            short temp_short;
            Byte temp_byte;

            temp_short = reader.ReadByte();

            if (GameCacheNumber != -1 && temp_short < GameCacheNumber)
            {
                //  此封包已經過期，不需要更新
                if (GameCacheNumber - temp_short < 100)
                {
                    return;
                }
                //  mod 剛執行，需要更新
            }

            CPlayer.Hp = 0;    //  假設已死亡

            temp_byte = reader.ReadByte();
            for (Byte i = 0; i < temp_byte; ++i)
            {
                UnpackUnit(g, reader);
            }

            temp_byte = reader.ReadByte();
            for (Byte i = 0; i < temp_byte; ++i)
            {
                UnpackEffect(g, reader);
            }

            CPlayer.CD = reader.ReadBytes(GameDef.SPELL_COUNT);

            g.Dispose();

            GameCacheMutex.WaitOne();
            GameCacheNumber = temp_short;
            if (GameCache != null && GameCache != CPlayer.GameCache) GameCache.Dispose();
            GameCache = bitmap;
            GameCacheMutex.ReleaseMutex();
        }
        static void UnpackUnit(Graphics g, BinaryReader reader)
        {
            short hp = reader.ReadInt16();
            if (hp < 0) hp = 0;
            int temp_int = reader.ReadInt32();

            int draw_arg = temp_int & 0x1F;
            temp_int >>= 5;
            int player_id = temp_int & 0x1F;
            temp_int >>= 5;
            int Y = temp_int & 0x7FF;
            temp_int >>= 11;
            int X = temp_int & 0x7FF;

            if (player_id == CPlayer.ID)
            {
                CPlayer.Hp = hp;
                CPlayer.Pos.X = X;
                CPlayer.Pos.Y = Y;
            }

            Draw(g, 0, draw_arg, X, Y);
            DrawHpBar(g, hp, X, Y);
        }
        static void UnpackEffect(Graphics g, BinaryReader reader)
        {
            int temp_int = reader.ReadInt32();
            int draw_arg = temp_int & 0x3F;
            temp_int >>= 6;
            int draw_id = temp_int & 0xF;
            temp_int >>= 4;
            int Y = temp_int & 0x7FF;
            temp_int >>= 11;
            int X = temp_int & 0x7FF;

            Draw(g, draw_id, draw_arg, X, Y);
        }
    }
}
