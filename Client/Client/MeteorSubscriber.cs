using Net.DDP.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySurfaceApplication
{
    public class Message
    {
        [JsonProperty("msg")]
        public string Type { get; set; }
    }

    public abstract class Collection : Message
    {
        [JsonProperty("collection")]
        public string CollectionName { get; set; }

        public string Id { get; set; }

        public Dictionary<string, object> Fields { get; set; }
    }

    public class AddedMessage : Message
    {
        public string Collection { get; set; }

        public string Id { get; set; }

        public Dictionary<string, object> Fields { get; set; }
    }

    public class Childs : Message
    {
        public string name { get; set; }

        public IEnumerable<IDictionary<string, object>> childs { get; set; }
    }

    public class Effect : Message
    {
        public string Name { get; set; }
        public float Value { get; set; }

        public string serializeToJSon()
        {
            string effectJSon = "";
            string varValue = this.Value.ToString().Replace(",", ".");
            effectJSon += "{\"name\":\"" + this.Name + "\",\"value\":" + varValue + "}";
            return effectJSon;
        }
    }

    public class LeapGesturesMapping : Message
    {
        public List<string> X { get; set; }
        public List<string> Y { get; set; }
        public List<string> Pitch { get; set; }
    }

    public class ChangedMessage : Message
    {
        public string Collection { get; set; }

        public string Id { get; set; }

        public Dictionary<string, object> Fields { get; set; }
    }

    public class MeteorSubscriber : IDataSubscriber
    {
        //
        Dictionary<string, Sample> samplesMap;
        SampleData samplesList;
        jamTracksData jamTracksList;
        JamData jamList;
        ZouzouData zouzouList;
        TrackGroupsData trackGroupsList;
        Object thisLock;

        Logger info = new Logger("MeteorSubscriber.log");

        private static List<Message> _messages = new List<Message>();

        public DDPClient Client { get; set; }

        private readonly Dictionary<string, List<IBinding<object>>> _bindings = new Dictionary<string, List<IBinding<object>>>();

        public MeteorSubscriber(ref Object thisLock, ref SampleData samplesList, ref jamTracksData jamTracksList, ref JamData jamList, ref ZouzouData zouzouList, 
            ref ConnexionData samplesMap, ref TrackGroupsData trackGroupsList)
        {
            this.samplesMap = samplesMap;
            this.samplesList = samplesList;
            this.jamTracksList = jamTracksList;
            this.jamList = jamList;
            this.zouzouList = zouzouList;
            this.trackGroupsList = trackGroupsList;
            this.thisLock = thisLock;
        }

        public void DataReceived(string data)
        {
            info.log(data);

            var message = JsonConvert.DeserializeObject<Message>(data);
            string myJamId = "";

            switch (message.Type)
            {
                case "ping":
                    Client.Pong();
                    break;

                case "added":
                    var added = JsonConvert.DeserializeObject<AddedMessage>(data);
                    var bindings = _bindings[added.Collection];

                    foreach (var binding in bindings)
                    {
                        if (added.Collection == "samples")
                        {
                            var childs = JsonConvert.DeserializeObject<Childs[]>(added.Fields["childs"].ToString());
                            for (int k = 0; k < childs.Count(); k++)
                            {
                                var childsK = childs[k].childs;
                                string instrumentName = childs[k].name.ToString();
                                for (int i = 0; i < childsK.Count(); i++)
                                {
                                    var childsI = childs[k].childs.ElementAt(i);
                                    var currentSample = new Sample(childsI.Values.ElementAt(0).ToString(), childsI.Values.ElementAt(1).ToString(), instrumentName);
                                    samplesList.Add(currentSample);
                                    samplesMap.Add(childsI.Values.ElementAt(1).ToString(), currentSample);
                                    for (int j = 0; j < childsI.Count; j++)
                                    {
                                        var key = childs[k].childs.ElementAt(i).ElementAt(j).Key.ToString();
                                        var value = childs[k].childs.ElementAt(i).ElementAt(j).Value.ToString();
                                        if (key == "name")
                                        {

                                        }
                                        else if (key == "path")
                                        {

                                        }
                                    }
                                }
                            }
                        }
                        else if (added.Collection == "jam")
                        {
                            string id = added.Id;
                            string jamName = added.Fields["name"].ToString();
                            jamList.Add(new Jam(id, jamName));                            
                        }
                        else if (added.Collection == "jam-tracks")
                        {
                            string id = added.Id;
                            string jamId = added.Fields["jamId"].ToString();
                            string zouzouColor = added.Fields["zouzou"].ToString();
                            string path = added.Fields["path"].ToString();

                            jamTracksList.Add(new JamTracks(id, jamId, zouzouColor, path));

                        }
                        else if (added.Collection == "zouzous")
                        {
                            string id = added.Id;
                            string jamId = added.Fields["jamId"].ToString();
                            string zouzouColor = added.Fields["hexId"].ToString();
                            string zouzouName;
                            try
                            {
                                zouzouName = added.Fields["nickname"].ToString();
                            }
                            catch (KeyNotFoundException e)
                            {
                                zouzouName = "Boloss";
                            };

                            zouzouList.Add(new Zouzou(id, jamId, zouzouColor, zouzouName));
                        }
                        else if (added.Collection == "track-groups")
                        {
                            string id = added.Id;
                            string color = added.Fields["color"].ToString();
                            string name = added.Fields["name"].ToString();
                            string jamId = added.Fields["jamId"].ToString();
                            string tracks = added.Fields["tracks"].ToString();
                            var trackTab = JsonConvert.DeserializeObject<List<string>>(added.Fields["tracks"].ToString());
                            var effects = JsonConvert.DeserializeObject<List<Effect>>(added.Fields["effects"].ToString());
                            var leapGesturesMapping = JsonConvert.DeserializeObject<LeapGesturesMapping>(added.Fields["leapGesturesMapping"].ToString());
                            TrackGroups trackGroups = new TrackGroups(id, jamId, color, name, trackTab, effects, leapGesturesMapping);
                            trackGroupsList.Add(trackGroups);
                            info.log(trackGroups.Name);
                        }
                    }
                    if (myJamId.Length > 0)
                    {
                        this.Bind(_messages, "jam", "jam", myJamId);
                        this.Bind(_messages, "jam-tracks", "jam-tracks", myJamId);
                    }
                    break;
                case "removed":
                    var removed = JsonConvert.DeserializeObject<AddedMessage>(data);
                    var removedBindings = _bindings[removed.Collection];

                    foreach (var binding in removedBindings)
                    {
                        if (removed.Collection == "jam-tracks")
                        {
                            string id = removed.Id;
                            jamTracksList.Remove(id);
                        }
                        if (removed.Collection == "track-groups")
                        {
                            trackGroupsList.Remove(removed.Id);
                        }
                    }
                    break;
                case "changed":
                    var changed = JsonConvert.DeserializeObject<AddedMessage>(data);
                    var changedBindings = _bindings[changed.Collection];

                    foreach (var binding in changedBindings)
                    {
                        if (changed.Collection == "track-groups")
                        {
                            TrackGroups trackGroups = trackGroupsList.findById(changed.Id);
                            if (trackGroups != null)
                            {
                                lock (thisLock)
                                {
                                    try
                                    {
                                        var trackTab = JsonConvert.DeserializeObject<List<string>>(changed.Fields["tracks"].ToString());
                                        trackGroups.TracksId = trackTab;
                                        trackGroupsList.TracksUpdate(ref trackGroups);
                                    }
                                    catch (KeyNotFoundException e) { }
                                    try
                                    {
                                        if (!trackGroupsList.beingModified)
                                        {
                                            var effects = JsonConvert.DeserializeObject<List<Effect>>(changed.Fields["effects"].ToString());
                                            trackGroups.Effects = effects;
                                            trackGroupsList.EffectUpdate(ref trackGroups);
                                        }
                                    }
                                    catch (KeyNotFoundException e) { }
                                    try
                                    {
                                        var leapGesturesMapping = JsonConvert.DeserializeObject<LeapGesturesMapping>(changed.Fields["leapGesturesMapping"].ToString());
                                        trackGroups.LeapGesturesMapping = leapGesturesMapping;
                                        trackGroupsList.LeapUpdate(ref trackGroups);
                                    }
                                    catch (KeyNotFoundException e) { }
                                }
                            }
                            
                        }
                    }
                    break;
            }
        }

        public void Bind<T>(List<T> list, string collectionName, string subscribeTo, params string[] args)
            where T : new()
        {
            if (!_bindings.ContainsKey(collectionName))
                _bindings.Add(collectionName, new List<IBinding<object>>());

            _bindings[collectionName].Add(new Binding<object>(list));

            Client.Subscribe(subscribeTo, args);
        }

        private interface IBinding<T>
            where T : new()
        {
            T Target { get; }
        }

        private class Binding<T> : IBinding<T>
            where T : new()
        {
            public T Target { get; private set; }

            public Binding(T target)
            {
                Target = target;
            }

            public string ToString()
            {
                return Target.ToString();
            }
        }
    }
}
