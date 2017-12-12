namespace sis
{
    public class Gravity : Spell
    {
        public Gravity()
        {
            cast_time_ = 5;
            cd_time_ = 300;
        }
        public override void Cast(Unit unit, IPair destination, Effect[] effect_list)
        {
            base.Cast(unit, destination, effect_list);
            MIGravity missile = new MIGravity(unit, destination);
            missile.Register();
        }
    }
}
