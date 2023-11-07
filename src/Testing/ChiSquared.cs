using System;
using System.Linq;

namespace DG.Cryptography.Testing
{
    /// <summary>
    /// Provides a way to calculate the chi-squared value of a contingency table.
    /// </summary>
    public class ChiSquared
    {
        private const double _bigX = 20;

        private readonly int _width;
        private readonly int _height;
        private readonly double[,] _grid;
        private readonly int _degreesOfFreedom;

        private double _total;
        private double[] _rowTotals;
        private double[] _colTotals;

        private Lazy<double> _x2;

        /// <summary>
        /// The degrees of freedom for this calculation
        /// </summary>
        public int DegreesOfFreedom => _degreesOfFreedom;

        /// <summary>
        /// The result of this chi-squared calculation.
        /// </summary>
        public double Result => _x2.Value;

        private ChiSquared(int width, int height)
        {
            _width = width;
            _height = height;
            _grid = new double[_width, _height];
            _degreesOfFreedom = CalculateDegreesOfFreedom(_width, _height);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ChiSquared"/> with the given array of frequencies.
        /// </summary>
        /// <param name="frequencies"></param>
        public ChiSquared(double[] frequencies) : this(frequencies.Length, 1)
        {
            for (long i = 0; i < frequencies.LongLength; i++)
            {
                _grid[i, 0] = frequencies[i];
            }
            CalculateTotals();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ChiSquared"/> with the given contingency table.
        /// </summary>
        /// <param name="grid"></param>
        public ChiSquared(double[,] grid) : this(grid.GetLength(0), grid.GetLength(1))
        {
            Array.Copy(grid, _grid, grid.LongLength);
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            _total = _grid.Cast<double>().Sum();
            _colTotals = Enumerable.Range(0, _width).Select(x => Enumerable.Range(0, _height).Sum(y => _grid[x, y])).ToArray();
            _rowTotals = Enumerable.Range(0, _height).Select(y => Enumerable.Range(0, _width).Sum(x => _grid[x, y])).ToArray();

            _x2 = new Lazy<double>(() => CalculateChiSquared());
        }

        private double CalculateChiSquared()
        {
            double x2 = 0;
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var observed = _grid[x, y];
                    var expected = CalculateExpectedValue(x, y);
                    var eo2e = CalculateEo2e(observed, expected);
                    x2 += eo2e;
                }
            }
            return x2;
        }

        private double CalculateExpectedValue(int x, int y)
        {
            if (_width == 1)
            {
                return _colTotals[0] / _height;
            }
            if (_height == 1)
            {
                return _rowTotals[0] / _width;
            }
            return _colTotals[x] * _rowTotals[y] / _total;
        }

        private static double CalculateEo2e(double observed, double expected)
        {
            return Math.Pow(expected - observed, 2) / expected;
        }

        /// <summary>
        /// Calculates the degree of freedom for a grid of the specified size.
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static int CalculateDegreesOfFreedom(int columns, int rows)
        {
            if (columns == 1)
            {
                return rows - 1;
            }
            if (rows == 1)
            {
                return columns - 1;
            }
            return (columns - 1) * (rows - 1);
        }

        /// <summary>
        /// Calculates the probability that this result is due to chance.
        /// </summary>
        /// <returns></returns>
        public double CalculatePValue()
        {
            return CalculatePValue(_x2.Value, _degreesOfFreedom);
        }

        /// <summary>
        /// Calculates the probability, based on the given chi-squared value and degrees of freedom, that this result is due to chance.
        /// </summary>
        /// <param name="chiSquaredValue"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns></returns>
        public static double CalculatePValue(double chiSquaredValue, int degreesOfFreedom)
        {
            double LOG_SQRT_PI = 0.5723649429247000870717135; // log(sqrt(pi))
            double I_SQRT_PI = 0.5641895835477562869480795;   // 1 / sqrt(pi)

            if (chiSquaredValue <= 0.0 || degreesOfFreedom < 1)
            {
                return 1.0;
            }

            double a = 0.5 * chiSquaredValue;
            bool even = degreesOfFreedom % 2 == 0;

            if (degreesOfFreedom <= 1)
            {
                return even ? 1.0 - a / Math.Sqrt(2.0) : StandardNormalCDF(-Math.Sqrt(chiSquaredValue)) * 2.0;
            }

            double y = ExponentialTerm(-a);
            double s = even ? y : 2.0 * StandardNormalCDF(-Math.Sqrt(chiSquaredValue));

            if (degreesOfFreedom <= 2)
            {
                return s;
            }

            double xHalf = 0.5 * (degreesOfFreedom - 1.0);
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
                double e = even ? 1.0 : I_SQRT_PI / Math.Sqrt(a);
                double c = 0.0;
                while (z <= xHalf)
                {
                    e *= a / z;
                    c += e;
                    z += 1.0;
                }
                return c * y + s;
            }
        }

        private static double ExponentialTerm(double x)
        {
            return x < -_bigX ? 0.0 : Math.Exp(x);
        }

        private static double StandardNormalCDF(double z)
        {
            double zMax = 6.0; // Maximum meaningful z value

            if (z == 0.0)
            {
                return 0.0;
            }

            double y = 0.5 * Math.Abs(z);

            if (y >= zMax * 0.5)
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
