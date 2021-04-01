using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Threading;

namespace csv_viewer
{

    public partial class Graph : UserControl
    {
        internal delegate void drawGelegate();
        internal drawGelegate draw;
        List<Channel> _channels = new List<Channel>();
        [Browsable(true)]
        public Color BackColorLegend;
        Bitmap bitmap;
        Graphics graph;
        float maxX, maxY, minX, minY;
        float Xscale = 1;
        float Yscace = 1;
        public Graph()
        {
            InitializeComponent();
            Global.drawable.Clear();
            draw += drawValues;
        }
        public void addChannel(String name)
        {
            _channels.Add(new Channel(name, Global.LegendPens[_channels.Count%Global.Colors]));

        }
        public int getChannelIndex(string channelName)
        {
            return _channels.FindIndex(x => x.Name == channelName);
        }
        public void insertInto(int channelIndex, PointF value)
        {
            _channels[channelIndex].add(value);
        }
        public void scale()
        {
            maxX = float.MinValue; maxY = float.MinValue; minX = float.MaxValue; minY = float.MaxValue;
            foreach (var i in Global.drawable)
            {
                if (_channels[i].maxX > maxX) maxX = _channels[i].maxX;
                if (_channels[i].maxY > maxY) maxY = _channels[i].maxY;
                if (_channels[i].minX < minX) minX = _channels[i].minX;
                if (_channels[i].minY < minY) minY = _channels[i].minY;
            }
            if (maxX - minX == 0 || maxY - minY == 0) return;



            Xscale = pictureBox1.Width / (maxX - minX);
            Yscace = pictureBox1.Height / (maxY - minY);



            foreach (var i in _channels)
                i.scale(Xscale, Yscace);
        }
        public void drawValues()
        {
            graph.Clear(Color.White);
            graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graph.ResetTransform();
            graph.ScaleTransform(1.0f, -1.0f); //flipped;
            graph.TranslateTransform(0, -bitmap.Height);
            graph.TranslateTransform(-minX * Xscale, -minY * Yscace);

            foreach (var i in Global.drawable)
            {
                _channels[i].draw(ref graph);
            }
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
        }
        public void drawAxes()
        {
            graph.ResetTransform();
            graph.ScaleTransform(1.0f, -1.0f); //flipped;
            graph.TranslateTransform(0, -bitmap.Height);
            graph.TranslateTransform(-minX * Xscale, -minY * Yscace);

            graph.DrawLine(new Pen(Color.Black, 3), 0, minY * Yscace, 0, maxY * Yscace);
            graph.DrawLine(new Pen(Color.Black, 3), minX * Xscale, 0, maxX * Xscale, 0);
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
        }
        public void drawLegend()
        {
            graph.ResetTransform();
            float X=1, Y=1;
            foreach (var i in Global.drawable)
            {
                if(graph.MeasureString(_channels[i].Name, new Font("Arial", 12)).Width>X)
                     X = graph.MeasureString(_channels[i].Name, new Font("Arial", 12)).Width;
               if(graph.MeasureString(_channels[i].Name, new Font("Arial", 12)).Height>Y)
                    Y = graph.MeasureString(_channels[i].Name, new Font("Arial", 12)).Height;
            }
            graph.FillRectangle(new SolidBrush(BackColorLegend), 20, 20, X, Y * Global.drawable.Count);
            int index = 0;
            foreach(var i in Global.drawable)
            {
                graph.DrawString(_channels[i].Name, new Font("Arial", 12), Global.LegendBrushes[i % Global.Colors], 20, 20 + 20 * index++);
            }
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
        }
        public void drawGrid()
        {
            graph.ResetTransform();
            graph.ScaleTransform(1.0f, -1.0f); //flipped;
            graph.TranslateTransform(0, -bitmap.Height);
            graph.TranslateTransform(-minX * Xscale, -minY * Yscace);

            float xStep = (maxX - minX) / 10.0f * Xscale;
            for (float i = minX * Xscale; i < 0; i += xStep)
                graph.DrawLine(Pens.Gray, i, minY*Yscace, i, maxY * Yscace);
            for (float i = 0; i < maxX * Xscale; i += xStep)
                graph.DrawLine(Pens.Gray, i, minY * Yscace, i, maxY * Yscace);

            float yStep = (maxY - minY) / 10.0f * Yscace;
            for (float i = minY * Yscace; i < 0; i += yStep)
                graph.DrawLine(Pens.Gray, minX * Xscale, i, maxX* Xscale, i);
            for (float i = 0; i < maxY * Yscace; i += yStep)
                graph.DrawLine(Pens.Gray, minX * Xscale, i, maxX*Xscale, i);
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
        }
        public void setChannelsNames(List<string> names)
        {

            Global.drawable.Clear();
            _channels = new List<Channel>();
            int colorIndex = 0;
            foreach (var i in names)
            {
                _channels.Add(new Channel(i, Global.LegendPens[colorIndex++ % Global.Colors]));
            }

            draw();
        }
        public void setChannelsValues(List<List<PointF>> values)
        {
            // Thread.Sleep(1000);
            for (int i = 0; i < _channels.Count; i++)
            {
                _channels[i].values.AddRange(values[i]);
                _channels[i].recalculateStatistics();
            }
            scale();
            draw();

        }
        private void Graph_Resize(object sender, EventArgs e)
        {
            try
            {
                bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                graph = Graphics.FromImage(bitmap);

                scale();
                draw();

            }
            catch(Exception ex)
            {

            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                draw += drawGrid;
            else
                draw -= drawGrid;
            draw();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                draw += drawAxes;
            else
                draw -= drawAxes;
            draw();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox3.Checked)
                draw += drawLegend;
            else
                draw -= drawLegend;
            draw();
        }
        public void clear()
        {
            _channels.Clear();
            graph.Clear(Color.White);
            pictureBox1.Image = bitmap;
            Refresh();
        }
        public class Channel
        {
            public List<PointF> values = new List<PointF>();
            List<PointF> scaled = new List<PointF>();

            public string Name;
            public int count, NaNs;
            public float avg, minX, maxX, minY, maxY;
            public Pen pen;
            public Channel(String name, Pen pen) { Name = name; this.pen = pen; }
            public void add(PointF val)
            {
                values.Add(val);
                recalculateStatistics();
            }
            public void recalculateStatistics()
            {
                avg = values.Average(x => x.Y);
                minX = values.Min(x => x.X);
                maxX = values.Max(x => x.X);
                minY = values.Min(x => x.Y);
                maxY = values.Max(x => x.Y);
                count = values.Count;
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

}
