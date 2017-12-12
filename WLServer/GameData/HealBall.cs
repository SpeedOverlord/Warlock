namespace sis
{
    public class HealBall : Spell
    {
        public HealBall()
        {
            cast_time_ = 5;
            cd_time_ = 350;
        }
        public override void Cast(Unit unit, IPair destination, Effect[] effect_list)
        {
            base.Cast(unit, destination, effect_list);
            MIHealBall missile = new MIHealBall(unit, destination);
            missile.Register();
        }
    }
}
