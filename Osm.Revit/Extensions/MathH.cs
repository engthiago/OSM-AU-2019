using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit
{
    static public class MathH
    {
        public static double RoundSnap(double number, double snap)
        {
            if (snap > 0)
            {
                return Math.Round(number / snap) * snap;
            }

            return number;
        }
    }
}
