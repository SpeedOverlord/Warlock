using System.Drawing;

namespace sis
{
    public class Ground : Drawable
    {
        public const int RADIUS = GameDef.GROUND_RADIUS;
        IPair pos_;
        public IPair Pos
        {
            get { return pos_; }
            set { pos_.Clone(value); }
        }
        public long X
        {
            get { return Pos.X; }
            set { Pos.X = value; }
        }
        public long Y
        {
            get { return Pos.Y; }
            set { Pos.Y = value; }
        }

        public Ground(int width, int height)
        {
            pos_ = new IPair((width >> 1) * GameDef.PIXEL_SCALE, (height >> 1) * GameDef.PIXEL_SCALE);
        }
        public bool InLava(IPair pos)
        {
            IPair delta = pos - Pos;
            int ground_radius = RADIUS * GameDef.PIXEL_SCALE;
            return delta.LengthSquare() > ground_radius * ground_radius;
        }
        //  packer
        public int DrawId { get { return 1; } }
        public int DrawArg { get { return RADIUS; } }
    }
}
