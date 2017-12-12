using System.Drawing;

namespace sis
{
    public class EFFireBall : Effect
    {
        public const int RADIUS = MIFireBall.RADIUS;
        public static SolidBrush BRUSH = new SolidBrush(Color.FromArgb(255, 255, 127, 39));
        public EFFireBall() : base(1, true)
        {
            DrawId = 2;
        }
    }
}
