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
        public int Count, NaNs;
        public float Avg, MinX, MaxX, MinY, MaxY;
        public Pen pen;
        public Channel(String name, Pen pen) { Name = name; this.pen = pen; }
        public void add(PointF val)
        {
            values.Add(val);
            recalculateStatistics();
        }

        public void recalculateStatistics()
        {
            Avg = values.Average(x => x.Y);
            MinX = values.Min(x => x.X);
            MaxX = values.Max(x => x.X);
            MinY = values.Min(x => x.Y);
            MaxY = values.Max(x => x.Y);
            Count = values.Count;
            NaNs = values.Count(x => double.IsNaN(x.Y));
        }
        public void scale(float X, float Y)
        {
            scaled = new List<PointF>();
            foreach (var i in values)
                scaled.Add(new PointF(i.X * X, i.Y * Y));
        }
        public void draw(ref Graphics graph)
        {
            if (values.Count > 1)
                graph.DrawLines(pen, scaled.ToArray());
        }

    }
}
