using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace csv_viewer
{
    class Channel
    {
        public List<PointF> values = new List<PointF>();
       // public List<PointF> scaled = new List<PointF>();

        public string Name;
        public int Count, NaNs, Valid;
        public float Avg, MinX, MaxX, MinY, MaxY;
        public Channel(String name) { Name = name; }

        public void add(PointF val)
        {
            values.Add(val);
        }

        public void recalculateStatistics(int limit)
        {
            Avg = 0;
            MinX = float.MaxValue;
            MaxX = float.MinValue;
            MinY = float.MaxValue;
            MaxY = float.MinValue;
            Valid = 0;
            float X, Y;
            for (int i = 0; i < limit && i < values.Count; i++)
                if (!float.IsNaN(values[i].Y))
                {
                    Valid++;
                    X = values[i].X;
                    Y = values[i].Y;

                    Avg += Y;
                    if (MinX > X) MinX = X;
                    if (MinY > Y) MinY = Y;
                    if (MaxX < X) MaxX = X;
                    if (MaxY < Y) MaxY = Y;
                }
            Avg /= (Valid * 1.0f);
            Count = values.Count;
            NaNs = Count - Valid;
        }
        public void draw(ref Graphics graph, Pen pen, int width)
        {
            if (NaNs == Count)
                return;
            int step;
            if (width < values.Count)
                step = (int)(values.Count / (width * 1.0f));
            else
                step = 1;
            if (values.Count > 1)
            {
                if (NaNs == 0)
                {
                    for (int i = 0; i < values.Count - step; i += step)
                        graph.DrawLine(pen, values[i], values[i + step]);
                }
                else
                {
                    for (int i = 0; i < values.Count - step; i += step)
                        if (!float.IsNaN(values[i].Y) && !float.IsNaN(values[i + step].Y))
                            graph.DrawLine(pen, values[i], values[i + step]);
                }
            }
        }
        public string GetStatistic()
        {
            return $"Channel: {Name}. Count = {Count} (NaNs = {NaNs}, Valid={Valid}); Avg = {Avg}; Range = [{MinY}...{MaxY}]";
        }
    }
}
