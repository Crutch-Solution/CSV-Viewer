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
        Point _mousePosition;
        Bitmap _clone;
        public Graph()
        {
            InitializeComponent();
            _fpsCounter = new Stopwatch();
            _fpsCounter.Start();

            _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            _graph = Graphics.FromImage(_bitmap);

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

        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public void draw(bool force)
        {
            if (_channels.Count == 0 || _channels[0].values.Count < 2)
                return;



            _draw -= Refresher;
            _draw += Refresher;

            limit = _channels[0].values.Count;
            lock (pictureBox1)
            {
                if (force)
                {
                    if (DrawThread != null)
                    {
                        DrawThread.Abort();
                        DrawThread = null;
                        //  Thread.Sleep(50);
                    }

                    _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    _graph = Graphics.FromImage(_bitmap);
                    _bitmapHeight = _bitmap.Height;
                    _bitmapWidth = _bitmap.Width;


                    _graph.Clear(Color.White);
                    _graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    statusLabel.Text = "Updating, please wait";
                    DrawThread = new Thread(new ThreadStart(()=> { try { _draw(); } catch (Exception ex) { } }));
                    DrawThread.Start();
                }
                else if (!DrawThread.IsAlive)
                {
                    if (DrawThread != null)
                    {
                        DrawThread.Abort();
                        DrawThread = null;
                        //Thread.Sleep(50);
                    }

                    _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    _graph = Graphics.FromImage(_bitmap);
                    _bitmapHeight = _bitmap.Height;
                    _bitmapWidth = _bitmap.Width;


                    _graph.Clear(Color.White);
                    _graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    statusLabel.Text = "Updating, please wait";

                    DrawThread = new Thread(new ThreadStart(() => { try { _draw(); } catch (Exception ex) { } }));
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
            _recalculateNeeded = true;
        }
        int offsets = 5;
        bool _recalculateNeeded;
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



            //foreach (var i in _channels)
            //    i.scale(_xScale, _yScale, limit, pictureBox1.Width);
        }
        List<int> _drawablePrevious = new List<int>();
        int _countPrevious=-1;
        void drawValues()
        {
            if (_recalculateNeeded)
            {
                for (int i = 0; i < _channels.Count; i++)
                    _channels[i].recalculateStatistics(limit);
                _recalculateNeeded = false;
            }
            scale();
            //check for buffer

            //

            //old
            ////_graph.Clear(Color.White);
            ////_graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            ////_graph.ResetTransform();
            ////_graph.ScaleTransform(1.0f, -1.0f); //flipped;
            ////_graph.TranslateTransform(0, -_bitmapHeight);
            ////_graph.TranslateTransform(-_minX * _xScale + offsets, -_minY * _yScale + offsets);
            //

            //new

            _graph.ResetTransform();
            _graph.ScaleTransform(_xScale * 1.0f,-_yScale *1.0f); //flipped;
            _graph.TranslateTransform(0, -_bitmapHeight/ _yScale);
            //_graph.TranslateTransform(-_minX + offsets/ _xScale, -_minY + offsets/ _yScale);
            _graph.TranslateTransform(-_minX, -_minY);
            //
            for (int i = 0; i < Drawable.Count; i++)
                _channels[Drawable[i]].draw(ref _graph, _legendPens[i%_colorsCount], pictureBox1.Width);

            //pictureBox1.Invoke((MethodInvoker)delegate () {
            //   // pictureBox1.Image = _bitmap;
            //    //pictureBox1.Refresh();
            //    statusLabel.Text = "";
            //});
            //// pictureBox1.Refresh();


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
               // pictureBox1.Image = _bitmap;
               // pictureBox1.Refresh();
            });
        }
        void drawLegend()
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
            //  pictureBox1.Image = _bitmap;
            //  pictureBox1.Refresh();

        }
        void drawGrid()
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

            //  pictureBox1.Image = _bitmap;
            //  pictureBox1.Refresh();
        }
        Stopwatch _fpsCounter;
        List<long> fpss = new List<long>();
        int fpsLimit = 0;
        void Refresher() {
            pictureBox1.Invoke((MethodInvoker)delegate ()
            {
                pictureBox1.Image = _bitmap;
                statusLabel.Text = "";
                pictureBox1.Refresh();
                _clone = (Bitmap)_bitmap.Clone();
                //refreshCounter++;
                //prevMsec += _fpsCounter.ElapsedMilliseconds;
                //if (prevMsec > 5000)
                //{

            });
        }
        private void Graph_Resize(object sender, EventArgs e)
        {
            draw(true);
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (DrawThread != null && DrawThread.IsAlive) return;
            if (_mousePosition.X == e.Location.X && _mousePosition.Y == e.Location.Y) return;
            if (_clone == null) return;
            lock (pictureBox1)
            {
                _bitmap = (Bitmap)_clone.Clone();
                _graph = Graphics.FromImage(_bitmap);

                _graph.ResetTransform();


                float X = _minX + _mousePosition.X / _xScale,
                    Y = _minY + (_bitmapHeight - _mousePosition.Y) / _yScale;
                _graph.DrawString($"{X}, {Y}", new Font("Arial", 12), Brushes.Black, _mousePosition);
                pictureBox1.Image = _bitmap;
                pictureBox1.Refresh();
                _mousePosition = e.Location;


                fpss.Add((1000 / _fpsCounter.ElapsedMilliseconds));
                if (fpss.Count > 3)
                    fpss.RemoveAt(0);
                fpsLimit++;
                if (fpsLimit > 10)
                {
                    fps.Text = fpss.Average().ToString();
                    fpsLimit = 0;
                }
                //    prevMsec = 0;
                //    refreshCounter = 0;
                //}

                _fpsCounter.Restart();

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



        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            draw(true);
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
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
