using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.IO;

namespace CompMath2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SurfacePlotModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new SurfacePlotModel();

            MainPlot.DataContext = viewModel;

            //MainPlot.Title = "Явная разностная схема";

            MainPlot.XAxisLabel = "Ось Х";

            MainPlot.YAxisLabel = "Ось Y";

            MainPlot.ZAxisLabel = "Ось Z";
        }

        Func<double, double> f = new Func<double, double>((x) => 
        {
            return Math.Pow(Math.E, Math.Pow(x, 3)) - 1.0;
        });

        private const double eps = 0.00005;
        private double h;

        private bool CheckForExplicit(double[,] U1,double[,] U2)
        {
            bool result = true;
            List<double> diffs = new List<double>();
            double mainEps = 3.0 * eps;
            double maxDif = 0;
            Parallel.For(0, (U2.GetLength(0) / 4), (i) =>
            {
                Parallel.For(0, U2.GetLength(1) / 2, (j) =>
                {
                    double diff = Math.Abs(U2[4 * i, 2 * j] - U1[i, j]);
                    if (diff > maxDif)
                    {
                        maxDif = diff;
                    }
                });
            });
            if(maxDif>mainEps)
            {
                result = false;
            }
            txt.Text = "Максимальная разница - " + (maxDif).ToString() + $" Рамер {U1.GetLength(0)} x {U1.GetLength(1)}";
            return result;
        }

        private bool CheckForImplicit(double[,] U1, double[,] U2)
        {
            bool result = true;
            List<double> diffs = new List<double>();
            double mainEps = 3.0 * eps;
            double maxDif = 0;
            Parallel.For(0, (U2.GetLength(0) / 2), (i) =>
            {
                Parallel.For(0, U2.GetLength(1) / 2, (j) =>
                {
                    double diff = Math.Abs(U2[2 * i, 2 * j] - U1[i, j]);
                    if (diff > maxDif)
                    {
                        maxDif = diff;
                    }
                });
            });
            if (maxDif > mainEps)
            {
                result = false;
            }
            txt.Text = "Максимальная разница - " + (maxDif).ToString()+$" Рамер {U1.GetLength(0)} x {U1.GetLength(1)}"; 
            return result;
        }

        private async Task<ExplicitDifferenceScheme> CalculateForExplicit(double hTemp)
        {
            Func<double, ExplicitDifferenceScheme> FT = new Func<double, ExplicitDifferenceScheme>((H) => {
                return new ExplicitDifferenceScheme(f, H, 1, 1, 0, 0, 1, 0, 1, new ExplicitDifferenceScheme.Interval() { From = 0, To = 1 }, new ExplicitDifferenceScheme.Interval() { From = 0, To = 1 });
            });
            return await Task.Run(() => FT(hTemp));           
        }

        private async Task<ImplicitDifferenceScheme> CalculateForImplicit(double hTemp,double tau=0)
        {
            Func<double, ImplicitDifferenceScheme> FT = new Func<double, ImplicitDifferenceScheme>((H) =>
            {
                if(tau == 0)
                {
                    return new ImplicitDifferenceScheme(f, H, 1, 1, 0, 0, 1, 0, 1, new ImplicitDifferenceScheme.Interval() { From = 0, To = 1 }, new ImplicitDifferenceScheme.Interval() { From = 0, To = 1 });
                }
                else
                {
                    return new ImplicitDifferenceScheme(f, H, 1, 1, 0, 0, 1, 0, 1, new ImplicitDifferenceScheme.Interval() { From = 0, To = 1 }, new ImplicitDifferenceScheme.Interval() { From = 0, To = 1 },tau);
                }
            });
            return await Task.Run(() => FT(hTemp));
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

        }


        private async void Choose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (Choose.SelectedIndex)
            {
                case 0:
                    {
                        viewModel.Title = "Явная разностная схема";

                        ExplicitDifferenceScheme scheme1;
                        ExplicitDifferenceScheme scheme2;

                        h = 0.5;

                        scheme1 = null;
                        scheme2 = null;

                        do
                        {
                            if (scheme1 == null)
                            {
                                scheme1 = await CalculateForExplicit(h);
                            }
                            else
                            {
                                scheme1 = (ExplicitDifferenceScheme)scheme2.Clone();
                            }

                            scheme2 = await CalculateForExplicit(h / 2);

                            h /= 2;
                        } while (!CheckForExplicit(scheme1.uArr, scheme2.uArr));

                        scheme2 = null;
                        GC.Collect();

                        double maxX = scheme1.xArr.Max();
                        double maxT = scheme1.tArr.Max();
                        double minX = scheme1.xArr.Min();
                        double minT = scheme1.tArr.Min(); ;


                        double maxXY = maxX > maxT ? maxX : maxT;
                        double minXY = minX > minT ? minX : minT;

                        viewModel.PlotFunction(scheme1.PlotFunction, minXY, maxXY);
                        break;
                    }
                case 1:
                    {
                        viewModel.Title = "Неявная разностная схема";

                        ImplicitDifferenceScheme scheme1;
                        ImplicitDifferenceScheme scheme2;

                        h = 0.015625;

                        scheme1 = null;
                        scheme2 = null;

                        do
                        {
                            if (scheme1 == null)
                            {
                                scheme1 = await CalculateForImplicit(h);
                            }
                            else
                            {
                                scheme1 = (ImplicitDifferenceScheme)scheme2.Clone();
                            }

                            scheme2 = await CalculateForImplicit(h / 2 , scheme1.Tau/2);

                            h /= 2;
                        } while (!CheckForImplicit(scheme1.uArr, scheme2.uArr));

                        scheme2 = null;
                        GC.Collect();

                        double maxX = scheme1.xArr.Max();
                        double maxT = scheme1.tArr.Max();
                        double minX = scheme1.xArr.Min();
                        double minT = scheme1.tArr.Min(); ;


                        double maxXY = maxX > maxT ? maxX : maxT;
                        double minXY = minX > minT ? minX : minT;

                        viewModel.PlotFunction(scheme1.PlotFunction, minXY, maxXY);
                        break;
                    }
                default:
                    break;
            }
        }

        private void WriteDataToExcel(Point3D[,] points)
        {
            Microsoft.Office.Interop.Excel.Application oXL;
            Microsoft.Office.Interop.Excel._Workbook oWB;
            Microsoft.Office.Interop.Excel._Worksheet oSheet;
            Microsoft.Office.Interop.Excel.Range oRng;
            object misvalue = System.Reflection.Missing.Value;
            try
            {
                //Start Excel and get Application object.
                oXL = new Microsoft.Office.Interop.Excel.Application();
                oXL.Visible = true;

                //Get a new workbook.
                oWB = (Microsoft.Office.Interop.Excel._Workbook)(oXL.Workbooks.Add(""));
                oSheet = (Microsoft.Office.Interop.Excel._Worksheet)oWB.ActiveSheet;

                //Add table headers going cell by cell.
                oSheet.Cells[1, 1] = "X";
                oSheet.Cells[1, 2] = "Y";
                oSheet.Cells[1, 3] = "Z";

                int k = 2;

                for (int i = 0; i < points.GetLength(0); i++)
                {
                    for (int j = 0; j < points.GetLength(1); j++)
                    {
                        oSheet.Cells[k, 1] = points[i, j].X;
                        oSheet.Cells[k, 2] = points[i, j].Y;
                        oSheet.Cells[k, 3] = points[i, j].Z;
                        k++;
                    }
                }

                oXL.Visible = true;
                oXL.UserControl = true;
                oWB.SaveAs("test1.xlsx", Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
                    false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                oWB.Close();
            }
            catch { }
        }
    }
}
