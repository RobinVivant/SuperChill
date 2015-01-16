using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySurfaceApplication
{
    public class Sample
    {
        private string name;
        private string path;

        public Sample(string name, string path)
        {
            this.name = name;
            this.path = path;
        }

        public string Name { get; set; }
        public string Path { get; set; }

    }
}
