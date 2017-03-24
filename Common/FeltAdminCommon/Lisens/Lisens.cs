using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeltAdminCommon.Lisens
{
    using System.IO;
    using System.Security.Cryptography;
    using System.Windows.Forms;

    using FeltAdmin.Diagnostics;

    public static class LisensChecker
    {

        public static string GenerateLisens(string Skytterlag, DateTime validto)
        {

            string totalString = Skytterlag.ToUpper();
            totalString = washstring(totalString);
            totalString = totalString +";"+ validto.ToString("s");

            var byteArray = Encoding.UTF8.GetBytes(totalString);
            string s = Convert.ToBase64String(byteArray);

            return s;
        }

        private static string washstring(string original)
        {
            if (string.IsNullOrEmpty(original))
            {
                return original;
            }
            var totalString = original.Trim();
            totalString = totalString.Replace(".", "");
            totalString = totalString.Replace("-", "");
            totalString = totalString.Replace("\\", "");
            totalString = totalString.Replace("/", "");
            return totalString;
        }

        public static bool Validate(string skytterlag, DateTime checkAgainstDate, string lisens)
        {
            try
            {
                string Washskytterlag = washstring(skytterlag);
                if (string.IsNullOrEmpty(Washskytterlag))
                {
                    return false;
                }
                Washskytterlag = Washskytterlag.ToUpper();

                var base64EncodedBytes = System.Convert.FromBase64String(lisens);

                string bufferLis = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                var splitted = bufferLis.Split(new[] { ';' });
                if (splitted[0] == Washskytterlag)
                {
                    DateTime date = DateTime.Parse(splitted[1]);

                    if (date >= checkAgainstDate)
                    {
                        return true;
                    }
                }

                return false;

            }
            catch (Exception e)
            {
                Log.Error(e, "Validate");
                return false;
            }
        }
    }
}
