namespace sis
{
    public class Spell
    {
        protected int cast_time_, cd_time_;
        //  retrieve
        public int CastTime
        {
            get { return cast_time_; }
        }
        public int CdTime
        {
            get { return cd_time_; }
        }
        //  virtual
        public virtual void Cast(Unit unit, IPair destination, Effect[] effect_list)
        {
            //  player have finished casting
            StopCast(unit, destination, effect_list);
        }
        public virtual Effect[] BeginCast(Unit unit, IPair destination)
        {
            //  player starting casting
            //  do nothing or add effect
            return null;
        }
        public virtual void StopCast(Unit unit, IPair destination, Effect[] effect_list)
        {
            //  player stoping casting
            //  kill effect
        }
    }
}
