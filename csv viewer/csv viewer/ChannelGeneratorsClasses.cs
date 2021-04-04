using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csv_viewer
{
    interface Generator
    {
        double getValue(float x);
    }
    class Sin : Generator
    {
        public double getValue(float x)
        {
            return Math.Sin(x);
        }
    }
    class Cos : Generator
    {
        public double getValue(float x)
        {
            return Math.Cos(x);
        }
    }
    class Saw : Generator
    {
        public double getValue(float x)
        {
            if((x/3)%2 == 0)
            {
                return x - (x / 3) * 3;
            }
            else
            {
                return (x - (x / 3) * 3)*5;
            }
        }
    }
}
