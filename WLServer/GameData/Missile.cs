namespace sis
{
    public class Missile : Countable
    {
        IPair pos_;   //  missile repulse 由重力球或追蹤球產生 (1/2 speed 抵銷)
        Unit caster_;

        protected IPair move_vector_, repulse_;
        protected Effect[] effect_list_;

        protected int radius_, damage_;
        protected int speed_;
        //  判斷常數    ally not include self
        //  所有投射物都會傷害敵人
        //  所有投射物都不會因為自己而爆炸
        //  所有投射物都不會被自己的投射物引爆
        protected bool hurt_self_, explode_enemy_, explode_enemy_missile_;
        //  retrieve
        public IPair Pos
        {
            get { return pos_; }
            set { pos_.Clone(value); }
        }
        protected int LifeTime
        {
            get { return period_; }
            set { period_ = value; }
        }
        public bool Explode { get; set; }   //  飛彈需要銷毀
        public bool Collision { get; set; } //  飛彈爆炸
        public IPair MoveVector { get { return move_vector_; } }
        public bool HurtSelf { get { return hurt_self_; } }
        public bool ExplodeEnemy { get { return explode_enemy_; } }
        public bool ExplodeEnemyMissile { get { return explode_enemy_missile_; } }
        public Effect[] EffectList { get { return effect_list_; } }
        public int Radius { get { return radius_; } }
        public Unit Caster { get { return caster_; } }
        //  constructor
        public Missile(int life_time, Unit caster)
            : base(life_time)
        {
            pos_ = new IPair();
            repulse_ = new IPair();
            caster_ = caster;
            move_vector_ = null;
            effect_list_ = null;
            Explode = Collision = false;
        }
        //  overload
        public override void Update()
        {
            base.Update();   //  count for life
            //delta
            IPair delta = new IPair();
            if (move_vector_ != null) delta.Add(move_vector_);
            delta.Add(repulse_);
            pos_.Add(delta);
            if (GameDef.CollisionWall(pos_, Radius)) Explode = true;
            //if (delta.Zero()) Explode = true;
            //  effect update
            if (effect_list_ != null)
            {
                foreach (Effect effect in effect_list_)
                {
                    //  特效也會做平移
                    effect.Pos.Add(delta);  //  effect update 由 list 執行
                }
            }
        }
        public override void Trigger()
        {
            //  life expire
            Explode = true;
        }
        //  register
        //  特效由此加入
        public virtual void Register()
        {
            Game.Missile_List.Add(this);
        }
        public virtual void Unregister()
        {
            Game.Missile_List.Remove(this);
        }
        //  collision
        public virtual void CheckCollision(Missile missile)
        {
            if (caster_ == missile.caster_) return;
            IPair delta = pos_ - missile.pos_;
            int collision_radius = (radius_ + missile.radius_) * GameDef.PIXEL_SCALE;
            if (delta.LengthSquare() > collision_radius * collision_radius) return;
            if (explode_enemy_missile_ && missile.explode_enemy_missile_) Explode = Collision = missile.Explode = missile.Collision = true;
        }
        public virtual void CheckCollision(Unit unit)
        {
            IPair delta = pos_ - unit.Pos;
            int collision_radius = (radius_ + unit.Radius) * GameDef.PIXEL_SCALE;
            if (delta.LengthSquare() > collision_radius * collision_radius) return;
            if (caster_ == unit)
            {
                if (hurt_self_) Collision = true;
            }
            else
            {
                Collision = true;
                if (explode_enemy_) Explode = true;
            }
        }
        public void CheckHurt(Unit unit)
        {
            IPair delta = pos_ - unit.Pos;
            int collision_radius = (radius_ + unit.Radius) * GameDef.PIXEL_SCALE;
            if (delta.LengthSquare() > collision_radius * collision_radius) return;
            if (caster_ == unit)
            {
                if (hurt_self_)
                {
                    unit.Damage(caster_.Owner, pos_, damage_);
                }
            }
            else
            {
                unit.Damage(caster_.Owner, pos_, damage_);
            }
        }
        public void AddRepulse(IPair delta)
        {
            repulse_.Add(delta);
        }
    }
}
