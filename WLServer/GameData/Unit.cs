using System;
using System.Collections.Generic;
using System.Drawing;

namespace sis
{
    public class Unit : Updatable, Drawable
    {
        Order order_;
        int[] cd_list_;
        int speed_;
        int hp_, mp_;    //  also x100
        IPair pos_, repusle_;
        Player last_damage_;
        //  retrieve
        public Player Owner { get; set; }
        public int Radius { get; set; }
        public int Hp
        {
            get { return hp_; }
            set { hp_ = value; }
        }
        public Player LastDamage
        {
            get { return last_damage_; }
        }
        public IPair Pos
        {
            get { return pos_; }
            set { pos_.Clone(value); }
        }
        //  constructor
        public Unit(Player owner, IPair pos)
        {
            order_ = new Order();
            cd_list_ = new int[GameDef.SPELL_COUNT];
            for (int i = 0; i < GameDef.SPELL_COUNT; ++i) cd_list_[i] = 0;
            speed_ = 350;
            hp_ = GameDef.UNIT_MAX_HP * GameDef.PIXEL_SCALE;
            mp_ = 0;
            pos_ = pos.Clone();
            repusle_ = new IPair();
            last_damage_ = owner;
            Owner = owner;
            Radius = GameDef.UNIT_RADIUS;
        }
        //  override
        public override void Update()
        {
            hp_ += GameDef.UNIT_RECOVER;
            hp_ = Math.Min(hp_, GameDef.UNIT_MAX_HP * GameDef.PIXEL_SCALE);
            //  cd count
            for (int i = 0; i < GameDef.SPELL_COUNT; ++i) if (cd_list_[i] > 0) --cd_list_[i];
            //  move
            IPair delta = repusle_.Clone();
            if (order_.Type == 1)
            {
                IPair move_vector = order_.Param1 - Pos;
                if (move_vector.LengthSquare() > speed_ * speed_) move_vector.ChangeLength(speed_);
                else
                {
                    order_.Stop();
                }
                delta.Add(move_vector);
            }
            //  repusle decrease
            long repusle_length_square = repusle_.LengthSquare();
            long repusle_decrease = speed_ >> GameDef.REPULSE_DECREASE_FACTOR;
            if (repusle_length_square < repusle_decrease * repusle_decrease)
            {
                repusle_.X = repusle_.Y = 0;
            }
            else
            {
                repusle_.ChangeLength((long)Math.Sqrt((double)repusle_length_square) - repusle_decrease);
            }
            Pos.Add(delta);
            GameDef.AdjustCollisionWall(Pos, Radius);
        }
        public void UpdateAction()
        {
            //  move 和 cast 分開 update 否則剛創造出來的 missile 就可以馬上移動(update) (因為我們的順序是先 unit update 後 missile update)
            //  還有就是，如果 A 完成移動後也完成天罰擊中 B，那 B 在此時間點的移動會加上天罰
            //  cast count
            if (order_.Type == 2)
            {
                Spell current_spell = Game.Spell_List[order_.Param2.X];
                if (order_.Param2.Y == 0)
                {
                    order_.EffectList = current_spell.BeginCast(this, order_.Param1);
                }
                if (order_.Param2.Y >= current_spell.CastTime)
                {
                    current_spell.Cast(this, order_.Param1, order_.EffectList);
                    order_.EffectList = null;
                    cd_list_[order_.Param2.X] = current_spell.CdTime;   //  進入 cd
                    order_.Stop();
                }
                order_.CastCount();
            }
        }
        public virtual void Draw(Graphics g)
        {
            Rectangle rect = new Rectangle((int)pos_.X / GameDef.PIXEL_SCALE - Radius,
                (int)pos_.Y / GameDef.PIXEL_SCALE - Radius, Radius << 1, Radius << 1);
            g.FillEllipse(GameDef.UNIT_BRUSH, rect);
        }
        //  order
        //  指令都是由上層 Player 傳遞下來
        public void Stop()
        {
            if (order_.Casting()) InterruptCast();
            order_.Stop();
        }
        public void Move(IPair destination)
        {
            if (order_.Casting()) InterruptCast();
            order_.Move(destination);
        }
        public bool Cast(int id, IPair destination) {
            //  return whether success (in cold-down or no ability)
            //  location test
            if (destination.Equal(pos_)) return false;
            //  check cd
            if (cd_list_[id] > 0) return false;
            //  not in cd
            if (order_.Casting())
            {
                if ((order_.Param2.X == id && order_.Param1.Equal(destination)) || order_.Param2.X == 1) return true;  //  同樣的指令不需要取消
                InterruptCast();
            }
            order_.Cast(id, destination);
            return true;
        }
        void InterruptCast()
        {
            Spell current_spell = Game.Spell_List[order_.Param2.X];
            current_spell.StopCast(this, order_.Param1, order_.EffectList);
            order_.EffectList = null;
        }
        //  operator
        public void Damage(Player player, IPair pos, int damage)
        {
            hp_ -= damage;
            mp_ += damage;  //  補血就賺囉
            if (mp_ < 0) mp_ = 0;
            if (damage <= 0) return;    //  heal
            last_damage_ = player;

            IPair delta = pos_ - pos;
            if (delta.Zero() == false)
            {
                delta.ChangeLength((damage * 3 * (mp_ / GameDef.PIXEL_SCALE + 100) / 100) >> 1);
                repusle_.Add(delta);
            }
        }
        public void AddRepulse(IPair delta) {
            repusle_.Add(delta);
        }
        public bool Dead()
        {
            return hp_ <= 0;
        }
        //  packer
        public int DrawId { get { return 0; } }
        public int DrawArg { get { return 0; } }
        public int GetCDRate(int index)
        {
            return cd_list_[index] * 51 / Game.Spell_List[index].CdTime;
        }
    }
}
