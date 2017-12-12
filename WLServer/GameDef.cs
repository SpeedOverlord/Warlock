using System;
using System.Drawing;

namespace sis
{
    class GameDef
    {
        //  game
        public const int WIDTH = 800, HEIGHT = WIDTH;
        public const int MAX_PLAYER = 20;
        public const int REPULSE_DECREASE_FACTOR = 3;
        //  coordinate
        public const int PIXEL_SCALE = 100;
        //  ground
        public const int GROUND_DECREASE_RATE = 15;
        public const int GROUND_RADIUS = 270;
        public const int LAVA_DAMAGE = 22;
        //  unit
        public const int UNIT_MAX_HP = 100;
        public const int UNIT_RADIUS = 10;
        public const int UNIT_RECOVER = 2;   //  每秒回一滴血
        public const int UNIT_RESPAWN_TIME = 150;
        public static SolidBrush UNIT_BRUSH = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
        //  spell
        public const int SPELL_COUNT = 6;
        public const int FIREBALL_RADIUS = 15;
        public const int REDEMPTION_RADIUS = UNIT_RADIUS * 5;
        public const int GRAVITY_RADIUS = 90;
        public const int TRACEBALL_RADIUS = 8;

        public const int PackMod = 200;

        public static bool CollisionWall(IPair pos, int radius)
        {
            //  判斷是否碰撞牆壁
            return pos.X / PIXEL_SCALE + radius >= WIDTH || pos.X / PIXEL_SCALE < radius || pos.Y / PIXEL_SCALE + radius >= HEIGHT || pos.Y / PIXEL_SCALE < radius;
        }
        public static void AdjustCollisionWall(IPair pos, int radius)
        {
            if (pos.X / PIXEL_SCALE + radius >= WIDTH) pos.X = (WIDTH - radius - 1) * PIXEL_SCALE;
            if (pos.X / PIXEL_SCALE < radius) pos.X = radius * PIXEL_SCALE;
            if (pos.Y / PIXEL_SCALE + radius >= HEIGHT) pos.Y = (HEIGHT - radius - 1) * PIXEL_SCALE;
            if (pos.Y / PIXEL_SCALE < radius) pos.Y = radius * PIXEL_SCALE;
        }
    }
}
