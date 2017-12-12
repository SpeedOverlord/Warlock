using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sis
{
    public class MIHealBall : Missile
    {
        public const int RADIUS = 50;
        public const int STRENGTH = 50;  //  as 50 damage spell
        public MIHealBall(Unit caster, IPair destination) : base(80, caster)
        {
            radius_ = RADIUS;
            damage_ = -10;
            speed_ = 350;

            hurt_self_ = true;
            explode_enemy_ = false;
            explode_enemy_missile_ = false;

            Unit target = null;
            bool used = false;
            long min_distance = 0, temp_distance;
            foreach (Unit unit in Game.Unit_List)
            {
                if (unit == caster) continue;
                if (used == false)
                {
                    used = true;
                    target = unit;
                    min_distance = (unit.Pos - Pos).LengthSquare();
                }
                else
                {
                    temp_distance = (unit.Pos - Pos).LengthSquare();
                    if (temp_distance < min_distance)
                    {
                        target = unit;
                        min_distance = temp_distance;
                    }
                }
            }
            if (target != null) destination.Clone(target.Pos);

            IPair delta = destination - caster.Pos;
            move_vector_ = delta.Clone();
            move_vector_.ChangeLength(speed_);
            delta.ChangeLength(GameDef.UNIT_RADIUS * GameDef.PIXEL_SCALE);
            Pos.Clone(caster.Pos);
            Pos.Add(delta);
        }
        public override void Register()
        {
            effect_list_ = new Effect[1];
            effect_list_[0] = new EFGravity();
            effect_list_[0].Pos.Clone(Pos);
            effect_list_[0].Register();
            base.Register();
        }
        public override void Unregister()
        {
            effect_list_[0].Unregister();
            effect_list_[0] = null;
            effect_list_ = null;
            base.Unregister();
        }
    }
}
