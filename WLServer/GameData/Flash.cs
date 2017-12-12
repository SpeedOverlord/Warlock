namespace sis
{
    public class Flash : Spell
    {
        public const int MAX_RANGE = GameDef.UNIT_RADIUS * 25 *GameDef.PIXEL_SCALE;
        public Flash()
        {
            cast_time_ = 0;
            cd_time_ = 180;
        }
        public override void Cast(Unit unit, IPair destination, Effect[] effect_list)
        {
            base.Cast(unit, destination, effect_list);
            IPair delta = destination - unit.Pos;
            if (delta.LengthSquare() > MAX_RANGE * MAX_RANGE) delta.ChangeLength(MAX_RANGE);
            unit.Pos.Add(delta);
        }
    }
}
