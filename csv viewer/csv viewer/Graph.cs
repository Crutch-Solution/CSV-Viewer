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
         
        }
        public void addChannel(String name)
        {
            _channels.Add(new Channel(name, Global.LegendPens[_channels.Count%Global.Colors]));
            draw();
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
            graph.Clear(Color.White);
            for (int i = 0; i < _channels.Count; i++)
            {
                _channels[i].draw(ref graph);
            }

            graph.ResetTransform();
            graph.FillRectangle(new SolidBrush(BackColorLegend), 20, 20, 200, 20 * _channels.Count);
            
            for (int i = 0; i < _channels.Count; i++)
            {
                graph.DrawString(_channels[i].Name, new Font("Arial", 12), Global.LegendBrushes[i % Global.Colors], 20,20+20*i );
            }

            graph.ScaleTransform(1.0F, -1.0F);
            graph.TranslateTransform(0, -bitmap.Height);

            graph.DrawEllipse(Pens.Red, 100, 100, 30, 30);
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
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
    }
    class Channel
    {
        public List<PointF> values = new List<PointF>();
        public string Name;
        int count, NaNs; double avg, min, max;
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
            min = values.Min(x => x.Y);
            max = values.Max(x => x.Y);
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
