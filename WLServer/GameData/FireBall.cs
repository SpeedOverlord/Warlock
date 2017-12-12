namespace sis
{
    public class FireBall : Spell
    {
        public FireBall()
        {
            cast_time_ = 8;
            cd_time_ = 100;
        }
        public override void Cast(Unit unit, IPair destination, Effect[] effect_list)
        {
            base.Cast(unit, destination, effect_list);
            MIFireBall missile = new MIFireBall(unit, destination);
            missile.Register();
        }
        public override Effect[] BeginCast(Unit unit, IPair destination)
        {
            IPair delta = destination - unit.Pos;
            delta.ChangeLength(GameDef.UNIT_RADIUS * GameDef.PIXEL_SCALE);

            Effect[] effect_list = new Effect[1];
            effect_list[0] = new EFMakeFireBall(unit, delta);
            effect_list[0].Register();
            return effect_list;
        }
        public override void StopCast(Unit unit, IPair destination, Effect[] effect_list)
        {
            if (effect_list != null)
            {
                effect_list[0].Unregister();
                effect_list[0] = null;
            }
        }
    }
}
