using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLO365
{
    public static class Utility
    {
        public static string fnConvertToBase64String(string objString)
        {
            string strModified = "";
            try
            {
                byte[] byt = System.Text.Encoding.UTF8.GetBytes(objString);
                strModified = Convert.ToBase64String(byt);
            }
            catch (Exception ex)
            {

            }
            return strModified;
        }

        public static string fnConvertFromBase64String(string objString)
        {
            string strOriginal = "";
            try
            {
                byte[] b = Convert.FromBase64String(objString);
                strOriginal = System.Text.Encoding.UTF8.GetString(b);
            }
            catch (Exception ex)
            {

            }
            return strOriginal;
        }
    }
}
