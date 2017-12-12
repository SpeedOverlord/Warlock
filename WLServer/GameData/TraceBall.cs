namespace sis
{
    public class TraceBall : Spell
    {
        public TraceBall()
        {
            cast_time_ = 2;
            cd_time_ = 200;
        }
        public override void Cast(Unit unit, IPair destination, Effect[] effect_list)
        {
            base.Cast(unit, destination, effect_list);
            MITraceBall missile = new MITraceBall(unit);
            missile.Register();
        }
    }
}
