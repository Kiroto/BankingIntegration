using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BankingIntegration.BankModel
{
    class Utils
    {
        public static string ReadFromStream(Stream stream)
        {
            // Might want to read with buffer
            using (StreamReader sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
