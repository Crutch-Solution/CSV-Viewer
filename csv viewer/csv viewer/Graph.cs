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

            graph.ResetTransform();

            graph.Clear(Color.White);
            graph.ScaleTransform(1.0f, -1.0f); //flipped;
            graph.TranslateTransform(0, -bitmap.Height);

            float Xscale = pictureBox1.Width / (maxX - minX);
            float Yscace = pictureBox1.Height / (maxY - minY);
            //graph.ScaleTransform(Xscale, Yscace); //useless, transfrom linewidth aswell
            //graph.TranslateTransform(-minX, -minY);
            graph.TranslateTransform(-minX * Xscale, -minY * Yscace);
            foreach (var i in _channels)
                i.scale(Xscale, Yscace);
        }
        public void drawValues()
        {
            foreach (var i in Global.drawable)
            {
                _channels[i].draw(ref graph);
            }
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
        }
        public void drawAxes()
        {
            graph.DrawLine(new Pen(Color.Black, 3), 0, minY, 0, maxY);
            graph.DrawLine(new Pen(Color.Black, 3), minX, 0, maxX, 0);
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
        }
        public void drawLegend()
        {
            graph.ResetTransform();
            graph.FillRectangle(new SolidBrush(BackColorLegend), 20, 20, 200, 20 * _channels.Count);
            for (int i = 0; i < _channels.Count; i++)
            {
                graph.DrawString(_channels[i].Name, new Font("Arial", 12), Global.LegendBrushes[i % Global.Colors], 20, 20 + 20 * i);
            }
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
        }
        public void drawGrid()
        {
            float xStep = (maxX - minX) / 10.0f;
            for (float i = minX; i < 0; i += xStep)
                graph.DrawLine(Pens.Gray, i, minY, i, maxY);
            for (float i = 0; i < maxX; i += xStep)
                graph.DrawLine(Pens.Gray, i, minY, i, maxY);

            float yStep = (maxY - minY) / 10.0f;
            for (float i = minY; i < 0; i += yStep)
                graph.DrawLine(Pens.Gray, minX, i, maxX, i);
            for (float i = 0; i < maxY; i += yStep)
                graph.DrawLine(Pens.Gray, minX, i, maxX, i);
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
                if(colorIndex<5)
                    Global.drawable.Add(colorIndex);
                _channels.Add(new Channel(i, Global.LegendPens[colorIndex++ % Global.Colors]));
            }

            draw();
        }
        public void setChannelsValues(List<List<PointF>> values)
        {
            for (int i = 0; i < _channels.Count; i++)
                for (int j = 0; j < values[i].Count; j++)
                    _channels[i].add(values[i][j]);
            scale();
            draw();
        }
        private void Graph_Resize(object sender, EventArgs e)
        {
            try
            {
                bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                graph = Graphics.FromImage(bitmap);
                graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                pictureBox1.Image = bitmap;
                graph.ScaleTransform(1.0F, -1.0F);
                graph.TranslateTransform(0, -bitmap.Height);
                scale();
                draw();
            }
            catch(Exception ex)
            {

            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
                draw += drawGrid;
            else
                draw -= drawGrid;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
                draw += drawAxes;
            else
                draw -= drawAxes;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox3.Checked)
                draw += drawLegend;
            else
                draw -= drawLegend;
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
            void recalculateStatistics()
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
