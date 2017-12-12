namespace sis
{
    public class MIFireBall : Missile
    {
        public const int RADIUS = GameDef.FIREBALL_RADIUS;
        public MIFireBall(Unit caster, IPair destination) : base(66, caster)
        {
            radius_ = RADIUS;
            damage_ = 700;
            speed_ = 900;   //  x100

            hurt_self_ = false;
            explode_enemy_ = true;
            explode_enemy_missile_ = true;

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
            effect_list_[0] = new EFFireBall();
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
