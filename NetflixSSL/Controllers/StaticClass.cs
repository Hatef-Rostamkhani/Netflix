using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetflixSSL.Controllers
{
    public static class StaticClass
    {
        public static string Normal(this string str)
        {
            str = str.Replace("\"", "\"\"");
            if (str.Contains(","))
            {
                return $"\"{str}\"";
            }
            return str;
        }
    }
}
