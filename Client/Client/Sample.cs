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
        private string type;

        public Sample(string name, string path,string type)
        {
            this.name = name;
            this.path = path;
            this.type = type;
        }

        public string Name { get { return this.name ;} set { ;} }
        public string Path { get { return this.path ;} set { ;} }
        public string Type { get { return this.type ;} set { ;} }
    }
}
