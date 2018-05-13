using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompMath2
{
    class ExplicitDifferenceScheme:ICloneable
    {
        #region Structs
        public struct Interval
        {
            public double From { get; set; }
            public double To { get; set; }
        }
        #endregion

        #region Fields
        public Func<double,double> f { get; set; }

        public double[,] uArr;

        public double[] xArr;

        public double[] tArr;

        public double Tau { get; set; }

        public double h { get; set; }

        public Interval x { get; set; }

        public Interval t { get; set; }

        public double N { get; set; }

        public double M { get; set; }

        public double a { get; set; }

        public double alfa0 { get; set; }

        public double alfa1 { get; set; }

        public double beta0 { get; set; }

        public double beta1 { get; set; }

        public double gamma0 { get; set; }

        public double gamma1 { get; set; }

        double[,] fArr;
        #endregion

        #region Constructors
        public ExplicitDifferenceScheme(Func<double, double> f,double h,double a, double alfa0, double alfa1, double beta0, double beta1, double gamma0, double gamma1, Interval x,Interval t)
        {
            this.alfa0 = alfa0;
            this.alfa1 = alfa1;
            this.beta0 = beta0;
            this.beta1 = beta1;
            this.gamma0 = gamma0;
            //this.gamma1 = gamma1;
            this.a = a;
            this.f = f;
            this.x = x;
            this.t = t;
            this.h = h;
            Tau = Math.Pow(h, 2.0) / 2.0;
            N = (int)(Math.Abs(t.To - t.From) / Tau);
            M = (int)(Math.Abs(x.To - x.From) / h);
            FillXarr();
            FillTarr();
            FillU();
        }

        public ExplicitDifferenceScheme() { }
        #endregion

        #region Fucntions
        private void FillXarr()
        {
            xArr = new double[(int)M + 1];
            for (int i = 0; i < M + 1; i++)
            {
                xArr[i] = x.From + i * h;
            }
        }

        private void FillTarr()
        {
            tArr = new double[(int)N + 1];
            for (int i = 0; i < N + 1; i++)
            {
                tArr[i] = t.From + i * Tau;
            }
        }

        private void FillU()
        {
            uArr = new double[((int)N + 1), ((int)M + 1)];
            fArr = new double[(int)N + 1, (int)M + 1];

            Parallel.For(0, (int)M + 1, (i) => uArr[0, i] = 0);
            Parallel.For(0, (int)N + 1, (n) =>
            {
                Parallel.For(0, (int)M + 1, (m) =>
                {
                    fArr[n, m] = f(xArr[m]);
                });
            });

            for (int n = 0; n < N; n++)
            {
                for (int m = 1; m < M; m++)
                {
                    uArr[n + 1, m] = CalculateNewUi(n, m);
                }
            }

            for (int n = 0; n < N; n++)
            {
                uArr[n + 1, 0] = CalculateNewU0(uArr[n + 1, 1], uArr[n + 1, 2]);
            }

            for (int n = 0; n < N; n++)
            {
                gamma1 = tArr[n + 1];
                uArr[n + 1, (int)M] = CalculateNewUm(uArr[n + 1, (int)M - 1], uArr[n + 1, (int)M - 2]);
            }

            //for (int n = 0; n < N + 1; n++)
            //{
            //    Parallel.For(1, (int)M, (m) =>
            //    {
            //        uArr[n + 1, m] = CalculateNewUi(n, m);
            //        if (m == 2)
            //        {
            //            uArr[n + 1, 0] = CalculateNewU0(uArr[n + 1, 1], uArr[n + 1, 2]);
            //        }
            //        if (m == N - 1)
            //        {
            //            gamma1 = tArr[n + 1];
            //            uArr[n + 1, m + 1] = CalculateNewUm(uArr[n + 1, m - 1], uArr[n + 1, m]);
            //        }
            //    });
            //}
        }

        private double CalculateNewUi(int n,int m)
        {
            double numerator = uArr[n, m + 1] - 2 * uArr[n, m] + uArr[n, m - 1];
            double denumerator = h * h;
            double divide = numerator / denumerator;
            return Tau * ((a * a) * (numerator / denumerator) + fArr[n, m]) + uArr[n, m];
        }

        private double CalculateNewU0(double U1, double U2)
        {
            double numerator = gamma0 - ((2.0 * beta0) / h) * U1 + ((beta0) / (2.0 * h)) * U2;
            double denumerator = alfa0 - (3.0 / 2.0) * (beta0 / h);
            return numerator / denumerator;
        }

        private double CalculateNewUm(double U1, double U2)
        {
            return (gamma1 - (beta1 / (2 * h)) * U2 + (2 * beta1 / h) * U1) / (alfa1 + 3 * beta1 / (2 * h));
        }

        public object Clone()
        {
            return new ExplicitDifferenceScheme()
            {
                a = a,
                alfa0 = alfa0,
                alfa1 = alfa1,
                beta0 = beta0,
                beta1 = beta1,
                gamma0 = gamma0,
                gamma1 = gamma1,
                xArr = (double[])xArr.Clone(),
                tArr = (double[])tArr.Clone(),
                uArr = (double[,])uArr.Clone(),
                t = t,
                x = x,
                M = M,
                N = N,
                f = f,
                h=h,
                Tau = Tau               
            };
        }


        private int getIndexOfElement(double[] Arr,double item)
        {
            int result = -1;
            Parallel.For(0, Arr.Length, (i,state) =>
            {
                if (Math.Abs(Arr[i] - item) < 0.0101)
                {
                    result = i;
                    state.Break();
                }
            });
            return result;
        }

        public double PlotFunction(double t,double x)
        {
            int i = getIndexOfElement(tArr, t);
            int j = getIndexOfElement(xArr, x);
            return uArr[Math.Abs(i), Math.Abs(j)];
        }

        #endregion
    }
}
