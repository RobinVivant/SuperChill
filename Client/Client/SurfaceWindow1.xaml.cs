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
    public class MeteorSubscriber : IDataSubscriber
    {
        public DDPClient Client { get; set; }

        private readonly Dictionary<string, List<IBinding<object>>> _bindings = new Dictionary<string, List<IBinding<object>>>();

        public MeteorSubscriber()
        {

        }

        public void DataReceived(string data)
        {
            Console.WriteLine(data);

            var message = JsonConvert.DeserializeObject<Message>(data);

            switch (message.Type)
            {
                case "added":
                    var added = JsonConvert.DeserializeObject<AddedMessage>(data);

                    var bindings = _bindings[added.Collection];

                    foreach (var binding in bindings)
                    {
                        Console.WriteLine(binding);
                    }
                    //subscription.onEvent += (s, e) =>
                    //{
                    //    Console.WriteLine(e.Data);

                    //    var message = JsonConvert.DeserializeObject<Message>(e.Data);

                    //    switch (message.Type)
                    //    {
                    //        case "added":

                    //            var fields = e.Data.GetFields();

                    //            var newObject = JsonConvert.DeserializeObject<T>(fields);

                    //            list.Add(newObject);
                    //            break;
                    //    }

                    //    Console.WriteLine();
                    //};

                    Console.WriteLine();
                    break;
            }

            Console.WriteLine();

            //try
            //{
            //    if (data.type == "added")
            //    {
            //        Console.WriteLine(data.prodCode + ": " + data.prodName + ": collection: " + data.collection);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}
        }

        public void Bind<T>(List<T> list, string collectionName, string subscribeTo)
            where T : new()
        {
            if (!_bindings.ContainsKey(collectionName))
                _bindings.Add(collectionName, new List<IBinding<object>>());

            _bindings[collectionName].Add(new Binding<object>(list));

            Client.Subscribe(subscribeTo);
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

    public class ChangedMessage : Message
    {
        public string Collection { get; set; }

        public string Id { get; set; }

        public Dictionary<string, object> Fields { get; set; }
    }

    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1 : SurfaceWindow
    {
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

            /*
            IDataSubscriber subscriber = new Subscriber();
            DDPClient client = new DDPClient(subscriber);

            Console.WriteLine("yo !! " + client.ToString());
            client.Connect("superchill.meteor.com");
            Console.WriteLine("bitch !! "+client.ToString());
            client.Subscribe("samples");
            Console.WriteLine("what's up ?? " + client.ToString());
            client.Call("hiBitch", "prout");*/
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