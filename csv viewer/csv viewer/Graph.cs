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
using System.Security.Permissions;
using System.Diagnostics;
namespace csv_viewer
{

    public partial class Graph : UserControl
    {
        //public fields
        public Color BackColorLegend;
        public int MinGridStep = 2;
        public int FontSize = 8;
        public Thread DrawThread = new Thread(new ThreadStart(() => { }));

        //private fields
        delegate void _drawGelegate();
        static object _accessGraphicsLock = new object();
        static object _accessChannelsLock = new object();

        /// <summary>
        /// to draw legend / grid / axes at once
        /// </summary>
        _drawGelegate _draw;

        /// <summary>
        /// List of channels to be draw
        /// </summary>
        List<int> _drawable = new List<int>();
        /// <summary>
        /// Length of colors array (requerement)
        /// </summary>
        static int _colorsCount = 6;
        /// <summary>
        /// Array of colors for graph display
        /// </summary>
        Color[] _legendColors = new Color[] { Color.Blue, Color.Red, Color.Green, Color.DarkBlue, Color.DarkRed, Color.DarkGreen };
        /// <summary>
        /// Array of brushes to display channel names in legend
        /// </summary>
        SolidBrush[] _legendBrushes;
        /// <summary>
        /// Array of Pens to display channel graphs
        /// </summary>
        Pen[] _legendPens;

        /// <summary>
        /// Main channels list
        /// </summary>
        List<Channel> _channels = new List<Channel>();

        //graphics attributes
        Bitmap _bitmap;
        Graphics _graph;
        float _maxX;
        float _maxY;
        float _minX;
        float _minY;
        float _xScale = 1;
        float _yScale = 1;
        Point _mousePosition;
        Bitmap _clone;
        int limit = -1;
        int _bitmapHeight, _bitmapWidth;
        int _offsets = 1;
        bool _recalculateNeeded;


        public void AddDrawable(int item)
        {
            _drawable.Add(item);
            _recalculateNeeded = true;
        }
        public void ClearDrawable()
        {
            _drawable.Clear();
        }

     

 
        public Graph()
        {
            InitializeComponent();

            _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            _graph = Graphics.FromImage(_bitmap);

            _drawable.Clear();
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
     

     
        /// <summary>
        /// Returns list of current channels statistic
        /// </summary>
        /// <returns></returns>
        public List<string> GetStatistic()
        {
            lock (_accessChannelsLock)
            {
                List<string> result = new List<string>();
                foreach (var i in _drawable)
                    result.Add(_channels[i].GetStatistic());
                return result;
            }
        }
        public void addChannel(String name)
        {
            lock (_accessChannelsLock)
            {

                    if (_drawable.Count < 5)
                        _drawable.Add(_channels.Count);
                    _channels.Add(new Channel(name));

            }
        }
        /// <summary>
        /// Name of the specific channel
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public int getChannelIndex(string channelName)
        {
            lock (_accessChannelsLock)
            {

                    return _channels.FindIndex(x => x.Name == channelName);
            }
        }
        /// <summary>
        /// Insert point into specific shannel
        /// </summary>
        /// <param name="channelIndex"></param>
        /// <param name="value"></param>
        public void insertInto(int channelIndex, PointF value)
        {
            lock (_accessChannelsLock)
            {

                    _channels[channelIndex].add(value);
                    _recalculateNeeded = true;

            }
        }

        /// <summary>
        /// Calculates current scale values for Graph
        /// </summary>
        public void scale()
        {
            _maxX = float.MinValue;
            _maxY = float.MinValue;
            _minX = float.MaxValue;
            _minY = float.MaxValue;

            foreach (var i in _drawable)
            {
                if (_channels[i].MaxX >_maxX)_maxX = _channels[i].MaxX;
                if (_channels[i].MaxY >_maxY)_maxY = _channels[i].MaxY;
                if (_channels[i].MinX <_minX)_minX = _channels[i].MinX;
                if (_channels[i].MinY <_minY)_minY = _channels[i].MinY;
            }
            if (_maxX -_minX == 0 ||_maxY -_minY == 0) return;

            _xScale = (_bitmapWidth - _offsets * 2) / (_maxX -_minX);
            _yScale = (_bitmapHeight - _offsets * 2) / (_maxY - _minY);

            foreach (var i in _legendPens)
            {
                i.ResetTransform();
                i.ScaleTransform(1 / _xScale, 1 / _yScale);
            }


        }
        /// <summary>
        /// Draw current channels 
        /// </summary>
        /// <param name="force">Kill previous draw task</param>

        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public void draw(bool force)
        {
            lock (_accessChannelsLock)
            {
                if (_channels.Count == 0 || _channels[0].values.Count < 2)
                    return;
            }



            if (force || !DrawThread.IsAlive)
            {
                if (DrawThread != null)
                {
                    DrawThread.Abort();
                    try
                    {
                        DrawThread.Join();
                    }
                    catch (Exception ex) { }
                    DrawThread = null;
                }
                else
                    return;
                DrawThread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        _draw();
                    }
                    catch (Exception ex)
                    {

                    }
                }));
            }
            else
            {
                return;
            }


            _draw -= Refresher;
            _draw += Refresher;

            limit = _channels[0].values.Count;

            //clear graphics, update status
            lock (_accessGraphicsLock)
            {

                _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                _graph = Graphics.FromImage(_bitmap);
                _bitmapHeight = _bitmap.Height;
                _bitmapWidth = _bitmap.Width;


                _graph.Clear(Color.White);
                _graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                statusLabel.Text = "Updating";

            }

            _scaleNeeded = true;
            DrawThread.Start();
        }
        bool _scaleNeeded = false;
        /// <summary>
        /// Display channel graphs
        /// </summary>
        void drawValues()
        {

            if (_recalculateNeeded)
            {
                for (int i = 0; i < _channels.Count; i++)
                    _channels[i].recalculateStatistics(limit);
                _recalculateNeeded = false;
            }
            if (_scaleNeeded)
            {
                scale();
                _scaleNeeded = false;
            }
            if (_xScale == 0 || _yScale == 0) return;
            lock (_accessGraphicsLock)
            {
                //invalid value exception!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                _graph.ResetTransform();
                _graph.ScaleTransform(_xScale * 1.0f, -_yScale * 1.0f); //flipped;
                _graph.TranslateTransform(0, -_bitmapHeight / _yScale);
                _graph.TranslateTransform(-_minX + _offsets / _xScale, -_minY + _offsets / _yScale);

                for (int i = 0; i < _drawable.Count; i++)
                    _channels[_drawable[i]].draw(ref _graph, _legendPens[i % _colorsCount], _bitmapWidth);

            }

        }
        /// <summary>
        /// Display Axes
        /// </summary>
        void drawAxes()
        {

            if (_recalculateNeeded)
            {
                for (int i = 0; i < _channels.Count; i++)
                    _channels[i].recalculateStatistics(limit);
                _recalculateNeeded = false;
            }
            if (_scaleNeeded)
            {
                scale();
                _scaleNeeded = false;
            }
            if (_xScale == 0 || _yScale == 0) return;
            lock (_accessGraphicsLock)
            {
                _graph.ResetTransform();
                _graph.ScaleTransform(1.0f, -1.0f); //flipped;
                _graph.TranslateTransform(0, -_bitmapHeight);
                _graph.TranslateTransform(-_minX * _xScale + _offsets, -_minY * _yScale + _offsets);

                _graph.DrawLine(new Pen(Color.Black, 3), -1, _minY * _yScale - _offsets, -1, _maxY * _yScale + _offsets);
                _graph.DrawLine(new Pen(Color.Black, 3), _minX * _xScale - _offsets, -1, _maxX * _xScale + _offsets, -1);
            }


        }
        /// <summary>
        /// Display Legend
        /// </summary>
        void drawLegend()
        {

            if (_recalculateNeeded)
            {
                for (int i = 0; i < _channels.Count; i++)
                    _channels[i].recalculateStatistics(limit);
                _recalculateNeeded = false;
            }
            if (_scaleNeeded)
            {
                scale();
                _scaleNeeded = false;
            }
            if (_xScale == 0 || _yScale == 0) return;
            lock (_accessGraphicsLock)
            {

                _graph.ResetTransform();
                float X = 1, Y = 1;
                foreach (var i in _drawable)
                {
                    SizeF stringSize = _graph.MeasureString(_channels[i].Name, new Font("Arial", 12));
                    if (stringSize.Width > X)
                        X = stringSize.Width;
                    if (stringSize.Height > Y)
                        Y = stringSize.Height;
                }
                _graph.FillRectangle(new SolidBrush(BackColorLegend), 20, 20, X, Y * _drawable.Count);
                for (int i = 0; i < _drawable.Count; i++)
                    _graph.DrawString(_channels[_drawable[i]].Name, new Font("Arial", 12), _legendBrushes[i % _colorsCount], 20, 20 + 20 * i);

            }
        }
        /// <summary>
        /// Display Grid
        /// </summary>
        void drawGrid()
        {

            if (_recalculateNeeded)
            {
                for (int i = 0; i < _channels.Count; i++)
                    _channels[i].recalculateStatistics(limit);
                _recalculateNeeded = false;
            }
            if (_scaleNeeded)
            {
                scale();
                _scaleNeeded = false;
            }
            if (_xScale == 0 || _yScale == 0) return;

            lock (_accessGraphicsLock)
            {

                _graph.ResetTransform();
                _graph.ScaleTransform(1.0f, -1.0f); //flipped;
                _graph.TranslateTransform(0, -_bitmapHeight);
                _graph.TranslateTransform(-_minX * _xScale + _offsets, -_minY * _yScale + _offsets);


                float xStep = (_maxX - _minX) / 10.0f;

                if (xStep * _xScale < MinGridStep)
                    xStep = MinGridStep / _xScale;


                double powNum = Math.Floor(Math.Log10(xStep));
                double dist5 = Math.Abs(Math.Pow(10, powNum) * 5 - xStep);
                double dist2 = Math.Abs(Math.Pow(10, powNum) * 2 - xStep);
                double dist1 = Math.Abs(Math.Pow(10, powNum) * 1 - xStep);
                double[] distances = new double[] { dist1, dist2, dist5 };
                double[] steps = new double[] { Math.Pow(10, powNum) * 1, Math.Pow(10, powNum) * 2, Math.Pow(10, powNum) * 5 };
                int minIndex = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (distances[minIndex] > distances[i])
                        minIndex = i;
                }
                xStep = (float)steps[minIndex] * _xScale;


                for (float i = 0; i > _minX * _xScale; i -= xStep)
                    _graph.DrawLine(Pens.Gray, i, _minY * _yScale - _offsets, i, _maxY * _yScale + _offsets);
                for (float i = 0; i < _maxX * _xScale; i += xStep)
                    _graph.DrawLine(Pens.Gray, i, _minY * _yScale - _offsets, i, _maxY * _yScale + _offsets);

                float yStep = (_maxY - _minY) / 10.0f;


                if (yStep * _yScale < MinGridStep)
                    yStep = MinGridStep / _yScale;

                powNum = Math.Floor(Math.Log10(yStep));
                dist5 = Math.Abs(Math.Pow(10, powNum) * 5 - yStep);
                dist2 = Math.Abs(Math.Pow(10, powNum) * 2 - yStep);
                dist1 = Math.Abs(Math.Pow(10, powNum) * 1 - yStep);
                distances = new double[] { dist1, dist2, dist5 };
                steps = new double[] { Math.Pow(10, powNum) * 1, Math.Pow(10, powNum) * 2, Math.Pow(10, powNum) * 5 };
                minIndex = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (distances[minIndex] > distances[i])
                        minIndex = i;
                }
                yStep = (float)steps[minIndex] * _yScale;




                for (float i = 0; i > _minY * _yScale; i -= yStep)
                    _graph.DrawLine(Pens.Gray, _minX * _xScale - _offsets, i, _maxX * _xScale + _offsets, i);
                for (float i = 0; i < _maxY * _yScale; i += yStep)
                    _graph.DrawLine(Pens.Gray, _minX * _xScale - _offsets, i, _maxX * _xScale + _offsets, i);

                _graph.ResetTransform();
                _graph.TranslateTransform(0, +_bitmapHeight);
                _graph.TranslateTransform(-_minX * _xScale + _offsets, +_minY * _yScale - _offsets);


                //display origin
                _graph.DrawString("0", new Font("Arial", FontSize), Brushes.Gray, 0, 0);
                //
           
                if (xStep / _xScale < 0.0001)
                {
                    for (float i = -xStep; i > _minX * _xScale; i -= xStep)
                        _graph.DrawString(((Math.Round(i / xStep) * xStep) / _xScale).ToString("0.0E00"), new Font("Arial", FontSize), Brushes.Gray, i, 0);
                    for (float i = +xStep; i < _maxX * _xScale; i += xStep)
                        _graph.DrawString(((Math.Round(i / xStep) * xStep) / _xScale).ToString("0.0E00"), new Font("Arial", FontSize), Brushes.Gray, i, 0);

                }
                else if (xStep / _xScale > 10000)
                {
                    for (float i = -xStep; i > _minX * _xScale; i -= xStep)
                        _graph.DrawString(Math.Round((Math.Round(i / xStep) * xStep) / _xScale, 3).ToString("0+E00"), new Font("Arial", FontSize), Brushes.Gray, i, 0);
                    for (float i = +xStep; i < _maxX * _xScale; i += xStep)
                        _graph.DrawString(Math.Round((Math.Round(i / xStep) * xStep) / _xScale, 3).ToString("0+E00"), new Font("Arial", FontSize), Brushes.Gray, i, 0);

                }
                else
                {
                    for (float i = -xStep; i > _minX * _xScale; i -= xStep)
                        _graph.DrawString(Math.Round((Math.Round(i / xStep) * xStep) / _xScale, 3).ToString(), new Font("Arial", FontSize), Brushes.Gray, i, 0);
                    for (float i = +xStep; i < _maxX * _xScale; i += xStep)
                        _graph.DrawString(Math.Round((Math.Round(i / xStep) * xStep) / _xScale, 3).ToString(), new Font("Arial", FontSize), Brushes.Gray, i, 0);

                }

                if (yStep / _yScale < 0.0001)
                {
                    for (float i = -yStep; i > _minY * _yScale; i -= yStep)
                        _graph.DrawString(((Math.Round(i / yStep) * yStep) / _yScale).ToString("0.0E00"), new Font("Arial", FontSize), Brushes.Gray, 0, -i);
                    for (float i = +yStep; i < _maxY * _yScale; i += yStep)
                        _graph.DrawString(((Math.Round(i / yStep) * yStep) / _yScale).ToString("0.0E00"), new Font("Arial", FontSize), Brushes.Gray, 0, -i);
                }
                else if (yStep / _yScale > 10000)
                {
                    for (float i = -yStep; i > _minY * _yScale; i -= yStep)
                        _graph.DrawString(Math.Round((Math.Round(i / yStep) * yStep) / _yScale, 3).ToString("0+E00"), new Font("Arial", FontSize), Brushes.Gray, 0, -i);
                    for (float i = +yStep; i < _maxY * _yScale; i += yStep)
                        _graph.DrawString(Math.Round((Math.Round(i / yStep) * yStep) / _yScale, 3).ToString("0+E00"), new Font("Arial", FontSize), Brushes.Gray, 0, -i);

                }
                else
                {
                    for (float i = -yStep; i > _minY * _yScale; i -= yStep)
                        _graph.DrawString(Math.Round((Math.Round(i / yStep) * yStep) / _yScale, 3).ToString(), new Font("Arial", FontSize), Brushes.Gray, 0, -i);
                    for (float i = +yStep; i < _maxY * _yScale; i += yStep)
                        _graph.DrawString(Math.Round((Math.Round(i / yStep) * yStep) / _yScale, 3).ToString(), new Font("Arial", FontSize), Brushes.Gray, 0, -i);

                }



            }
        }
        /// <summary>
        /// Refresh control
        /// </summary>
        void Refresher()
        {
            if (_xScale == 0 || _yScale == 0) return;

            lock (_accessGraphicsLock)
            {
                _clone = (Bitmap)_bitmap.Clone();
            }

            pictureBox1.Invoke((MethodInvoker)delegate ()
            {

                pictureBox1.Image = _bitmap;
                statusLabel.Text = "";
                pictureBox1.Refresh();
            });
        }
        private void Graph_Resize(object sender, EventArgs e)
        {
            draw(true);
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //  fps counter

            //    //fpss.Add((1000 / _fpsCounter.ElapsedMilliseconds));
            //    //if (fpss.Count > 3)
            //    //    fpss.RemoveAt(0);
            //    //fpsLimit++;
            //    //if (fpsLimit > 10)
            //    //{
            //    //    fps.Text = Math.Round(fpss.Average()).ToString();
            //    //    fpsLimit = 0;
            //    //}


            //_fpsCounter.Restart();


            if (_mousePosition.X == e.Location.X && _mousePosition.Y == e.Location.Y) return;
            if (DrawThread != null && DrawThread.IsAlive) return;
            lock (_accessGraphicsLock)
            {
                if (_clone == null) _clone = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                _bitmap = (Bitmap)_clone.Clone();
                _graph = Graphics.FromImage(_bitmap);

                _graph.ResetTransform();


                float X = _minX + _mousePosition.X / _xScale - _offsets / _xScale,
                    Y = _minY + (_bitmapHeight - _mousePosition.Y) / _yScale - _offsets / _yScale;
                _graph.DrawString($"{X}, {Y}", new Font("Arial", 12), Brushes.Black, _mousePosition.X, _mousePosition.Y-20);
                _graph.DrawLine(Pens.Violet, _mousePosition.X, 0, _mousePosition.X, _bitmapHeight);
                _graph.DrawLine(Pens.Violet, 0, _mousePosition.Y, _bitmapWidth, _mousePosition.Y);
                pictureBox1.Image = _bitmap;
                pictureBox1.Refresh();
                _mousePosition = e.Location;

            }


       




        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            createDelegate();
            draw(true);
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            createDelegate();
            draw(true);
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            createDelegate();
            draw(true);
        }
        /// <summary>
        /// Sets delegate according to checkboxes
        /// </summary>
        void createDelegate()
        {
            _draw -= drawLegend; _draw -= drawAxes; _draw -= drawGrid; _draw -= drawValues;
            if (checkBox3.Checked)
                _draw += drawLegend;
        
            _draw += drawValues;
            if (checkBox1.Checked)
                _draw += drawGrid;
            if (checkBox2.Checked)
                _draw += drawAxes;

        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Hide();
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            lock (_accessGraphicsLock)
            {
                if (_clone == null) _clone = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                _bitmap = (Bitmap)_clone.Clone();
                _graph = Graphics.FromImage(_bitmap);


                pictureBox1.Image = _bitmap;
                pictureBox1.Refresh();

            }
            Cursor.Show();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            MinGridStep = trackBar1.Value;
            gridStepValue.Text = MinGridStep.ToString();
            draw(true);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            _offsets = trackBar2.Value;
            borderWidth.Text = _offsets.ToString();
            draw(true);
        }


        /// <summary>
        /// clear everything
        /// </summary>
        public void clear()
        {
            _channels.Clear();
            _graph.Clear(Color.White);
            _drawable.Clear();
            pictureBox1.Image = _bitmap;
            Refresh();
        }
        
    }

}
