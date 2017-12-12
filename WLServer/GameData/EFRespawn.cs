namespace sis
{
    public class EFRespawn : Effect
    {
        Player owner_;
        public EFRespawn(Player player) : base(GameDef.UNIT_RESPAWN_TIME, false)
        {
            DrawId = 8;

            owner_ = player;
        }
        public override void Trigger()
        {
            owner_.Spawn();
            base.Trigger();
        }
    }
}
