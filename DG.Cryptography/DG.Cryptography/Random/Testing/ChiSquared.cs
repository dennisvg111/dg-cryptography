using DG.Common.Exceptions;
using System;
using System.Linq;

namespace DG.Cryptography.Random.Testing
{
    public class ChiSquared
    {
        private readonly double[,] _grid;
        private Lazy<ChiSquaredCalculation> _calcuation;

        public ChiSquared(int width, int height)
        {
            _grid = new double[width, height];
            ResetCalculation();
        }

        public ChiSquared(double[,] grid) : this(grid.GetLength(0), grid.GetLength(1))
        {
            Array.Copy(grid, _grid, grid.Length);
        }

        public ChiSquared(params double[][] grid) : this(grid.Length, grid[0].Length)
        {
            var combined = CombineArrays(grid);
            Array.Copy(combined, _grid, combined.Length);
        }

        private static double[,] CombineArrays(double[][] arrays)
        {
            ThrowIf.Parameter.IsNull(arrays, nameof(arrays));
            ThrowIf.Collection(arrays, nameof(arrays)).Any(a => a == null, "Cannot contain null arrays.");
            ThrowIf.Collection(arrays, nameof(arrays)).Any(a => a.Length != arrays[0].Length, "Input arrays should all have the same length.");

            var width = arrays.Length;
            var height = arrays[0].Length;
            var result = new double[width, height];

            for (int arrayIndex = 0; arrayIndex < width; arrayIndex++)
            {
                for (int i = 0; i < height; i++)
                {
                    result[arrayIndex, i] = arrays[arrayIndex][i];
                }
            }

            return result;
        }

        private void ResetCalculation()
        {
            _calcuation = new Lazy<ChiSquaredCalculation>(() => new ChiSquaredCalculation(_grid));
        }

        public void Set(int x, int y, double value)
        {
            _grid[x, y] = value;
            ResetCalculation();
        }

        public double CalculateX2()
        {
            return _calcuation.Value.GetX2();
        }

        public double CalculateExpectedValue(int x, int y)
        {
            return _calcuation.Value.GetExpectedValue(x, y);
        }

        public int CalculateDegreesOfFreedom()
        {
            return _calcuation.Value.GetDegreesOfFreedom();
        }

        /// <summary>
        /// This represents the probability that this result is due to chance.
        /// </summary>
        /// <returns></returns>
        public double CalculateAlpha()
        {
            return _calcuation.Value.CalculateAlpha();
        }

        private class ChiSquaredCalculation
        {
            private const double _bigX = 20;

            private readonly double[,] _grid;

            private readonly int _width;
            private readonly int _height;

            private Lazy<double> _total;
            private Lazy<double>[] _rowTotals;
            private Lazy<double>[] _colTotals;


            public ChiSquaredCalculation(double[,] grid)
            {
                _width = grid.GetLength(0);
                _height = grid.GetLength(1);

                _grid = new double[_width, _height];
                Array.Copy(grid, _grid, grid.Length);

                _total = new Lazy<double>(() => CalculateTotal());
                _colTotals = Enumerable.Range(0, _width).Select(x => new Lazy<double>(() => CalculateColumnTotal(x))).ToArray();
                _rowTotals = Enumerable.Range(0, _height).Select(y => new Lazy<double>(() => CalculateRowTotal(y))).ToArray();
            }

            public double GetX2()
            {
                double x2 = 0;
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        var observed = _grid[x, y];
                        var expected = GetExpectedValue(x, y);
                        var eo2e = CalculateEo2e(observed, expected);
                        x2 += eo2e;
                    }
                }
                return x2;
            }

            public double GetExpectedValue(int x, int y)
            {
                return (_colTotals[x].Value * _rowTotals[y].Value) / _total.Value;
            }

            public int GetDegreesOfFreedom()
            {
                return (_width - 1) * (_height - 1);
            }

            private static double CalculateEo2e(double observed, double expected)
            {
                return Math.Pow(expected - observed, 2) / expected;
            }

            private double CalculateRowTotal(int y)
            {
                return Enumerable.Range(0, _width).Sum(x => _grid[x, y]);
            }

            private double CalculateColumnTotal(int x)
            {
                return Enumerable.Range(0, _height).Sum(y => _grid[x, y]);
            }

            private double CalculateTotal()
            {
                return _grid.Cast<double>().Sum();
            }

            public double CalculateAlpha()
            {
                return Pochisq(GetX2(), GetDegreesOfFreedom());
            }

            public static double Pochisq(double x, int df)
            {
                double LOG_SQRT_PI = 0.5723649429247000870717135; // log(sqrt(pi))
                double I_SQRT_PI = 0.5641895835477562869480795;   // 1 / sqrt(pi)

                if (x <= 0.0 || df < 1)
                {
                    return 1.0;
                }

                double a = 0.5 * x;
                bool even = (df % 2 == 0);

                if (df <= 1)
                {
                    return even ? (1.0 - a / Math.Sqrt(2.0)) : (StandardNormalCDF(-Math.Sqrt(x)) * 2.0);
                }

                double y = ExponentialTerm(-a);
                double s = even ? y : (2.0 * StandardNormalCDF(-Math.Sqrt(x)));

                if (df <= 2)
                {
                    return s;
                }

                double xHalf = 0.5 * (df - 1.0);
                double z = even ? 1.0 : 0.5;

                if (a > _bigX)
                {
                    double e = even ? 0.0 : LOG_SQRT_PI;
                    double c = Math.Log(a);
                    while (z <= xHalf)
                    {
                        e += Math.Log(z);
                        s += ExponentialTerm(c * z - a - e);
                        z += 1.0;
                    }
                    return s;
                }
                else
                {
                    double e = even ? 1.0 : (I_SQRT_PI / Math.Sqrt(a));
                    double c = 0.0;
                    while (z <= xHalf)
                    {
                        e *= (a / z);
                        c += e;
                        z += 1.0;
                    }
                    return c * y + s;
                }
            }

            private static double ExponentialTerm(double x)
            {
                return (x < -_bigX) ? 0.0 : Math.Exp(x);
            }

            private static double StandardNormalCDF(double z)
            {
                double zMax = 6.0; // Maximum meaningful z value

                if (z == 0.0)
                {
                    return 0.0;
                }

                double y = 0.5 * Math.Abs(z);

                if (y >= (zMax * 0.5))
                {
                    return z > 0.0 ? 1.0 : 0.0;
                }
                if (y < 1.0)
                {
                    double w = y * y;
                    double x = ((((((((0.000124818987 * w
                             - 0.001075204047) * w + 0.005198775019) * w
                             - 0.019198292004) * w + 0.059054035642) * w
                             - 0.151968751364) * w + 0.319152932694) * w
                             - 0.531923007300) * w + 0.797884560593) * y * 2.0;
                    return z > 0.0 ? (x + 1.0) * 0.5 : (1.0 - x) * 0.5;
                }
                y -= 2.0;
                double x2 = (((((((((((((-0.000045255659 * y
                               + 0.000152529290) * y - 0.000019538132) * y
                               - 0.000676904986) * y + 0.001390604284) * y
                               - 0.000794620820) * y - 0.002034254874) * y
                               + 0.006549791214) * y - 0.010557625006) * y
                               + 0.011630447319) * y - 0.009279453341) * y
                               + 0.005353579108) * y - 0.002141268741) * y
                               + 0.000535310849) * y + 0.999936657524;
                return z > 0.0 ? (x2 + 1.0) * 0.5 : (1.0 - x2) * 0.5;
            }
        }
    }
}
