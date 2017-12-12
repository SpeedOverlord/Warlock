using System.Drawing;

namespace sis
{
    public class EFTraceBall : Effect
    {
        public const int RADIUS = MITraceBall.RADIUS;
        public static SolidBrush BRUSH = new SolidBrush(Color.FromArgb(255, 25, 255, 255));
        public EFTraceBall() : base(1, true)
        {
            DrawId = 6;
        }
    }
}
