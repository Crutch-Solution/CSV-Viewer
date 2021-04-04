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
            lock (values)
            {
                values.Add(val);
            }
        }

        public void recalculateStatistics()
        {
            lock (values)
            {
                var clearList = values.FindAll(x => !float.IsNaN(x.Y));
                Avg = values.Average(x => x.Y);
                MinX = clearList.Min(x => x.X);
                MaxX = clearList.Max(x => x.X);
                MinY = clearList.Min(x => x.Y);
                MaxY = clearList.Max(x => x.Y);
                Count = values.Count;
                NaNs = Count - clearList.Count;
                Valid = clearList.Count;
            }
        }
        public void scale(float X, float Y)
        {
            lock (values)
            {
                scaled = new List<PointF>();
                foreach (var i in values)
                    scaled.Add(new PointF(i.X * X, i.Y * Y));
            }
        }
        public void draw(ref Graphics graph, Pen pen)
        {
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
