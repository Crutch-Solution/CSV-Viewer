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
    class Channel
    {
        public List<PointF> values = new List<PointF>();
        public string Name;
        int count, NaNs; double avg, min, max;
        public Channel(String name) { Name = name; }
        public void add(PointF val)
        {
            values.Add(val);
            recalculateStatistics();
        }
        void recalculateStatistics()
        {
            avg = values.Average(x=>x.Y);
            min = values.Min(x=>x.Y);
            max = values.Max(x => x.Y);
            count = values.Count;
            NaNs = values.Count(x => double.IsNaN(x.Y));
        }
    }
    public partial class Graph : UserControl
    {
        List<Channel> _channels = new List<Channel>();
        [Browsable(true)]
        public Color BackColorLegend
        {
            get { return label1.BackColor; }
            set { label1.BackColor = value; }
        }
        Bitmap bitmap;
        Graphics graph;
        public Graph()
        {
            InitializeComponent();
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            graph = Graphics.FromImage(bitmap);
            pictureBox1.Image = bitmap;
            graph.ScaleTransform(1.0F, -1.0F);
            graph.TranslateTransform(0, -bitmap.Height);
        }
        public void addChannel(String name)
        {
            _channels.Add(new Channel(name));
        }
        public int getChannelIndex(string channelName)
        {
            return _channels.FindIndex(x => x.Name == channelName);
        }
        public void insertInto(int channelIndex, double value)
        {
            _channels[channelIndex].add(value);
        }
        public void draw()
        {
            foreach(var i in _channels){
                foreach(var j in i.values)
                {
                    graph.DrawLines()
                }
            }
        }
    }
}
