using System;

namespace Osm.Revit
{
    static public class Doubles
    {
        static public bool IsAlmostEqual(this double firstNumber, double secondNumber, double tolerance = 0.0328084)
        {
            if (Math.Abs(firstNumber - secondNumber) <= tolerance) return true;

            return false;
        }
    }
}
