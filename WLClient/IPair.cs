using System;

namespace sis
{
    public class IPair
    {
        public long X { get; set; }
        public long Y { get; set; }
        //  constructor
        public IPair() : this(0, 0) { ; }
        public IPair(long x, long y)
        {
            X = x;
            Y = y;
        }
        public IPair(IPair pair) : this(pair.X, pair.Y) { ; }
        //  copy
        public IPair Clone()
        {
            return new IPair(this);
        }
        public void Clone(IPair pair)
        {
            X = pair.X;
            Y = pair.Y;
        }
        //  operator
        public static IPair operator-(IPair pair1, IPair pair2)
        {
            return new IPair(pair1.X - pair2.X, pair1.Y - pair2.Y);
        }
        public bool Equal(IPair pair)
        {
            return X == pair.X && Y == pair.Y;
        }
        public bool Zero()
        {
            return X == 0 && Y == 0;
        }
        public void Add(IPair pair)
        {
            X += pair.X;
            Y += pair.Y;
        }
        public void Sub(IPair pair)
        {
            X -= pair.X;
            Y -= pair.Y;
        }
        public IPair Mul(long scale)
        {
            X *= scale;
            Y *= scale;
            return this;
        }
        public void Square()
        {
            X *= X;
            Y *= Y;
        }
        public void SquareRoot()
        {
            //  suppose X and Y >= 0
            X = (long)Math.Sqrt((double)X);
            Y = (long)Math.Sqrt((double)Y);
        }
        //  vector
        public long LengthSquare()
        {
            return X * X + Y * Y;
        }
        public long Length()
        {
            return (long)Math.Sqrt((double)LengthSquare());
        }
        public void ChangeLength(long new_length)
        {
            //  不改編方向的前提下改變長度
            //  new_vector^2 : old_vector^2 = new_length^2 : old_length^2
            MulDivEx(new_length * new_length);
        }
        //  short cut
        public void MulDiv(long param1, long param2) {
            X = X * param1 / param2;
            Y = Y * param1 / param2;
        }
        public void MulDivEx(long param)
        {
            //  result^2 : cur^2 = param : |cur|^2
            bool sign_x = X < 0, sign_y = Y < 0;
            Square();
            //if (X * param < 0) throw new System.Exception();
            //if (Y * param < 0) throw new System.Exception();
            MulDiv(param, X + Y);
            SquareRoot();
            if (sign_x == true) X = -X;
            if (sign_y == true) Y = -Y;
        }
    }
}
