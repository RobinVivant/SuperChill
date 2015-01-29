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

        Logger info = new Logger("MeteorSubscriber.log");

        private static List<Message> _messages = new List<Message>();

        public DDPClient Client { get; set; }

        private readonly Dictionary<string, List<IBinding<object>>> _bindings = new Dictionary<string, List<IBinding<object>>>();

        public MeteorSubscriber(ref SampleData samplesList, ref jamTracksData jamTracksList, ref JamData jamList, ref ZouzouData zouzouList, ref ConnexionData samplesMap)
        {
            this.samplesMap = samplesMap;
            this.samplesList = samplesList;
            this.jamTracksList = jamTracksList;
            this.jamList = jamList;
            this.zouzouList = zouzouList;
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
                            string zouzouName = added.Fields["nickname"].ToString();

                            zouzouList.Add(new Zouzou(id, jamId, zouzouColor, zouzouName));
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
