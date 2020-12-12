using System;
using System.Collections.Generic;
using System.Text;

namespace Aetherium.Utils
{
    public class MathHelpers
    {
        public static string FloatToPercentageString(float number, float numberBase = 100)
        {
            return (number * numberBase).ToString("##0") + "%";
        }
    }
}
