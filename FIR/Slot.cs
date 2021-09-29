namespace FIR
{
    public class Slot
    {
        public const int LEFT = 0;
        public const int UPPER_LEFT = 1;
        public const int UPPER = 2;
        public const int UPPER_RIGHT = 3;
        public const int RIGHT = 4;
        public const int BOTTOM_RIGHT = 5;
        public const int BOTTOM = 6;
        public const int BOTTOM_LEFT = 7;
        public Slot(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
        public Slot UpperLeft { get { return new Slot(X - 1, Y - 1); } }
        public Slot Upper { get { return new Slot(X, Y - 1); } }
        public Slot UpperRight { get { return new Slot(X + 1, Y - 1); } }
        public Slot Left { get { return new Slot(X - 1, Y); } }
        public Slot Right { get { return new Slot(X + 1, Y); } }
        public Slot BottomLeft { get { return new Slot(X - 1, Y + 1); } }
        public Slot Bottom { get { return new Slot(X, Y + 1); } }
        public Slot BottomRight { get { return new Slot(X + 1, Y + 1); } }
        public Slot GetNextAtDirection(int direction)
        {
            switch (direction)
            {
                case 0:
                    return Left;
                case 1:
                    return UpperLeft;
                case 2:
                    return Upper;
                case 3:
                    return UpperRight;
                case 4:
                    return Right;
                case 5:
                    return BottomRight;
                case 6:
                    return Bottom;
                case 7:
                    return BottomLeft;
            }
            return this;
        }
        public Slot GetTimesAtDirection(int direction, int times)
        {
            Slot tmp = this;
            for (int i=0;i<times;i++)
            {
                tmp = tmp.GetNextAtDirection(direction);
            }
            return tmp;
        }
    }
}
