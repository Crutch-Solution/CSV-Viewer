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
        List<Channel> _channels = new List<Channel>();
        [Browsable(true)]
        public Color BackColorLegend;
        Bitmap bitmap;
        Graphics graph;

        public Graph()
        {
            InitializeComponent();
            Global.drawable.Clear();
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
        public void draw()
        {
            float maxX = float.MinValue, maxY = float.MinValue, minX = float.MaxValue, minY = float.MaxValue;
           foreach(var i in Global.drawable)
            {
                if (_channels[i].maxX > maxX) maxX = _channels[i].maxX;
                if (_channels[i].maxY > maxY) maxY = _channels[i].maxY;
                if (_channels[i].minX < minX) minX = _channels[i].minX;
                if (_channels[i].minY < minY) minY = _channels[i].minY;
            }
            if (maxX - minX == 0 || maxY - minY == 0) return;
            //graph.ScaleTransform(1.0F, -1.0F);
             //maxX += 10;
             //maxY += 10;
             //minX += 10;
             //minY += 10;
            //
            graph.ResetTransform();

            graph.Clear(Color.White);
            graph.ScaleTransform(1.0f, -1.0f); //flipped;
            graph.TranslateTransform(0, -bitmap.Height);

            float Xscale = pictureBox1.Width / (maxX - minX), 
                Yscace = pictureBox1.Height / (maxY - minY);
            graph.ScaleTransform(Xscale, Yscace); //scaled
            graph.TranslateTransform(-minX, -minY);
            //  graph.TranslateTransform(-minX / Xscale, (pictureBox1.Height + minY) / Yscace); //transformed

            //graph.FillEllipse(Brushes.Red, minX - 4, minY - 4, 8, 8);
            //graph.FillEllipse(Brushes.Red, minX - 4, maxY - 4, 8, 8);
            //graph.FillEllipse(Brushes.Red, maxX - 4, minY - 4, 8, 8);
            //graph.FillEllipse(Brushes.Red, maxX - 4, maxY - 4, 8, 8);
            //graph.FillEllipse(Brushes.Red, (maxX + minX) / 2 - 4, (maxY + minY) / 2 - 4, 8, 8);
            // pictureBox1.Image = bitmap;
            // pictureBox1.Refresh();
            //return;
            //graph.FillRectangle(Brushes.Red, minX-10, minY-10, 20, 20);
            //graph.ScaleTransform(pictureBox1.Width / (maxX - minY), -pictureBox1.Height / (maxY - minY));
            //graph.TranslateTransform(-minX, pictureBox1.Height / (pictureBox1.Height / (maxY - minY)) + minY / (pictureBox1.Height / (maxY - minY)));
            foreach (var i in Global.drawable)
            {
                _channels[i].draw(ref graph);
            }
            if (checkBox1.Checked)
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
            }
            if (checkBox2.Checked)
            {
                    graph.DrawLine(new Pen(Color.Black, 3), 0, minY, 0, maxY);
                    graph.DrawLine(new Pen(Color.Black, 3), minX, 0, maxX,0);
            }
            if (checkBox3.Checked)
            {
                graph.ResetTransform();
                graph.FillRectangle(new SolidBrush(BackColorLegend), 20, 20, 200, 20 * _channels.Count);
                for (int i = 0; i < _channels.Count; i++)
                {
                    graph.DrawString(_channels[i].Name, new Font("Arial", 12), Global.LegendBrushes[i % Global.Colors], 20, 20 + 20 * i);
                }
            }
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
        }
        public void setChannesl(List<Channel> list)
        {
            _channels = list;
            if (Global.drawable.Count == 0)
            {
                for (int i = 0; i < 5 && i < _channels.Count; i++)
                    Global.drawable.Add(i);
            }
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
                draw();
            }
            catch(Exception ex)
            {

            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            draw();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            draw();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            draw();
        }
    }
    public class Channel
    {
        public List<PointF> values = new List<PointF>();
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
        public void draw(ref Graphics graph)
        {
            if(values.Count>1)
                graph.DrawLines(pen, values.ToArray());
        }
    }
}
