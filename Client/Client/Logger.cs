using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySurfaceApplication
{
    class Logger
    {
        private string fileName;
        public Logger(String fileName){
            this.fileName = fileName;
        }

        public void log(String lines)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true);
            string logLine = System.String.Format("{0:G}: {1}.", System.DateTime.Now, lines);
            file.WriteLine(logLine);
            file.Close();
        }
    }
}
