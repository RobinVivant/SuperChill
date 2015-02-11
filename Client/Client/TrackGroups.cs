using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySurfaceApplication
{
    public class TrackGroups
    {
        private List<string> tracks;
        private List<Effect> effects;
        private string id;
        private string color;
        private string name;
        private string jamId;
        private LeapGesturesMapping leapGesturesMapping;
        private bool modified;

        public TrackGroups(string id, string jamId, string color, string name, List<string> tracks, List<Effect> effects, LeapGesturesMapping leapGesturesMapping)
        {
            this.id = id;
            this.jamId = jamId;
            this.color = color;
            this.name = name;
            this.tracks = tracks;
            this.effects = effects;
            this.leapGesturesMapping = leapGesturesMapping;
            modified = false;
        }

        public List<Effect> Effects
        {
            get { return effects; }
            set {
                effects = value;
            }
        }

        public List<string> TracksId
        {
            get { return tracks; }
            set { tracks = value; }
        }

        public LeapGesturesMapping LeapGesturesMapping
        {
            get { return leapGesturesMapping; }
            set { leapGesturesMapping = value; }
        }

        public string Id
        {
            get { return id; }
            set { ;}
        }

        public string JamId
        {
            get { return jamId; }
            set { ;}
        }

        public bool Modified
        {
            get { return modified; }
            set { modified = value;}
        }

        public string Color
        {
            get { return color; }
            set { ;}
        }

        public string Name
        {
            get { return name; }
            set { ;}
        }

        public string serializeEffectsToJSon()
        {
            string effectsJson = "{\"effects\":["; // +effects + "]}";
            foreach (Effect effect in effects)
            {
                effectsJson += effect.serializeToJSon();
                effectsJson += ",";
            }
            string result = effectsJson.Substring(0,effectsJson.Length - 1);
            result += "]}";
            return result;
        }
    }
}
