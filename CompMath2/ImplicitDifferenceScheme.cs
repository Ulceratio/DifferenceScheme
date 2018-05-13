using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompMath2
{
    class ImplicitDifferenceScheme
    {
        #region Structs
        public struct Interval
        {
            public double From { get; set; }
            public double To { get; set; }
        }
        #endregion

        #region Fields
        public Func<double, double> f { get; set; }

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
        #endregion

        #region Constructors
        public ImplicitDifferenceScheme(Func<double, double> f, double h, double a, double alfa0, double alfa1, double beta0, double beta1, double gamma0, double gamma1, Interval x, Interval t,double tau = 0)
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
            if(tau == 0)
            {
                Tau = Math.Pow(h, 2.0) / 2.0;
            }
            else
            {
                Tau = tau;
            }
            N = (int)(Math.Abs(t.To - t.From) / Tau);
            M = (int)(Math.Abs(x.To - x.From) / h);
            FillXarr();
            FillTarr();
            FillU();
        }

        public ImplicitDifferenceScheme() { }
        #endregion

        #region MainFucntions
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
            double[] rArr = new double[(int)M + 1];
            double[] sArr = new double[(int)M + 1];
            double[] aArr = new double[(int)M + 1];
            double[] bArr = new double[(int)M + 1];
            double[] cArr = new double[(int)M + 1];
            double[] dArr = new double[(int)M + 1];
            double[,] fArr = new double[(int)N + 1, (int)M + 1];
            Parallel.For(0, (int)M + 1, (i) => uArr[0, i] = 0);
            Parallel.For(0, (int)N + 1, (n) =>
            {
                Parallel.For(0, (int)M + 1, (m) =>
                {
                    fArr[n, m] = f(xArr[m]);
                });
            });
            for (int n = 1; n < (int)N + 1; n++)
            {

                aArr[0] = 0.0;
                bArr[0] = 1.0;    
                cArr[0] = 0.0;    
                dArr[0] = 0.0;

                for (int m = 1; m < M; m++)
                {
                    aArr[m] = (a * a * Tau) / Math.Pow(h, 2.000);
                    bArr[m] = -(1.0 + (2.0 * Tau * a * a) / Math.Pow(h, 2.0));
                    cArr[m] = (a * a * Tau) / Math.Pow(h, 2.000);
                    dArr[m] = -Tau * fArr[n, m] - uArr[n - 1, m];
                }

                aArr[(int)M] = -(2.0 * Tau * a * a) / Math.Pow(h, 2.000);
                bArr[(int)M] = 1.0 + (2.0 * Tau * a * a) / Math.Pow(h, 2.000); 
                cArr[(int)M] = 0.0;
                dArr[(int)M] = Tau * fArr[n, (int)M] + uArr[n - 1, (int)M] + 2.0 * Tau * tArr[n] / h;

                rArr[0] = -cArr[0] / bArr[0];
                sArr[0] = dArr[0] / bArr[0];

                for (int i = 0; i < M; i++)
                {
                    rArr[i + 1] = -cArr[i] / (aArr[i] * rArr[i] + bArr[i]);
                    sArr[i + 1] = (dArr[i] - aArr[i] * sArr[i]) / (aArr[i] * rArr[i] + bArr[i]);
                }

                uArr[n, (int)M] = (dArr[(int)M] - aArr[(int)M] * sArr[(int)M]) / (bArr[(int)M] + aArr[(int)M] * rArr[(int)M]);

                for (int m = (int)M; m > 0; m--)
                {
                    uArr[n, m - 1] = rArr[m] * uArr[n, m] + sArr[m];
                }
            }
        }
        #endregion

        #region ExtraFunctions
        public object Clone()
        {
            return new ImplicitDifferenceScheme()
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
                h = h,
                Tau = Tau
            };
        }


        private int getIndexOfElement(double[] Arr, double item)
        {
            int result = -1;
            Parallel.For(0, Arr.Length, (i, state) =>
            {
                if (Math.Abs(Arr[i] - item) < 0.0101)
                {
                    result = i;
                    state.Break();
                }
            });
            return result;
        }

        public double PlotFunction(double t, double x)
        {
            int i = getIndexOfElement(tArr, t);
            int j = getIndexOfElement(xArr, x);
            return uArr[i, j];
        }

        #endregion
    }
}
