using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using Newtonsoft.Json;
using Net.DDP.Client;

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

    public class Childs
    {
        public string name { get; set; }

        public Dictionary<string, object> childs { get; set; }
    }

    public class ChangedMessage : Message
    {
        public string Collection { get; set; }

        public string Id { get; set; }

        public Dictionary<string, object> Fields { get; set; }
    }
    public class MeteorSubscriber : IDataSubscriber
    {
        Logger info = new Logger("MeteorSubscriber.log");

        public DDPClient Client { get; set; }

        private readonly Dictionary<string, List<IBinding<object>>> _bindings = new Dictionary<string, List<IBinding<object>>>();

        public MeteorSubscriber()
        {
        }

        public void DataReceived(string data)
        {
            info.log(data);

            var message = JsonConvert.DeserializeObject<Message>(data);

            switch (message.Type)
            {
                case "added":
                    var added = JsonConvert.DeserializeObject<AddedMessage>(data);

                    var bindings = _bindings[added.Collection];

                    foreach (var binding in bindings)
                    {
                        if (added.Collection == "samples") {
                            string name = added.Fields["name"].ToString();
                            //var childs = JsonConvert.DeserializeObject<List<AddedMessage>>(added.Fields["childs"].ToString());
                            string instrumentName = "";
                            //string specificName = childs.childs["name"].ToString();
                            info.log(instrumentName);
                        }
                        else if (added.Collection == "jam")
                        {
                            string id = added.Id;
                            string jamName = added.Fields["name"].ToString();
                        }
                        else if (added.Collection == "jamList")
                        {
                            string id = added.Id;
                            string jamName = added.Fields["name"].ToString();
                        }
                        else if (added.Collection == "jam-tracks")
                        {
                            string id = added.Id;
                            string jamId = added.Fields["jamId"].ToString();
                            string zouzouColor = added.Fields["zouzou"].ToString();
                            string path = added.Fields["path"].ToString();
                        }
                        else if (added.Collection == "zouzous")
                        {
                            string id = added.Id;
                            string jamId = added.Fields["jamId"].ToString();
                            string zouzouColor = added.Fields["hexId"].ToString();
                            string zouzouName = added.Fields["nickname"].ToString();
                        }
                    }
                    
                    break;
            }            
        }

        public void Bind<T>(List<T> list, string collectionName, string subscribeTo, params string [] args)
            where T : new()
        {
            if (!_bindings.ContainsKey(collectionName))
                _bindings.Add(collectionName, new List<IBinding<object>>());

            _bindings[collectionName].Add(new Binding<object>(list));

            Client.Subscribe(subscribeTo,args);
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

        //private class CollectionBinding<L, T> : IBinding
        //    where L : IList<T>
        //{
        //    private L _target;
        //    private T _generic;

        //    public CollectionBinding(L target, T generic)
        //    {
        //        _target = target;
        //        _generic = generic;
        //    }
        //}
    }

    

    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1 : SurfaceWindow
    {
        Logger info = new Logger("Surface.log");

        private static List<Message> _messages = new List<Message>(); 
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();


            var subscriber = new MeteorSubscriber();
            var client = new DDPClient(subscriber);

            // TODO; hack
            subscriber.Client = client;

            client.Connect("superchill.meteor.com");

            subscriber.Bind(_messages, "samples", "samples");
            subscriber.Bind(_messages, "jam", "jam", "Zx4duhaPxeL9WRf7u");
            subscriber.Bind(_messages, "jamList", "jamList");
            subscriber.Bind(_messages, "jam-tracks", "jam-tracks", "Zx4duhaPxeL9WRf7u");
            subscriber.Bind(_messages, "zouzous", "zouzouList");
        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }
    }
}