using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySurfaceApplication
{
    public class Jam
    {
        private string id;
        private string name;
        private string leapTarget;

        public Jam(string id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public string Id
        {
            get{ return id;}
            set{ ;}
        }

        public string LeapTarget
        {
            get { return leapTarget; }
            set { this.leapTarget=value;}
        }

        public string Name
        {
            get { return name;}
            set { ;}
        }


        public override string ToString()
        {
            return name;
        }
    }
}
