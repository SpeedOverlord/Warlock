namespace sis
{
    public class MITraceBall : Missile
    {
        public const int RADIUS = GameDef.TRACEBALL_RADIUS;
        Unit target_;
        public MITraceBall(Unit caster) : base(270, caster)
        {
            radius_ = RADIUS;
            damage_ = 400;
            speed_ = 600;

            hurt_self_ = false;
            explode_enemy_ = true;
            explode_enemy_missile_ = true;

            Pos.Clone(caster.Pos);

            bool used = false;
            long min_distance = 0, temp_distance;
            target_ = null;
            foreach (Unit unit in Game.Unit_List)
            {
                if (unit == caster) continue;
                if (used == false)
                {
                    used = true;
                    target_ = unit;
                    min_distance = (unit.Pos-Pos).LengthSquare();
                }
                else
                {
                    temp_distance = (unit.Pos-Pos).LengthSquare();
                    if(temp_distance < min_distance) {
                        target_ = unit;
                        min_distance = temp_distance;
                    }
                }
            }
        }
        public override void Update()
        {
            if (Game.Unit_List.Contains(target_))
            {
                IPair delta = target_.Pos - Pos;
                if (repulse_.Zero() == false)
                {
                    repulse_.ChangeLength(speed_ * 94 / 100);
                    delta.ChangeLength(speed_ - repulse_.Length());
                }
                else
                {
                    delta.ChangeLength(speed_);
                }
                repulse_.Add(delta);
            }
            else
            {
                move_vector_ = null;
                Explode = true;
            }
            base.Update();
        }
        public override void Register()
        {
            effect_list_ = new Effect[1];
            effect_list_[0] = new EFTraceBall();
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
