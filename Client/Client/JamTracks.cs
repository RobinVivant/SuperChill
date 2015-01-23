using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySurfaceApplication
{

    public class JamTracks
    {
        string id;
        string jamId;
        string zouzouColor;
        string path;

        public JamTracks(string id,string jamId, string zouzouColor, string path)
        {
            this.id = id;
            this.jamId = jamId;
            this.zouzouColor = zouzouColor;
            this.path = path;
        }

        public string Id { 
            get 
            { 
                return this.id; 
            } 
            set 
            {

            } 
        }
        public string JamId { get { return this.jamId; } set { ;} }
        public string ZouzouColor { get { return this.zouzouColor; } set { ;} }
        public string Path { get { return this.path; } set { ;} }
    }
}
