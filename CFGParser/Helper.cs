using System;
using System.Text.RegularExpressions;

namespace CFGParser
{
    internal static class Helper
    {
        public static string AddQuote(this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            return '"' + str + '"';
        }
    }
}
