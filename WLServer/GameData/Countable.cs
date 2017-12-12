namespace sis
{
    public class Countable : Updatable
    {
        protected int count_, period_;

        //  retrieve
        public int Count
        {
            get { return count_; }
        }
        public int Period
        {
            get { return period_; }
        }
        //  constructor
        public Countable(int period)
        {
            period_ = period;
        }

        public override void Update()
        {
            ++count_;
            while (count_ >= period_)
            {
                Trigger();

                count_ -= period_;
            }
        }
        public virtual void Trigger()
        {
            throw new System.Exception();
        }
    }
}
