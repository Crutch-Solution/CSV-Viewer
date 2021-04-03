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
        string _fieldSeparator = "\t";
        string _decimalSeparator = ".";
        public Form1()
        {
            InitializeComponent();
            graph2.BackColorLegend = Color.FromArgb(125, Color.Yellow);
        }
        /// <summary>
        /// Kill opening or file generation process
        /// </summary>
        internal void KillBackground()
        {
            if(_background!=null && _background.IsAlive)
                _background.Abort();
            _background = null;
        }
        
        public void callbackNames(List<string> names)
        {
            //if (listBox1.InvokeRequired)
            //{
            //    BeginInvoke((MethodInvoker)delegate () { callbackNames(names); });
            //}
            //else
            //{
            //    lock (graph2)
            //    {
            //        int selectedIndex = 0;
            //        graph2.setChannelsNames(names);
            //        graph2.Drawable.Clear();
            //        foreach (var i in names)
            //        {
            //            listBox1.Items.Add(i);
            //            if (selectedIndex < 5)
            //                graph2.Drawable.Add(selectedIndex++);
            //        }
            //    }
            //}
        }
        public void callbackValues(List<List<PointF>> values)
        {
            //if (listBox1.InvokeRequired)
            //{
            //    BeginInvoke((MethodInvoker)delegate () { callbackValues(values); });
            //}
            //else
            //{
            //    lock (graph2)
            //    {
            //        graph2.setChannelsValues(values);
            //    }
            //}
        }
        public void callbackCancel(string filename)
        {
            //if (listBox1.InvokeRequired)
            //{
            //    BeginInvoke((MethodInvoker)delegate () { callbackCancel(filename); });
            //}
            //else
            //{
            //    listBox1.SelectedIndexChanged -= listBox1_SelectedIndexChanged;
            //        statusStrip1.Items[0].Text = "Completed successfully";
            //    this.Text = $"CSV Viewer - {filename}";
            //    button2.Enabled = true;
            //        foreach (var i in graph2.Drawable)
            //            listBox1.SelectedIndices.Add(i);
            //    listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            graph2.clear();
            statusStrip1.Items[0].Text = "Opening CSV file";
            OpenFileDialog file = new OpenFileDialog();
            if (file.ShowDialog() == DialogResult.OK)
            {
                statusStrip1.Items[0].Text = $"Opening CSV file: '{file.FileName}'";
                ProgressWindow PW = new ProgressWindow("In progress", $"Opening{file.FileName}");
                new Thread(new ThreadStart((MethodInvoker)delegate () { Application.Run(PW); })).Start();
                new Task(() => OpenFileBackground(file.FileName)).Start();
                //_background();
            }
        }
        void OpenFileBackground(string filename)
        {
            long maximum = new FileInfo(filename).Length;
            long current = 0;
            using (StreamReader reader = new StreamReader(filename))
            {
                //Get Names
                string line = reader.ReadLine();
                string[] row = line.Split(new string[] { _fieldSeparator }, StringSplitOptions.RemoveEmptyEntries);
                current = line.Length;
                for (int i = 0; i < row.Length - 1; i++)
                    graph2.addChannel(row[i + 1]);
                //get values async
                while (!reader.EndOfStream)
                {
                    row = reader.ReadLine().Split(new string[] { _fieldSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < row.Length; i++)
                    {
                        try
                        {
                            graph2.insertInto(i-1, new PointF(Convert.ToSingle(row[0]), Convert.ToSingle(row[i])));
                        }
                        catch (Exception ex)
                        {
                            graph2.insertInto(i - 1, new PointF(0, 0));
                        }
                    }
                    graph2.draw();
                }
            }
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

        private void AnyFieldSep_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton[] FieldSep = groupBox7.Controls.OfType<RadioButton>().ToArray();
          /*  for(int  i=0;i<FieldSep.Length;i++)
                generator = (MethodInvoker)delegate () { FileGenerator(, ""); };*/
        }
        private void AnyDecSep_CheckedChanged(object sender, EventArgs e)
        {
          /*  var target = groupBox5.Controls.OfType<RadioButton>().Where(x => x.Checked).First();
            generator = (MethodInvoker)delegate () { FileGenerator(, ""); };*/
        }
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
    }
}
