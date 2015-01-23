using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySurfaceApplication
{
    public class Zouzou
    {
        private string id;
        private string jamId;
        private string color;
        private string name;

        public Zouzou(string id,string jamId,string color, string name)
        {
            this.id = id;
            this.jamId = jamId;
            this.color = color;
            this.name = name;
        }

        public string Id
        {
            get{ return id;}
            set{ ;}
        }

        public string JamId
        {
            get { return jamId; }
            set { ;}
        }

        public string Color
        {
            get { return color; }
            set { ;}
        }

        public string Name
        {
            get { return name;}
            set { ;}
        }
    }
}
