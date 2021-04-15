using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace csv_viewer
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Background open operation
        /// </summary>
        Thread _openBackground = new Thread(new ThreadStart(()=> { }));
        /// <summary>
        /// Background generate operations
        /// </summary>
        List<Thread> _generateBackground = new List<Thread>();
        /// <summary>
        /// current decimal delimeter mode
        /// </summary>
        enum decimalCeparatorMode {sign, auto}
        /// <summary>
        /// current field delimeter mode
        /// </summary>
        enum fieldCeparatorMode { sign, auto }
        //default values
        decimalCeparatorMode _DecimalSeparatorMode = decimalCeparatorMode.auto;
        fieldCeparatorMode _FieldCeparatorMode = fieldCeparatorMode.auto;
        string _fieldSeparator = "\t";
        string _decimalSeparator = ".";
        string _filename;
        public Form1()
        {
            InitializeComponent();
            graph2.BackColorLegend = Color.FromArgb(125, Color.Yellow);
        }
        /// <summary>
        /// open file button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

            graph2.clear();
            statusStrip1.Items[0].Text = "Opening CSV file";
            OpenFileDialog file = new OpenFileDialog();
           file.Filter = "CSV files (*.csv, *.txt) | *.csv; *.txt";
            if (file.ShowDialog() == DialogResult.OK)
            {
                _filename = file.FileName;
                statusStrip1.Items[0].Text = $"Opening CSV file: '{file.FileName}'";
                _openBackground = new Thread(new ThreadStart(() => OpenFileBackground(file.FileName)));
                _openBackground.Start();
                button2.Enabled = true;
                Text = $"Csv Viewer - {file.FileName}";
                button1.Enabled = false;
            }
        }
        /// <summary>
        /// background 'open file' thread (callbacks inside)
        /// </summary>
        /// <param name="filename"></param>
        void OpenFileBackground(string filename)
        {
            ProgressWindow progressWindow = new ProgressWindow("In progress", $"Opening{filename}", _openBackground, statusStrip1);
            Task r = Task.Run(delegate () { Application.Run(progressWindow); });
            double fileSize = new FileInfo(filename).Length;
            double current = 0;
            double previousPercentForRefreshGraph = 0;
            double prevPercentForCallback = 0 ;
            double percent = 0;
            if (_FieldCeparatorMode == fieldCeparatorMode.auto)
            {
                //predicting field separator
                using (StreamReader reader = new StreamReader(filename))
                {
                    string line = reader.ReadLine();
                    if (line.Contains("\t"))
                        _fieldSeparator = "\t";
                    else if (line.Contains(";"))
                        _fieldSeparator = ";";
                    else
                        _fieldSeparator = ",";
                }
            }
            if (_DecimalSeparatorMode == decimalCeparatorMode.auto)
            {
                //predicting decimal separator
                using (StreamReader reader = new StreamReader(filename))
                {
                    if(_fieldSeparator == ",")
                        _decimalSeparator = ".";
                    else
                    {
                        //first line
                        reader.ReadLine();

                        string line = reader.ReadLine();
                        while (!line.Contains(".") && !line.Contains(","))
                            line = reader.ReadLine();
                        if (line.Contains("."))
                            _decimalSeparator = ".";
                        else if(line.Contains(","))
                            _decimalSeparator = ",";
                    }

                   
                }
            }

            using (StreamReader reader = new StreamReader(filename))
            {
                //Get Names
                string line = reader.ReadLine();
                string[] row = line.Split(new string[] { _fieldSeparator }, StringSplitOptions.RemoveEmptyEntries);
                float[] floatRow;
                current = line.Length;
                int channelCount = row.Length;
                floatRow = new float[channelCount];
                listBox1.Invoke((MethodInvoker)delegate ()
                {
                    listBox1.SelectedIndexChanged -= listBox1_SelectedIndexChanged;
                    listBox1.Items.Clear();

                    for (int i = 0; i < row.Length; i++)
                    {
                        graph2.addChannel(row[i]);
                        listBox1.Items.Add(row[i]);
                        if (i < 5)
                        {       
                            listBox1.SelectedIndices.Add(i);
                        }
                    }
                    listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
                });
                //get values async
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator != _decimalSeparator)
                        line = line.Replace(_decimalSeparator, Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
                    row = line.Split(new string[] { _fieldSeparator }, StringSplitOptions.None);
                    if (row.Length == channelCount)
                    {
                        for (int i = 0; i < channelCount; i++)
                        {
                            if (!float.TryParse(row[i], out floatRow[i]))
                                floatRow[i] = float.NaN;
                        }
                        if (!float.IsNaN(floatRow[0]))
                        {
                            for (int i = 0; i < channelCount; i++)
                            {
                                graph2.insertInto(i, new PointF(floatRow[0], floatRow[i]));
                            }
                        }

                    }
                    current += line.Length;
                    percent = (int)(current / fileSize * 100);
                    if (percent != prevPercentForCallback)
                    {
                        prevPercentForCallback = percent;
                        if (progressWindow.IsHandleCreated)
                            progressWindow.BeginInvoke((MethodInvoker)delegate ()
                        {
                            if (progressWindow.IsHandleCreated)
                                progressWindow.UpdateProgressBar(percent);
                        });
                        if (percent - previousPercentForRefreshGraph > 5)
                        {
                            graph2.BeginInvoke((MethodInvoker)delegate ()
                            {
                                try
                                {
                                    if (graph2.IsHandleCreated)
                                        graph2.draw(false);
                                }
                                catch (Exception ex) { }
                            });
                            statisticBox.BeginInvoke((MethodInvoker)delegate ()
                            {
                                statisticBox.Text = "";
                                List<string> stat = graph2.GetStatistic();
                                stat.Insert(0, $"File: {filename}");
                                statisticBox.Lines = stat.ToArray();

                            });
                            previousPercentForRefreshGraph = percent;
                        }

                    }

                }
            }
            if (graph2.IsHandleCreated)
                graph2.BeginInvoke((MethodInvoker)delegate ()
            {
                try
                {
                    if (graph2.IsHandleCreated)
                        graph2.draw(true);
                }
                catch (Exception ex) { }
            });
            statisticBox.BeginInvoke((MethodInvoker)delegate ()
            {
                statisticBox.Text = "";
                List<string> stat = graph2.GetStatistic();
                stat.Insert(0, $"File: {filename}");
                statisticBox.Lines = stat.ToArray();
            });
            if (progressWindow.IsHandleCreated)
                progressWindow.BeginInvoke((MethodInvoker)delegate () {
                if (progressWindow.IsHandleCreated)
                    progressWindow.UpdateProgressBar(100);
            });

        }
        /// <summary>
        /// selected channels indexes changed, calls update Drawable, rescale and draw
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            graph2.ClearDrawable();
            foreach (var i in listBox1.SelectedIndices)
            {
                graph2.AddDrawable(Convert.ToInt32(i));
            }
            try
            {
                graph2.draw(true);
                statisticBox.Text = "";
                List<string> stat = graph2.GetStatistic();
                stat.Insert(0, $"File: {_filename}");
                statisticBox.Lines = stat.ToArray();
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// close current channels button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (graph2.DrawThread.IsAlive)
                graph2.DrawThread.Abort();
            graph2.statusLabel.Text = "";
            graph2.clear();
            Text = "CSV Viewer";
            statusStrip1.Items[0].Text = "Completed successfully";
            statisticBox.Text = "";
            listBox1.Items.Clear();
            button2.Enabled = false;
            button1.Enabled = true;
        }
        /// <summary>
        /// generate file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < _generateBackground.Count; i++)
            {
                if (_generateBackground[i] == null || !_generateBackground[i].IsAlive)
                {
                    _generateBackground.RemoveAt(i);
                    i--;
                }
          
            }
            SaveFileDialog file = new SaveFileDialog();
            file.Filter = "CSV files (*.csv, *.txt) | *.csv; *.txt";
            if (file.ShowDialog() == DialogResult.OK)
            {
                if (checkBox1.Checked)
                    _generateBackground.Add(new Thread(new ThreadStart(() => GenerateFileBackground(file.FileName, Convert.ToInt32(channelCount.Text), Convert.ToInt32(rowCount.Text), 10))));
                else
                    _generateBackground.Add(new Thread(new ThreadStart(() => GenerateFileBackground(file.FileName, Convert.ToInt32(channelCount.Text), Convert.ToInt32(rowCount.Text)))));
                _generateBackground[_generateBackground.Count-1].Start();
                statusStrip1.Items[0].Text = $"Generating CSV file '{file.FileName}'";
            }
        }

        /// <summary>
        /// background 'generate file' thread (callbacks inside)
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="channelCount"></param>
        /// <param name="rowCount"></param>
        /// <param name="NaNs"></param>
        void GenerateFileBackground(string filename, int channelCount, int rowCount, int NaNs = -1)
        {
            //file generator
            ProgressWindow progressWindow = new ProgressWindow("In Process", $"Generating CSV File '{filename}'", Thread.CurrentThread, statusStrip1);


            Task r = Task.Run(delegate () { Application.Run(progressWindow); });
            
            List<Generator> generators = new List<Generator>();
            Random random = new Random();
            List<List<double>> values = new List<List<double>>();
            
            for (int i = 0; i < channelCount; i++)
            {
                int classType = random.Next(0, 3);
                switch (classType)
                {
                    case 0:
                        generators.Add(new Sin(random.Next(1,100)/(10*1.0f)));
                        break;
                    case 1:
                        generators.Add(new Cos(random.Next(1, 100) / (10 * 1.0f)));
                        break;
                    case 2:
                        generators.Add(new SinCos(random.Next(1, 100) / (10 * 1.0f)));
                        break;
                }
            }
            float step = 0.01F;
            List<string> line = new List<string>();
            for (int i = 0; i < channelCount+1; i++)
                line.Add(i.ToString());
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = _decimalSeparator;
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine(String.Join(_fieldSeparator, line));
                for (float i = 0, j=0;  j<rowCount; i += step, j++)
                {
                    line = new List<string>();
                    line.Add(i.ToString());
                    foreach (var generator in generators)
                    {
                        if(random.Next(0,100)<NaNs)
                            line.Add("err");
                        else
                        line.Add( generator.getValue(i).ToString(nfi));
                    }
                    writer.WriteLine(String.Join(_fieldSeparator, line));
                    if (progressWindow.IsHandleCreated)
                        progressWindow.BeginInvoke((MethodInvoker)delegate ()
                    {
                        if (progressWindow.IsHandleCreated)
                            progressWindow.UpdateProgressBar(Math.Round(j / (rowCount * 1.0f)*100, 3));
                    });
                    
                }
            }
            if (progressWindow.IsHandleCreated)
                progressWindow.BeginInvoke((MethodInvoker)delegate ()
            {
                if (progressWindow.IsHandleCreated)
                    progressWindow.UpdateProgressBar(100);
            });
        }
        
        private void sepTab_CheckedChanged(object sender, EventArgs e)
        {
            if (sepTab.Checked)
            {
                _fieldSeparator = "\t";
                _FieldCeparatorMode = fieldCeparatorMode.sign;
            }
        }

        private void sepSemi_CheckedChanged(object sender, EventArgs e)
        {
            if (sepSemi.Checked)
            {
                _fieldSeparator = ";";
                _FieldCeparatorMode = fieldCeparatorMode.sign;
            }
        }

        private void sepComma_CheckedChanged(object sender, EventArgs e)
        {
            if (sepComma.Checked)
            {
                _fieldSeparator = ",";
                _FieldCeparatorMode = fieldCeparatorMode.sign;
                if (deciComma.Checked)
                    deciAuto.Checked = true;
            }
        }

        private void sepAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (sepAuto.Checked)
            {
                _FieldCeparatorMode = fieldCeparatorMode.auto;
                _fieldSeparator = "\t";
            }
        }

        private void deciDot_CheckedChanged(object sender, EventArgs e)
        {
            if (deciDot.Checked)
            {
                _decimalSeparator = ".";
                _DecimalSeparatorMode = decimalCeparatorMode.sign;
            }
        }

        private void deciComma_CheckedChanged(object sender, EventArgs e)
        {
            if (deciComma.Checked)
            {
                _decimalSeparator = ",";
                _DecimalSeparatorMode = decimalCeparatorMode.sign;
                if (sepComma.Checked)
                    sepAuto.Checked = true;
            }
        }

        private void deciAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (deciAuto.Checked)
            {
                _DecimalSeparatorMode = decimalCeparatorMode.auto;
                _decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            }
        }
        /// <summary>
        /// validating generation parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void channelCount_KeyUp(object sender, KeyEventArgs e)
        {
            int val = 0;
            if(int.TryParse(channelCount.Text, out val))
            {
                if (val < 1)
                {
                    errorProvider1.SetError(channelCount, "number must be positive");
                    button3.Enabled = false;
                }

                else if(val > 100000)
                {
                    errorProvider1.SetError(channelCount, "number is too big");
                    button3.Enabled = false;
                }

                else
                {
                    button3.Enabled = true;
                    errorProvider1.SetError(channelCount, null);
                }
            }
            else
            {
                errorProvider1.SetError(channelCount, "invalid number");
                button3.Enabled = false;
            }


            val = 0;
            if (int.TryParse(rowCount.Text, out val))
            {
                if (val < 1)
                {
                    errorProvider1.SetError(rowCount, "number must be positive");
                    button3.Enabled = false;
                }

                else if (val > 100000)
                {
                    errorProvider1.SetError(rowCount, "number is too big");
                    button3.Enabled = false;
                }

                else
                {
                    errorProvider1.SetError(rowCount, null);
                    button3.Enabled = true;
                }
            }
            else
            {
                errorProvider1.SetError(rowCount, "invalid number");
                button3.Enabled = false;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //statisticBox.Text = "";
            //statisticBox.Lines = graph2.GetStatistic().ToArray();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            graph2.DrawThread.Abort();
            for (int i = 0; i < _generateBackground.Count; i++)
               if(_generateBackground[i]!=null && _generateBackground[i].IsAlive)
                    _generateBackground[i].Abort();
            if (_openBackground != null && _openBackground.IsAlive)
                _openBackground.Abort();
        }
    }
}
