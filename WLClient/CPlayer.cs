using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace sis
{
    public class CPlayer
    {
        public static int ID = 0;
        public static Byte[] CD = null; //  CD max = ICON_WIDTH
        public static int Hp { get; set; }

        public static IPair Pos = new IPair();

        const int PADDING = 3;
        static SolidBrush PADDING_BRUSH = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
        static SolidBrush HP_BACK = new SolidBrush(Color.FromArgb(255, 25, 25, 25));
        static SolidBrush HP_FRONT = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
        static SolidBrush CD_FRONT = new SolidBrush(Color.FromArgb(192, 0, 0, 0));
        static Image[] SpellImage = new Image[GameDef.SPELL_COUNT << 1];

        public static void LoadSpellImage()
        {
            SpellImage[0] = WLClient.Properties.Resources.BTNFireBolt;
            SpellImage[1] = WLClient.Properties.Resources.BTNInnerFire;
            SpellImage[2] = WLClient.Properties.Resources.BTNBlink;
            SpellImage[3] = WLClient.Properties.Resources.BTNHoldPosition;
            SpellImage[4] = WLClient.Properties.Resources.BTNOrbOfFrost;
            SpellImage[5] = WLClient.Properties.Resources.BTNReplenishHealth;
        }
        static Bitmap UI_Cache = null;
        static void DrawUIFrame()
        {
            UI_Cache = new Bitmap(CurrentDisplayWidth, UI_HEIGHT);
            Graphics g = Graphics.FromImage(UI_Cache);
            Rectangle rect = new Rectangle(0, 0, CurrentDisplayWidth, PADDING);
            g.FillRectangle(PADDING_BRUSH, rect);

            rect.Y = UI_HEIGHT - PADDING;
            g.FillRectangle(PADDING_BRUSH, rect);

            rect.Y = PADDING;
            rect.Width = PADDING;
            rect.Height = UI_HEIGHT - (PADDING << 1);
            g.FillRectangle(PADDING_BRUSH, rect);

            rect.X = CurrentDisplayWidth - PADDING;
            for (int i = 0; i < 4; ++i)
            {
                g.FillRectangle(PADDING_BRUSH, rect);
                rect.X -= UI_ICON_WIDTH + PADDING;
            }

            rect.X += UI_ICON_WIDTH + (PADDING << 1);
            rect.Y += UI_ICON_WIDTH;
            rect.Width = 3 * UI_ICON_WIDTH + (PADDING << 1);
            rect.Height = PADDING;
            g.FillRectangle(PADDING_BRUSH, rect);

            g.Dispose();
        }
        static void DrawUIDetail()
        {
            Graphics g = Graphics.FromImage(UI_Cache);
            Rectangle rect = new Rectangle(PADDING, PADDING,
                CurrentDisplayWidth - 5 * PADDING - 3 * UI_ICON_WIDTH, UI_HEIGHT - (PADDING << 1));
            g.FillRectangle(HP_BACK, rect);

            int hp_width = rect.Width;

            if (Hp < 0) Hp = 0;
            rect.Width = rect.Width * Hp / (GameDef.UNIT_MAX_HP * GameDef.PIXEL_SCALE);
            g.FillRectangle(HP_FRONT, rect);

            rect.Y = PADDING;
            rect.Width = UI_ICON_WIDTH; rect.Height = UI_ICON_WIDTH;
            for (int i = 0; i < 2; ++i)
            {
                rect.X = hp_width + (PADDING << 1);
                for (int j = 0; j < 3; ++j)
                {
                    g.DrawImageUnscaled(SpellImage[i * 3 + j], rect.X, rect.Y);
                    if (CD != null)
                    {
                        if (CD[i * 3 + j] != 0)
                        {
                            rect.Height = CD[i * 3 + j];
                            g.FillRectangle(CD_FRONT, rect);
                            rect.Height = UI_ICON_WIDTH;
                        }
                    }
                    rect.X += UI_ICON_WIDTH + PADDING;
                }
                rect.Y += UI_ICON_WIDTH + PADDING;
            }
            g.Dispose();
        }
        public const int UI_HEIGHT = 111;   //  必須在 -3PADDING 後是偶數
        public const int UI_ICON_WIDTH = (UI_HEIGHT - 3 * PADDING) >> 1;    //  (111-3*5)/2 = 51

        public static int CurrentScale = 1; //  1 = normal pixel, 2 = full pixel
        public static int CurrentDisplayWidth, CurrentDisplayHeight;    //  螢幕上顯示遊戲畫面的長寬
        static int LastDrawNumber = -1;
        public static Bitmap GameCache = null;

        public static int CurrentTop = 0;   //  此時左上角對應到遊戲的座標
        public static int CurrentLeft = 0;  //  此時左上角對應到遊戲的座標
        public static int CurrentGameWidth = GameDef.WIDTH;     //  此時顯示的寬對應到遊戲的寬
        public static int CurrentGameHeight = GameDef.HEIGHT;

        public static void DrawPlayerUI(Graphics g)
        {
            if (UI_Cache != null)
            {
                DrawUIDetail();
                g.DrawImage(UI_Cache, 0, CurrentDisplayHeight);
            }
        }
        public static void CacheSize(int width, int height)
        {
            CurrentDisplayWidth = Math.Min(width * 8 / 10, GameDef.WIDTH);
            CurrentDisplayHeight = Math.Min(height - UI_HEIGHT, GameDef.HEIGHT);

            DrawUIFrame();
        }
        public static void FillGame(Graphics g)
        {
            DrawCache.GameCacheMutex.WaitOne();
            if (LastDrawNumber != sis.DrawCache.GameCacheNumber && sis.DrawCache.GameCache != null)
            {
                if (GameCache != null) GameCache.Dispose();
                GameCache = sis.DrawCache.GameCache;
                LastDrawNumber = sis.DrawCache.GameCacheNumber;
            }
            DrawCache.GameCacheMutex.ReleaseMutex();

            if (GameCache != null)
            {
                if (CurrentScale == 1)
                {
                    int src_x = (int)Pos.X - (CurrentDisplayWidth >> 1);
                    int src_y = (int)Pos.Y - (CurrentDisplayHeight >> 1);
                    if (src_x < 0) src_x = 0;
                    if (src_x + CurrentDisplayWidth > GameDef.WIDTH) src_x = GameDef.WIDTH - CurrentDisplayWidth;
                    if (src_y < 0) src_y = 0;
                    if (src_y + CurrentDisplayHeight > GameDef.HEIGHT) src_y = GameDef.HEIGHT - CurrentDisplayHeight;

                    Bitmap clip = GameCache.Clone(new Rectangle(src_x, src_y, CurrentDisplayWidth, CurrentDisplayHeight), GameCache.PixelFormat);
                    g.DrawImage(clip, 0, 0);
                    clip.Dispose();

                    CurrentLeft = src_x;
                    CurrentTop = src_y;
                    CurrentGameWidth = CurrentDisplayWidth;
                    CurrentGameHeight = CurrentDisplayHeight;
                }
                else
                {
                    //  原圖顯示
                    g.DrawImage(GameCache, 0, 0, CurrentDisplayWidth, CurrentDisplayHeight);

                    CurrentLeft = CurrentTop = 0;
                    CurrentGameWidth = GameDef.WIDTH;
                    CurrentGameHeight = GameDef.HEIGHT;
                }
            }
        }
        public static bool ValidMousePos(int x, int y)
        {
            return x < CurrentDisplayWidth && y < CurrentDisplayHeight;
        }
        public static IPair GetGamePos(int x, int y)
        {
            return new IPair(CurrentLeft + x * CurrentGameWidth / CurrentDisplayWidth, CurrentTop + y * CurrentGameHeight / CurrentDisplayHeight);
        }
    }
}
