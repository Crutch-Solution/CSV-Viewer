using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
namespace csv_viewer
{
    public partial class ProgressWindows : Form
    {
       
        string Path;
        Form1 father;
        double maximum;
        double current;
        Thread main;
        public ProgressWindows(string path, Form1 father)
        {
            InitializeComponent();
            Path = path;
            this.father = father;
            maximum = new FileInfo(Path).Length;
            progressBar1.Value = 0;
            this.Text = "In progress...";
            label1.Text = $"Opening CSV file: '{path}'";
            label2.Text = "0%";

        }
        public void openfile()
        {
            List<List<PointF>> values = new List<List<PointF>>();
            using (StreamReader reader = new StreamReader(Path))
            {

                string line = reader.ReadLine();
                string[] row;
                current = line.Length;
                List<string> names = new List<string>();
                for (int i = 0; i < line.Split('\t').Length - 1; i++)
                {
                    names.Add(line.Split('\t')[i + 1]);
                    values.Add(new List<PointF>());
                }

                callBackNames(names);
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    try
                    {
                        row = line.Split('\t');
                        for (int i = 0; i < row.Length - 1; i++)
                        {
                            try
                            {
                                Convert.ToSingle(row[i + 1]);
                            }
                            catch
                                (Exception ex)
                            {
                                continue;
                            }
                            values[i].Add(new PointF(Convert.ToSingle(row[0]), Convert.ToSingle(row[i + 1])));
                            // label1.Text = i.ToString();
                        }
                    }catch(Exception ex)
                    {

                    }
                    current += line.Length;

                    if (watch.ElapsedMilliseconds > 3000)
                    {
                        List<List<PointF>> buff = new List<List<PointF>>();
                        foreach(var i in values)
                        {
                            List<PointF> newone = new List<PointF>();
                            foreach (var j in i)
                                newone.Add(new PointF(j.X, j.Y));
                            buff.Add(newone);
                        }
                        callBackValues(buff);
                        foreach (var i in values)
                            i.Clear();
                        watch = System.Diagnostics.Stopwatch.StartNew();
                    }



                }
                if (values.Count != 0)
                {
                    List<List<PointF>> buff = new List<List<PointF>>();
                    foreach (var i in values)
                    {
                        List<PointF> newone = new List<PointF>();
                        foreach (var j in i)
                            newone.Add(new PointF(j.X, j.Y));
                        buff.Add(newone);
                    }
                    callBackValues(buff);
                    foreach (var i in values)
                        i.Clear();
                }

            }
            callBackCancel();
        }
        public void callBackNames(List<string> names)
        {
            father.callbackNames(names);
        }
        public void callBackValues(List<List<PointF>> values)
        {
            if (progressBar1.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate() { callBackValues(values); });
            }
            else
            {
                progressBar1.Value = (int)(current/maximum*100);
                label2.Text = $"{progressBar1.Value}%";
                father.callbackValues(values);
            }
        }
        public void callBackCancel()
        {
            if (progressBar1.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate() { callBackCancel(); });
            }
            else
            {
                Close();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ProgressWindows_FormClosing(object sender, FormClosingEventArgs e)
        {
            father.callbackCancel(Path);
            main.Abort();
        }

        private void ProgressWindows_Shown(object sender, EventArgs e)
        {
            main = new Thread(new ThreadStart(openfile));
            main.Start();
        }
    }
}
