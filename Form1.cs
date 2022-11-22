using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using Brushes = System.Windows.Media.Brushes;
using LiveCharts.Defaults;
using LiveCharts.Wpf.Charts.Base;


namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        double f1, f2, f3;
        double f_d;
        int N;
        int N1, N2;
        int N_1, N_2;
        double E; //порог
        int T; //кол-во отсчетов окна


        public double[,] signal;
        public double[,] prog;
        public double[,] pred;
        public double[,] doorstep; //порог
        public double[,] convolution; //свертка

        public Form1()
        {
            InitializeComponent();
        }

        //private void CartesianChart1OnDataClick(object sender, ChartPoint chartPoint)
        //{
        //    MessageBox.Show("(" + chartPoint.X + ";" + chartPoint.Y + ")");
        //}


        public List<double[,]> Signal()
        {

            if (for_f1.Text != "" || for_f2.Text != "" || for_f3.Text != "")
            {
                f1 = Convert.ToDouble(for_f1.Text);
                f2 = Convert.ToDouble(for_f2.Text);
                f3 = Convert.ToDouble(for_f3.Text);
            }
            else { MessageBox.Show("Вы не ввели параметр f", "Внимание!"); }
            if (for_f_d.Text != "" || for_N.Text != "" || for_N1.Text != "" || for_N2.Text != "")
            {
                f_d = Convert.ToDouble(for_f_d.Text);
                N = Convert.ToInt32(for_N.Text);
                N1 = Convert.ToInt32(for_N1.Text);
                N2 = Convert.ToInt32(for_N2.Text);
                T = Convert.ToInt32(for_T.Text);
                E = Convert.ToDouble(for_E.Text);
            }
            else { MessageBox.Show("Вы не ввели параметр T, E, N, N1, N2 или f_d", "Внимание!"); }

            signal = new double[2, N];//Массив значений 
            doorstep = new double[2, N];
            float dt = (float)(1 / f_d);

            for (int i = 0; i < N; i++)
            {
                signal[1, i] = (float)i * dt;
            }

            double curPhase = 0;
            for (int j=0; j<N; j++)
            {
                signal[0, j] = Math.Sin(curPhase);
                if (j < N1) curPhase += 2 * Math.PI * f1 * dt;
                else if (j < N2) curPhase += 2 * Math.PI * f2 * dt;
                else curPhase += 2 * Math.PI * f3 * dt;
                doorstep[1,j]= (float)j* dt;
                doorstep[0, j] = (float)E;
            }
            List<double[,]> sv = new List<double[,]>();
            sv.Add(signal);
            sv.Add(doorstep);

            return sv;
        }
        public double[,] Prognoz()
        {
            double w = 2 * Math.PI * f2;
            double a1 = (-2 * Math.Cos(w / f_d));
            prog = new double[2, N];
            pred = new double[2, N];
            pred[0, 0] = signal[0, 0];
            prog[1, 0] = signal[1, 0];
            //tek[0, 0]=(float)Math.Abs(signal[0, 0] - pred[0, 0]);
            pred[0,1] = (-signal[0, 1] * a1);
            prog[1,1] = signal[1, 1];
            //tek[0, 1]=(float)Math.Abs(signal[0, 1] - pred[0, 1]);
            for (int i = 2; i < N; i++)
            {
                double temp = -a1 * signal[0,i - 1] - signal[0, i - 2];
                pred[0,i] = temp;
                prog[1, i] = signal[1, i];
                prog[0, i]=(float)Math.Abs(pred[0, i] - signal[0, i]);
            }
            return prog;
        }

        public double [,] LPFilter()
        {
            //double [] arr = new double [2*N];
            //double[] it = new double[2*N];
            List<double> arr = new List<double>();
            List<double> it = new List<double>();
            double temp = 0;
            for (int i = (int)(T / 2); i < (N - T / 2); i++)
            {
                for (int j = (int)(i - T / 2); j < (i + T / 2); j++)
                {
                    temp += 1 * prog[0,j];
                }
                //arr[i]=(temp / T);
                //it[i]=(signal[1,i]);
                arr.Add(temp / T);
                it.Add(signal[1, i]);
                temp = 0;
            }
            convolution = new double [2, arr.Count];
            for (int i = 0; i < arr.Count; i++)
            {
                convolution[0,i] =  (float)arr[i];
                convolution[1, i] = (float)it[i];
            }
            return convolution;
        }
        public void SearchBorder()
        {
            bool first = false;
            for (int i = 0; i < N - T; i++)
            {
                if (convolution[0,i] <= E && first == false)//если меньше порога 
                {
                    first = true;
                    N_1 = i + T / 2;
                }
                if (convolution[0,i] <= E && first == true)
                {
                    N_2 = i + T / 2;
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            build_Graph(Signal()[0]);
            build_Graph1(Prognoz(), Signal()[1], LPFilter());
            //Prognoz();
            //LPFilter();
            SearchBorder();
            for_N_1.Text = N_1.ToString();
            for_N_2.Text = N_2.ToString();   
        }

        private void build_Graph(double[,] signal)
        {
            //Очистка предыдущих коллекций
            cartesianChart1.Series.Clear();
            cartesianChart1.AxisX.Clear();
            cartesianChart1.AxisY.Clear();

            SeriesCollection series = new SeriesCollection();//Коллекция линий
            LineSeries ln = new LineSeries();//Линия

            ChartValues<ObservablePoint> Values = new ChartValues<ObservablePoint>();//Коллекция значений по Oy
            for (int j = 0; j < signal.GetLength(1); j++)
            {
                Values.Add(new ObservablePoint(signal[1, j], signal[0, j]));
            }
            ln.Values = Values;//Добавление значений на линию
            ln.PointGeometrySize = 1;
            series.Add(ln);//Добавление линии в коллекцию линий
            cartesianChart1.Series = series;//Добавление коллекции на график


            //Определение максимума и минимума по Ox
            double min = signal[1, 0];
            double max = signal[1, 0];
            for (int j = 0; j < signal.GetLength(1); j++)
            {
                if (min > signal[1, j])
                {
                    min = signal[1, j];
                }
                if (max < signal[1, j])
                {
                    max = signal[1, j];
                }
            }
            //Ось Ox
            cartesianChart1.AxisX.Add(new Axis
            {
                Title = "t, с",//подпись
                LabelFormatter = value => value.ToString(""),
                MinValue = min,
                MaxValue = max,
                Separator = new Separator
                {
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 79, 86))
                }
            });
            min = signal[0, 0];
            max = signal[0, 0];
            for (int j = 0; j < signal.GetLength(1); j++)
            {
                if (min > signal[0, j])
                {
                    min = signal[0, j];
                }
                if (max < signal[0, j])
                {
                    max = signal[0, j];
                }
            }
            //Ось Oy
            cartesianChart1.AxisY.Add(new Axis
            {
                Title = "F(t)",
                MinValue = min,
                MaxValue = max,
                Separator = new Separator
                {
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 79, 86))
                }
            });
        }

        private void build_Graph1(double[,] prog, double[,] doorstep, double[,] convolution)
        {
            //Очистка предыдущих коллекций
            cartesianChart2.Series.Clear();
            cartesianChart2.AxisX.Clear();
            cartesianChart2.AxisY.Clear();

            SeriesCollection series = new SeriesCollection();//Коллекция линий
            LineSeries ln = new LineSeries();//Линия

            ChartValues<ObservablePoint> Values = new ChartValues<ObservablePoint>();//Коллекция значений по Oy
            for (int j = 0; j < prog.GetLength(1); j++)
            {
                Values.Add(new ObservablePoint(prog[1, j], prog[0, j]));
            }
            ln.Values = Values;//Добавление значений на линию
            ln.PointGeometrySize = 1;
            series.Add(ln);//Добавление линии в коллекцию линий

            LineSeries ln1 = new LineSeries();//Линия

            ChartValues<ObservablePoint> Values1 = new ChartValues<ObservablePoint>();//Коллекция значений по Oy
            for (int j = 0; j < convolution.GetLength(1); j++)
            {
                Values1.Add(new ObservablePoint(convolution[1,j],convolution[0,j]));
            }
            ln1.Values = Values1;//Добавление значений на линию
            ln1.PointGeometrySize = 1;
            series.Add(ln1);//Добавление линии в коллекцию линий

            LineSeries ln2 = new LineSeries();//Линия

            ChartValues<ObservablePoint> Values2 = new ChartValues<ObservablePoint>();//Коллекция значений по Oy
            for (int j = 0; j < doorstep.GetLength(1); j++)
            {
                Values2.Add(new ObservablePoint(doorstep[1, j], doorstep[0, j]));
            }
            ln2.Values = Values2;//Добавление значений на линию
            ln2.PointGeometrySize = 1;
            series.Add(ln2);//Добавление линии в коллекцию линий

            cartesianChart2.Series = series;//Добавление коллекции на график


            //Определение максимума и минимума по Ox
            double min = prog[1, 0];
            double max = prog[1, 0];
            for (int j = 0; j < prog.GetLength(1); j++)
            {
                if (min > prog[1, j])
                {
                    min = prog[1, j];
                }
                if (max < prog[1, j])
                {
                    max = prog[1, j];
                }
            }
            //Ось Ox
            cartesianChart2.AxisX.Add(new Axis
            {
                Title = "t, с",//подпись
                LabelFormatter = value => value.ToString(""),
                MinValue = min,
                MaxValue = max,
                Separator = new Separator
                {
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 79, 86))
                }
            });
            min = prog[0, 0];
            max = prog[0, 0];
            for (int j = 0; j < prog.GetLength(1); j++)
            {
                if (min > prog[0, j])
                {
                    min = prog[0, j];
                }
                if (max < prog[0, j])
                {
                    max = prog[0, j];
                }
            }
            //Ось Oy
            cartesianChart2.AxisY.Add(new Axis
            {
                Title = "F(t)",
                MinValue = min,
                MaxValue = max,
                Separator = new Separator
                {
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 79, 86))
                }
            });
        }
    }

}
