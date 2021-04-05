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
        float _period;
        public Sin(float period)
        {
            _period = period;
        }
        public double getValue(float x)
        {
            double result = Math.Round(Math.Sin(x* _period) * _period, 3);
            return result;
        }
    }
    class Cos : Generator
    {
        float _period;
        public Cos(float period)
        {
            _period = period;
        }
        public double getValue(float x)
        {
            double result = Math.Round(Math.Cos(x *_period), 3);
            return result;
        }
    }
    class Saw : Generator
    {
        float _period;
        public Saw(float period)
        {
            _period = period;
        }
        public double getValue(float x)
        {
            double result = Math.Round(Math.Cos(x * _period) * Math.Sin(x* _period) * _period, 3);
            return result;
        }
    }
}
