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
        delegate void drawGelegate();
        /// <summary>
        /// to draw legend / grid / axes at once
        /// </summary>
        drawGelegate _draw;
        public Color BackColorLegend;
        /// <summary>
        /// List of channels to be draw
        /// </summary>
        public List<int> Drawable = new List<int>();
        public int FontSize=8;

        //private fields
        static int _colorsCount = 6;
        Color[] _legendColors = new Color[] { Color.Blue, Color.Red, Color.Green, Color.DarkBlue, Color.DarkRed, Color.DarkGreen };
        SolidBrush[] _legendBrushes;
        Pen[] _legendPens;
     
        /// <summary>
        /// Main channels list
        /// </summary>
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
            _draw += drawValues;
            _legendBrushes = new SolidBrush[_colorsCount];
            _legendPens = new Pen[_colorsCount];
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            for(int i = 0; i < _colorsCount; i++)
            {
                _legendBrushes[i] = new SolidBrush(_legendColors[i]);
                _legendPens[i] = new Pen(_legendColors[i]);
            }
        }
        public Thread DrawThread = new Thread(new ThreadStart(()=> { }));
        int limit = -1;
        int _bitmapHeight, _bitmapWidth;
        /// <summary>
        /// Draw current channels 
        /// </summary>
        /// <param name="force">Kill previous draw task</param>
        public void draw(bool force)
        { 
            if (_channels.Count == 0 || _channels[0].values.Count < 2)
                return;
            _bitmapHeight = _bitmap.Height;
            _bitmapWidth = _bitmap.Width;
            limit = _channels[0].values.Count;
            lock (pictureBox1)
            {
                if (force)
                {
                    statusLabel.Text = "Updating, please wait";
                    if (DrawThread != null)
                        DrawThread.Abort();
                    DrawThread = new Thread(new ThreadStart(_draw));
                    DrawThread.Start();
                }
                else if (!DrawThread.IsAlive)
                {
                    statusLabel.Text = "Updating, please wait";
                    if (DrawThread != null)
                        DrawThread.Abort();
                    DrawThread = new Thread(new ThreadStart(_draw));
                    DrawThread.Start();
                }
            }
        }
        /// <summary>
        /// Returns list of current channels statistic
        /// </summary>
        /// <returns></returns>
        public List<string> GetStatistic()
        {
            List<string> result = new List<string>();
            foreach (var i in Drawable)
                result.Add(_channels[i].GetStatistic());
            return result;
        }
        public void addChannel(String name)
        {
            lock (Drawable)
            {
                if (Drawable.Count < 5)
                    Drawable.Add(_channels.Count);
            }

            _channels.Add(new Channel(name));

        }
        /// <summary>
        /// Name of the specific channel
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public int getChannelIndex(string channelName)
        {
            return _channels.FindIndex(x => x.Name == channelName);
        }
        /// <summary>
        /// Insert point into specific shannel
        /// </summary>
        /// <param name="channelIndex"></param>
        /// <param name="value"></param>
        public void insertInto(int channelIndex, PointF value)
        {
            _channels[channelIndex].add(value);
        }
        int offsets = 5;
        /// <summary>
        /// Calculates current scale values for Graph
        /// </summary>
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



            _xScale = (pictureBox1.Width - offsets*2) / (_maxX -_minX);
            _yScale = (pictureBox1.Height - offsets * 2) / (_maxY - _minY);



            foreach (var i in _channels)
                i.scale(_xScale, _yScale, limit);
        }
        void drawValues()
        {
            for(int i=0;i<_channels.Count;i++)
               _channels[i].recalculateStatistics(limit);
            scale();

            _graph.Clear(Color.White);
            _graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            _graph.ResetTransform();
            _graph.ScaleTransform(1.0f, -1.0f); //flipped;
            _graph.TranslateTransform(0, -_bitmapHeight);
            _graph.TranslateTransform(-_minX * _xScale + offsets, -_minY * _yScale + offsets);

            for(int i = 0; i < Drawable.Count; i++)
                _channels[Drawable[i]].draw(ref _graph, _legendPens[i%_colorsCount]);
            pictureBox1.Invoke((MethodInvoker)delegate () { pictureBox1.Image = _bitmap;
                pictureBox1.Refresh();
                statusLabel.Text = "";
            });
            // pictureBox1.Refresh();
        }
        void drawAxes()
        {
            pictureBox1.Invoke((MethodInvoker)delegate ()
            {
                _graph.ResetTransform();
                _graph.ScaleTransform(1.0f, -1.0f); //flipped;
                _graph.TranslateTransform(0, -_bitmap.Height);
                _graph.TranslateTransform(-_minX * _xScale + offsets, -_minY * _yScale + offsets);

                _graph.DrawLine(new Pen(Color.Black, 2), -1, _minY * _yScale - offsets, -1, _maxY * _yScale + offsets);
                _graph.DrawLine(new Pen(Color.Black, 2), _minX * _xScale - offsets, -1, _maxX * _xScale + offsets, -1);
                pictureBox1.Image = _bitmap;
                pictureBox1.Refresh();
            });
        }
        void drawLegend()
        {
            pictureBox1.Invoke((MethodInvoker)delegate ()
            {
                _graph.ResetTransform();
                float X = 1, Y = 1;
                foreach (var i in Drawable)
                {
                    if (_graph.MeasureString(_channels[i].Name, new Font("Arial", 12)).Width > X)
                        X = _graph.MeasureString(_channels[i].Name, new Font("Arial", 12)).Width;
                    if (_graph.MeasureString(_channels[i].Name, new Font("Arial", 12)).Height > Y)
                        Y = _graph.MeasureString(_channels[i].Name, new Font("Arial", 12)).Height;
                }
                _graph.FillRectangle(new SolidBrush(BackColorLegend), 20, 20, X, Y * Drawable.Count);
                for (int i = 0; i < Drawable.Count; i++)
                    _graph.DrawString(_channels[Drawable[i]].Name, new Font("Arial", 12), _legendBrushes[i % _colorsCount], 20, 20 + 20 * i);
                pictureBox1.Image = _bitmap;
                pictureBox1.Refresh();
            });
        }
        void drawGrid()
        {
            pictureBox1.Invoke((MethodInvoker)delegate ()
            {
                _graph.ResetTransform();
                _graph.ScaleTransform(1.0f, -1.0f); //flipped;
                _graph.TranslateTransform(0, -_bitmap.Height);
                _graph.TranslateTransform(-_minX * _xScale + offsets, -_minY * _yScale + offsets);

                float xStep = (_maxX - _minX) / 10.0f * _xScale;
                for (float i = 0; i > _minX * _xScale; i -= xStep)
                    _graph.DrawLine(Pens.Gray, i, _minY * _yScale - offsets, i, _maxY * _yScale + offsets);
                for (float i = 0; i < _maxX * _xScale; i += xStep)
                    _graph.DrawLine(Pens.Gray, i, _minY * _yScale - offsets, i, _maxY * _yScale + offsets);

                float yStep = (_maxY - _minY) / 10.0f * _yScale;
                for (float i = 0; i > _minY * _yScale; i -= yStep)
                    _graph.DrawLine(Pens.Gray, _minX * _xScale - offsets, i, _maxX * _xScale + offsets, i);
                for (float i = 0; i < _maxY * _yScale; i += yStep)
                    _graph.DrawLine(Pens.Gray, _minX * _xScale - offsets, i, _maxX * _xScale + offsets, i);

                //grid prompt
                _graph.ResetTransform();
                //_graph.ScaleTransform(1.0f, -1.0f); //flipped;
                _graph.TranslateTransform(0, +_bitmap.Height);
                _graph.TranslateTransform(-_minX * _xScale + offsets, +_minY * _yScale - offsets);

                for (float i = 0; i > _minX * _xScale; i -= xStep)
                    _graph.DrawString(Math.Round(i / _xScale, 3).ToString(), new Font("Arial", FontSize), Brushes.Gray, i, 0);
                for (float i = 0; i < _maxX * _xScale; i += xStep)
                    _graph.DrawString(Math.Round(i / _xScale, 3).ToString(), new Font("Arial", FontSize), Brushes.Gray, i, 0);


                for (float i = 0; i > _minY * _yScale; i -= yStep)
                    _graph.DrawString(Math.Round(i / _yScale, 3).ToString(), new Font("Arial", FontSize), Brushes.Gray, 0, -i);
                for (float i = 0; i < _maxY * _yScale; i += yStep)
                    _graph.DrawString(Math.Round(i / _yScale, 3).ToString(), new Font("Arial", FontSize), Brushes.Gray, 0, -i);
                ////

                pictureBox1.Image = _bitmap;
                pictureBox1.Refresh();
            });
        }


        private void Graph_Resize(object sender, EventArgs e)
        {
            try
            {
                _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                _graph = Graphics.FromImage(_bitmap);

                scale();
                draw(true);

            }
            catch(Exception ex)
            {

            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                _draw += drawGrid;
            else
                _draw -= drawGrid;
            draw(true);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                _draw += drawAxes;
            else
                _draw -= drawAxes;
            draw(true);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox3.Checked)
                _draw += drawLegend;
            else
                _draw -= drawLegend;
            draw(true);
        }
        /// <summary>
        /// clear everything
        /// </summary>
        public void clear()
        {
            _channels.Clear();
            _graph.Clear(Color.White);
            Drawable.Clear();
            pictureBox1.Image = _bitmap;
            Refresh();
        }
        
    }

}
