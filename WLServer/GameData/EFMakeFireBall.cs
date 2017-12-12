using System.Drawing;

namespace sis
{
    public class EFMakeFireBall : Effect
    {
        Unit unit_;
        IPair offset_;
        public EFMakeFireBall(Unit unit, IPair offset) : base(9, true)
        {
            DrawId = 3;

            unit_ = unit;
            offset_ = offset.Clone();
        }
        public override void Update()
        {
            Pos.Clone(unit_.Pos);
            Pos.Add(offset_);
            base.Update();
        }
    }
}
