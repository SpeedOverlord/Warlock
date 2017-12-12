using System.Drawing;

namespace sis
{
    public class EFRedemption : Effect
    {
        public const int RADIUS = Redemption.RADIUS;
        public static SolidBrush BRUSH = new SolidBrush(Color.FromArgb(168, 255, 255, 0));
        public EFRedemption() : base(15, false)
        {
            DrawId = 4;
        }
    }
}
