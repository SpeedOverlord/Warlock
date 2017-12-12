using System.Drawing;

namespace sis
{
    public class EFGravity : Effect
    {
        public const int RADIUS = MIGravity.RADIUS;
        public static SolidBrush BRUSH = new SolidBrush(Color.FromArgb(128, 64, 255, 64));

        public EFGravity() : base(1, true)
        {
            DrawId = 5;
        }
    }
}
