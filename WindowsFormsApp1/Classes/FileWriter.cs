using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Classes
{
    class FileWriter
    {
        private string writePath = @"D:\Projects\test.txt";
        public void WriteToFile(string StringToWrite)
        {
            StreamWriter sw = new StreamWriter(writePath, true, System.Text.Encoding.Default);
            sw.WriteLine(StringToWrite);
            sw.Close();
        }
    }
}
