using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sis
{
    public class EFExclamation : Effect
    {
        public Player Owner;

        public EFExclamation(Player owner, IPair pos) : base(60, false)
        {
            DrawId = 10;

            Owner = owner;
            Pos.Clone(pos);
            Pos.Y -= 60 * GameDef.PIXEL_SCALE;
            GameDef.AdjustCollisionWall(Pos, 60);
        }
        public override void Update()
        {
            if (Owner.FocusUnit != null)
            {
                Pos.Clone(Owner.FocusUnit.Pos);
                Pos.Y -= 60 * GameDef.PIXEL_SCALE;
                GameDef.AdjustCollisionWall(Pos, 60);
            }
            
            base.Update();
        }
        public override void Trigger()
        {
            Owner.exclamation = null;

            base.Trigger();
        }
    }
}
