using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeTrip.Utils.ConfigFileChanger.Extensions
{
    public static class StringExtensions
    {

        public static string FormatWith(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
    }
}
