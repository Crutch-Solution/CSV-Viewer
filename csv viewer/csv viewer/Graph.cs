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
        //public fields
        public delegate void drawGelegate();
        public drawGelegate draw;
        public Color BackColorLegend;
        public List<int> Drawable = new List<int>();

        //private fields
        static int _colorsCount = 6;
        Color[] _legendColors = new Color[] { Color.Blue, Color.Red, Color.Green, Color.DarkBlue, Color.DarkRed, Color.DarkGreen };
        SolidBrush[] _legendBrushes;
        Pen[] _legendPens;
     
        
        List<Channel> _channels = new List<Channel>();
        Bitmap _bitmap;
        Graphics _graph;
        float _maxX;
        float _maxY;
        float _minX;
        float _minY;
        float _xScale = 1;
        float _yScale = 1;

        public Graph()
        {
            InitializeComponent();
            Drawable.Clear();
            draw += drawValues;
            _legendBrushes = new SolidBrush[_colorsCount];
            _legendPens = new Pen[_colorsCount];
            for(int i = 0; i < _colorsCount; i++)
            {
                _legendBrushes[i] = new SolidBrush(_legendColors[i]);
                _legendPens[i] = new Pen(_legendColors[i]);
            }
        }
        public void addChannel(String name)
        {
            if (Drawable.Count < 5)
                Drawable.Add(_channels.Count);
            _channels.Add(new Channel(name, _legendPens[_channels.Count%_colorsCount]));

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
            _maxX = float.MinValue;
            _maxY = float.MinValue;
            _minX = float.MaxValue;
            _minY = float.MaxValue;
            foreach (var i in Drawable)
            {
                if (_channels[i].MaxX >_maxX)_maxX = _channels[i].MaxX;
                if (_channels[i].MaxY >_maxY)_maxY = _channels[i].MaxY;
                if (_channels[i].MinX <_minX)_minX = _channels[i].MinX;
                if (_channels[i].MinY <_minY)_minY = _channels[i].MinY;
            }
            if (_maxX -_minX == 0 ||_maxY -_minY == 0) return;



            _xScale = pictureBox1.Width / (_maxX -_minX);
            _yScale = pictureBox1.Height / (_maxY - _minY);



            foreach (var i in _channels)
                i.scale(_xScale, _yScale);
        }
        public void drawValues()
        {
            if (_channels.Count!=0 && _channels[0].Count < 2) return;
            scale();
            
            _graph.Clear(Color.White);
           _graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
           _graph.ResetTransform();
           _graph.ScaleTransform(1.0f, -1.0f); //flipped;
           _graph.TranslateTransform(0, -_bitmap.Height);
           _graph.TranslateTransform(-_minX * _xScale, -_minY * _yScale);

            foreach (var i in Drawable)
            {
                _channels[i].draw(ref _graph);
            }
            pictureBox1.Image =_bitmap;
           // pictureBox1.Refresh();
        }
        public void drawAxes()
        {
           _graph.ResetTransform();
           _graph.ScaleTransform(1.0f, -1.0f); //flipped;
           _graph.TranslateTransform(0, -_bitmap.Height);
           _graph.TranslateTransform(-_minX * _xScale, -_minY * _yScale);

           _graph.DrawLine(new Pen(Color.Black, 3), 0,_minY * _yScale, 0,_maxY * _yScale);
           _graph.DrawLine(new Pen(Color.Black, 3),_minX * _xScale, 0,_maxX * _xScale, 0);
            pictureBox1.Image =_bitmap;
            pictureBox1.Refresh();
        }
        public void drawLegend()
        {
           _graph.ResetTransform();
            float X=1, Y=1;
            foreach (var i in Drawable)
            {
                if(_graph.MeasureString(_channels[i].Name, new Font("Arial", 12)).Width>X)
                     X =_graph.MeasureString(_channels[i].Name, new Font("Arial", 12)).Width;
               if(_graph.MeasureString(_channels[i].Name, new Font("Arial", 12)).Height>Y)
                    Y =_graph.MeasureString(_channels[i].Name, new Font("Arial", 12)).Height;
            }
           _graph.FillRectangle(new SolidBrush(BackColorLegend), 20, 20, X, Y * Drawable.Count);
            int index = 0;
            foreach(var i in Drawable)
            {
               _graph.DrawString(_channels[i].Name, new Font("Arial", 12), _legendBrushes[i % _colorsCount], 20, 20 + 20 * index++);
            }
            pictureBox1.Image =_bitmap;
            pictureBox1.Refresh();
        }
        public void drawGrid()
        {
           _graph.ResetTransform();
           _graph.ScaleTransform(1.0f, -1.0f); //flipped;
           _graph.TranslateTransform(0, -_bitmap.Height);
           _graph.TranslateTransform(-_minX * _xScale, -_minY * _yScale);

            float xStep = (_maxX - _minX) / 10.0f * _xScale;
            for (float i =_minX * _xScale; i < 0; i += xStep)
               _graph.DrawLine(Pens.Gray, i,_minY*_yScale, i,_maxY * _yScale);
            for (float i = 0; i <_maxX * _xScale; i += xStep)
               _graph.DrawLine(Pens.Gray, i,_minY * _yScale, i,_maxY * _yScale);

            float yStep = (_maxY - _minY) / 10.0f * _yScale;
            for (float i =_minY * _yScale; i < 0; i += yStep)
               _graph.DrawLine(Pens.Gray,_minX * _xScale, i,_maxX* _xScale, i);
            for (float i = 0; i <_maxY * _yScale; i += yStep)
               _graph.DrawLine(Pens.Gray,_minX * _xScale, i,_maxX*_xScale, i);
            pictureBox1.Image =_bitmap;
            pictureBox1.Refresh();
        }


        private void Graph_Resize(object sender, EventArgs e)
        {
            try
            {
               _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                _graph = Graphics.FromImage(_bitmap);

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
           _graph.Clear(Color.White);
            pictureBox1.Image = _bitmap;
            Refresh();
        }
        
    }

}
