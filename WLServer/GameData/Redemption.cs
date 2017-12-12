namespace sis
{
    public class Redemption : Spell
    {
        public const int RADIUS = GameDef.REDEMPTION_RADIUS;
        public const int DAMAGE = 1000;
        public Redemption()
        {
            cast_time_ = 20;
            cd_time_ = 10;
        }
        public override void Cast(Unit unit, IPair destination, Effect[] effect_list)
        {
            base.Cast(unit, destination, effect_list);
            foreach (Unit u in Game.Unit_List)
            {
                IPair delta = unit.Pos - u.Pos;
                int radius = (RADIUS + unit.Radius) * GameDef.PIXEL_SCALE;
                if (delta.LengthSquare() <= radius * radius)
                {
                    if (unit == u) {
                        u.Hp -= DAMAGE;
                        if (u.Hp <= 0) u.Hp = 1;
                    }
                    else u.Damage(unit.Owner, unit.Pos, DAMAGE);
                }
            }
            foreach (Missile missile in Game.Missile_List)
            {
                if (missile.Caster == unit) continue;
                IPair delta = unit.Pos - missile.Pos;
                int radius = (RADIUS + missile.Radius) * GameDef.PIXEL_SCALE;
                if (delta.LengthSquare() <= radius * radius)
                {
                    if(missile.ExplodeEnemyMissile) missile.Explode = true;
                }
            }
            EFRedemption effect = new EFRedemption();
            effect.Pos.Clone(unit.Pos);
            effect.Register();
        }
    }
}
