namespace sis
{
    public class RegisterList<Type>
    {
        int length_, count_;
        Type[] list_;
        bool[] used_;

        //  retrieve
        public int Length
        {
            get { return length_; }
        }
        public int Count
        {
            get { return count_; }
        }
        public Type[] List
        {
            get { return list_; }
        }
        public bool[] Used
        {
            get { return used_; }
        }
        public Type this[int index]
        {
            get { return list_[index]; }
            set { list_[index] = value; }
        }
        //  constructor
        public RegisterList(int length)
        {
            list_ = new Type[length];
            used_ = new bool[length];
            length_ = length;
            count_ = 0;
        }
        //  operator
        public int Register(Type data)
        {
            for (int i = 0; i < length_; ++i)
            {
                if (used_[i] == false)
                {
                    used_[i] = true;
                    list_[i] = data;
                    ++count_;
                    return i;
                }
            }
            return length_;
        }
        public void Unregister(int index)
        {
            if (used_[index] == false) return;
            --count_;
            used_[index] = false;
        }
    }
}
