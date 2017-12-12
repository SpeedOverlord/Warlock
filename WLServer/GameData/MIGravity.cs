namespace sis
{
    public class MIGravity : Missile
    {
        public const int RADIUS = GameDef.GRAVITY_RADIUS;
        public const int STRENGTH = 60;  //  as 50 damage spell
        public MIGravity(Unit caster, IPair destination) : base(80, caster)
        {
            radius_ = RADIUS;
            damage_ = 0;
            speed_ = 450;

            hurt_self_ = false;
            explode_enemy_ = false;
            explode_enemy_missile_ = false;

            IPair delta = destination - caster.Pos;
            move_vector_ = delta.Clone();
            move_vector_.ChangeLength(speed_);
            delta.ChangeLength(GameDef.UNIT_RADIUS * GameDef.PIXEL_SCALE);
            Pos.Clone(caster.Pos);
            Pos.Add(delta);
        }
        public override void Update()
        {
            base.Update();
            foreach (Unit unit in Game.Unit_List)
            {
                if (unit == Caster) continue;
                IPair delta = Pos - unit.Pos;
                int radius = (RADIUS + unit.Radius) * GameDef.PIXEL_SCALE;
                if (delta.LengthSquare() > radius * radius || delta.Zero()) continue;
                delta.ChangeLength(STRENGTH);
                unit.AddRepulse(delta);
            }
            foreach (Missile missile in Game.Missile_List)
            {
                if (missile.Caster == Caster) continue;
                IPair delta = Pos - missile.Pos;
                int radius = (RADIUS + missile.Radius) * GameDef.PIXEL_SCALE;
                if (delta.LengthSquare() > radius * radius || delta.Zero()) continue;
                delta.ChangeLength(STRENGTH);
                missile.AddRepulse(delta);
            }
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
