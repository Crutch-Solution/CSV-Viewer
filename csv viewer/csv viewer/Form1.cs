using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        Thread _background = null;
        enum decimalCeparatorMode {sign, auto}
        enum fieldCeparatorMode { sign, auto }
        decimalCeparatorMode _DecimalSeparatorMode = decimalCeparatorMode.auto;
        fieldCeparatorMode _FieldCeparatorMode = fieldCeparatorMode.auto;
        string _fieldSeparator = "\t";
        string _decimalSeparator = ".";
        public Form1()
        {
            InitializeComponent();
            graph2.BackColorLegend = Color.FromArgb(125, Color.Yellow);
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            graph2.clear();
            statusStrip1.Items[0].Text = "Opening CSV file";
            OpenFileDialog file = new OpenFileDialog();
            if (file.ShowDialog() == DialogResult.OK)
            {
                statusStrip1.Items[0].Text = $"Opening CSV file: '{file.FileName}'";
                _background = new Thread(new ThreadStart(() => OpenFileBackground(file.FileName)));
                _background.Start();
                //new Task(() => OpenFileBackground(file.FileName)).Start();
                //_background();
            }
        }
        void OpenFileBackground(string filename)
        {
            ProgressWindow progressWindow = new ProgressWindow("In progress", $"Opening{filename}", _background);
            Task r = Task.Run(delegate () { Application.Run(progressWindow); });
            //new Thread(new ThreadStart((MethodInvoker)delegate () { Application.Run(progressWindow); })).Start();
            //progressWindow.Show();
           // r.Wait();
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
                    if(Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator!= _decimalSeparator)
                        line= line.Replace(_decimalSeparator, Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
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
                    percent = Math.Round(current / fileSize * 100, 1);
                    if (percent != prevPercentForCallback)
                    {

                        progressWindow.BeginInvoke((MethodInvoker)delegate ()
                        {
                            if (progressWindow.IsHandleCreated)
                                progressWindow.UpdateProgressBar(percent);
                        });
                        if (percent - previousPercentForRefreshGraph > 5)
                        {
                            graph2.BeginInvoke((MethodInvoker)delegate ()
                            {
                                if (graph2.IsHandleCreated)
                                    graph2.draw();
                            });
                            previousPercentForRefreshGraph = percent;
                        }
                        prevPercentForCallback = percent;
                    }

                }
            }
            progressWindow.BeginInvoke((MethodInvoker)delegate () {
                if (progressWindow.IsHandleCreated)
                    progressWindow.UpdateProgressBar(100);
            });
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            graph2.Drawable.Clear();
            foreach (var i in listBox1.SelectedIndices)
            {
                graph2.Drawable.Add(Convert.ToInt32(i));
            }
            graph2.scale();
            graph2.draw();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            graph2.clear();
            Text = "CSV Viewer";
            statusStrip1.Items[0].Text = "Completed successfully";
            listBox1.Items.Clear();
            button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
        Delegate generator;

        void FileGenerator(string separatorField, List<List<string>> values, string filename)
        {
            //ProgressWindow pg = new ProgressWindow();
            /*pg.Show();
            if (File.Exists(filename))
                File.Delete(filename);
            File.Create(filename).Close();
            using (StreamWriter writer = new StreamWriter(filename))
            {
                string line="";
                for (int i = 0; i < values[0].Count;i++)
                {
                    line = "";
                    for (int j = 0; j < values.Count-1;j++)
                        line += values[j][i] + "\t";
                    line += values[values.Count - 1][i];
                }
                writer.WriteLine(line);
            }
         //       System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
         //   nfi.NumberDecimalSeparator = separatorDecimal;
            double hui = 99.99;
            string a = hui.ToString(nfi);*/
        }

        private void sepTab_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void sepSemi_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void sepComma_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void sepAuto_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void fieldDot_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void fieldComma_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void fieldAuto_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
