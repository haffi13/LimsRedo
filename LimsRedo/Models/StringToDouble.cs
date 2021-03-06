﻿namespace LimsRedo
{
    class StringToDouble
    {
        public static bool CanConvertToDouble(string str)
        {
            if (str.Contains(","))
            {
                str = str.Replace(',', '.');
            }
            return double.TryParse(str, out double dbl);
        }
        public static double ConvertToDouble(string str)
        {
            if (str.Contains(","))
            {
                str = str.Replace(',', '.');
            }
            return double.Parse(str);
        }
    }
}
