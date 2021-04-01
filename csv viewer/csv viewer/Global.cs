using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csv_viewer
{
    class Global
    {
        static Global()
        {
            for(int i = 0; i < Colors; i++)
            {
                LegendBrushes[i] = new SolidBrush(LegendColors[i]);
                LegendPens[i] = new Pen(LegendColors[i]);
            }

        }
        public static object obj = new object();
        public static int Colors = 6;
        public static Color[] LegendColors = new Color[] { Color.Blue, Color.Red, Color.Green, Color.DarkBlue, Color.DarkRed, Color.DarkGreen };
        public static SolidBrush[] LegendBrushes = new SolidBrush[Colors];
        public static Pen[] LegendPens = new Pen[Colors];
        public static List<int> drawable = new List<int>();
    }
}
