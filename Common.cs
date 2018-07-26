using System;
using System.Runtime.InteropServices;

namespace HPSocketCS
{
    public class Common
    {
        /// <summary>
        /// </summary>
        /// <param name="ptr"></param>
        public static string PtrToAnsiString(IntPtr ptr)
        {
            string str = "";
            try
            {
                if (ptr != IntPtr.Zero)
                {
                    str = Marshal.PtrToStringAnsi(ptr);
                }
            }
            catch (Exception)
            {
            }
            return str;
        }
    }
}