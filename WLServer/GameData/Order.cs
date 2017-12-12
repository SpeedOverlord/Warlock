namespace sis
{
    public class Order
    {
        public int type_;
        IPair param1_, param2_;
        /*
         *  type =  0 (stop)
         *          1 (move)
         *              param1 = destination
         *          2 (cast spell)
         *              param1 = destination
         *              param2 = spell id/count
         *              param3 = spell effect x2
         * 
         */
        //  retrieve
        public Effect[] EffectList = null;
        public IPair Param1
        {
            get { return param1_; }
        }
        public IPair Param2
        {
            get { return param2_; }
        }
        public int Type
        {
            get { return type_; }
        }
        //  constructor
        public Order()
        {
            type_ = 0;

            param1_ = new IPair();
            param2_ = new IPair();
        }
        //  operator
        public void Stop()
        {
            type_ = 0;
        }
        public void Move(IPair destination)
        {
            param1_.Clone(destination);

            type_ = 1;
        }
        public void Cast(int id, IPair destination)
        {
            param1_.Clone(destination);
            
            param2_.X = id;
            param2_.Y = 0;

            type_ = 2;
        }
        public bool Casting()
        {
            return type_ == 2;
        }
        public void CastCount()
        {
            ++param2_.Y;
        }
    }
}
