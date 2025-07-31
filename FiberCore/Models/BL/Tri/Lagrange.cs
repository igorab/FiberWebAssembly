using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lagrange
{
    public class Lagrange
    {
        private double Lgr(int index, double[] X, double x)
        {
            double l = 1;
            for (int idx = 0; idx < X.Length; idx++)
            {
                if (idx != index)
                {
                    l *= (x - X[idx]) / (X[index] - X[idx]);
                }
            }
            return l;
        }

        public double GetValue(double[] X, double[] Y, double x)
        {
            double y = 0;

            for (int idx = 0; idx < X.Length; idx++)
            {
                y += Y[idx] * Lgr(idx, X, x);
            }

            return y;
        }

        private double Lgr(int index, double[] X, double h, double x)
        {
            double lgr = 1;

            for (int i = 0; i < X.Length; i++)
            {
                if (i != index)
                {
                    lgr *= (x - X[0] - i * h) / (index - i);
                }
            }

            return lgr / Math.Pow(h, X.Length - 1);
        }

        public double GetValue(double[] X, double h, double[] Y, double x)
        {
            double y = 0;

            for (int idx = 0; idx < X.Length; idx++)
            {
                y += Y[idx] * Lgr(idx, X, h, x);
            }

            return y;
        }
    }
}
