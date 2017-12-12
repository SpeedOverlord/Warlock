using System.Drawing;

namespace sis
{
    public class Effect : Countable, Drawable
    {
        //  overload Updatae(int) 來讓 effect 移動
        bool loop_;
        IPair pos_;
        //  retrieve
        public IPair Pos
        {
            get { return pos_; }
            set { pos_.Clone(pos_); }
        }
        public bool Dead { get; set; }
        public bool Loop {
            get { return loop_; }
        }
        // constructor
        public Effect(int period, bool loop) : base(period)
        {
            pos_ = new IPair();
            loop_ = loop;
            Dead = false;
        }
        //  overload
        public override void Trigger()
        {
            if (loop_ == false) Dead = true;    //  Update 會自動重置 count
        }
        public virtual void Draw(Graphics g) { ; }
        //  operator
        public void Register()
        {
            Game.Effect_List.Add(this);
        }
        public void Unregister()
        {
            //  remove from effect list in Game
            Game.Effect_List.Remove(this);
        }
        public int DrawId { get; set; }
        public int DrawArg { get { return count_; } }
    }
}
