using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csv_viewer
{
    class Channel
    {
        public List<PointF> values = new List<PointF>();
        List<PointF> scaled = new List<PointF>();

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
            List<PointF> clearList = new List<PointF>();
            for (int i = 0; i < limit && i< values.Count; i++)
                if (!float.IsNaN(values[i].Y))
                    clearList.Add(values[i]);
            Count = values.Count;
            NaNs = Count - clearList.Count;
            if (NaNs == Count)
                return;
            Avg = clearList.Average(x => x.Y);
            MinX = clearList.Min(x => x.X);
            MaxX = clearList.Max(x => x.X);
            MinY = clearList.Min(x => x.Y);
            MaxY = clearList.Max(x => x.Y);
            Valid = clearList.Count;
        }
        public void scale(float X, float Y, int limit)
        {
            if (NaNs == Count)
                return;
            scaled = new List<PointF>();
            for (int i = 0; i < limit; i++)
            {
                if (float.IsNaN(values[i].Y))
                    scaled.Add(new PointF(values[i].X * X, float.NaN));
                else
                    scaled.Add(new PointF(values[i].X * X, values[i].Y * Y));
            }
        }
        public void draw(ref Graphics graph, Pen pen)
        {
            if (NaNs == Count)
                return;
            if (values.Count > 1)
            {
                if (NaNs == 0)
                {
                    graph.DrawLines(pen, scaled.ToArray());
                }
                else
                {
                    for(int i=0;i< scaled.Count-1; i++)
                        if(!float.IsNaN(scaled[i].Y) && !float.IsNaN(scaled[i+1].Y))
                            graph.DrawLine(pen, scaled[i], scaled[i+1]);
                }
              
            }
        }
        public string GetStatistic()
        {
            return $"Channel: {Name}. Count = {Count} (NaNs = {NaNs}, Valid={Valid}); Avg = {Math.Round(Avg, 3)}; Range = [{Math.Round(MinY, 3)}...{Math.Round(MaxY, 3)}]";
        }
    }
}
