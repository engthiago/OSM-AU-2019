using System;

namespace Osm.Revit.Utils
{
    class RealToFractionHelper
    {

        public Fraction RealToFraction(double value, double accuracy)
        {
            if (accuracy <= 0.0 || accuracy >= 1.0)
            {
                throw new ArgumentOutOfRangeException("accuracy", "Must be > 0 and < 1.");
            }

            int sign = Math.Sign(value);

            if (sign == -1)
            {
                value = Math.Abs(value);
            }

            // Accuracy is the maximum relative error; convert to absolute maxError
            double maxError = sign == 0 ? accuracy : value * accuracy;

            int n = (int)Math.Floor(value);
            value -= n;

            if (value < maxError)
            {
                return new Fraction(sign * n, 1);
            }

            if (1 - maxError < value)
            {
                return new Fraction(sign * (n + 1), 1);
            }

            // The lower fraction is 0/1
            int lower_n = 0;
            int lower_d = 1;

            // The upper fraction is 1/1
            int upper_n = 1;
            int upper_d = 1;

            while (true)
            {
                // The middle fraction is (lower_n + upper_n) / (lower_d + upper_d)
                int middle_n = lower_n + upper_n;
                int middle_d = lower_d + upper_d;

                if (middle_d * (value + maxError) < middle_n)
                {
                    // real + error < middle : middle is our new upper
                    upper_n = middle_n;
                    upper_d = middle_d;
                }
                else if (middle_n < (value - maxError) * middle_d)
                {
                    // middle < real - error : middle is our new lower
                    lower_n = middle_n;
                    lower_d = middle_d;
                }
                else
                {
                    // Middle is our best fraction
                    return new Fraction((n * middle_d + middle_n) * sign, middle_d);
                }
            }
        }

        public string ToNearestSixteenth(double value)
        {

            var inches = Math.IEEERemainder(value, 12);
            var remainder = inches / 0.0625;
            remainder = Math.Round(remainder);
            var fraction = "";

            switch (remainder)
            {
                case 1:
                    fraction = "1/16";
                    break;
                case 2:
                    fraction = "1/8";
                    break;
                case 3:
                    fraction = "3/16";
                    break;
                case 4:
                    fraction = "1/4";
                    break;
                case 5:
                    fraction = "5/16";
                    break;
                case 6:
                    fraction = "3/8";
                    break;
                case 7:
                    fraction = "7/16";
                    break;
                case 8:
                    fraction = "1/2";
                    break;
                case 9:
                    fraction = "9/16";
                    break;
                case 10:
                    fraction = "5/8";
                    break;
                case 11:
                    fraction = "11/16";
                    break;
                case 12:
                    fraction = "3/4";
                    break;
                case 13:
                    fraction = "13/16";
                    break;
                case 14:
                    fraction = "7/8";
                    break;
                case 15:
                    fraction = "15/16";
                    break;
            }
            return " " + fraction + @"''";
        }

        public struct Fraction
        {
            public Fraction(int n, int d)
            {
                N = n;
                D = d;
            }

            public int N { get; private set; }
            public int D { get; private set; }
        }
    }
}
