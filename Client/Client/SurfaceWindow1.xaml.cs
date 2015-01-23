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
using System.Windows.Threading;
using System.ComponentModel;

namespace MySurfaceApplication
{
    public partial class SurfaceWindow1 : SurfaceWindow
    {

        ConnexionData samplesMap;
        jamTracksData jamTracksList;
        JamData jamList;
        ZouzouData zouzouList;
        private SoundManager manager;
        // Name d'un ScatterViewItem = beginningLetter + trackId 
        //car Name doit obligatoirement commencer par une lettre
        string beginningLetter = "k";

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

        // Dessine un cercle sur la table surface correspondant � une track (avec la couleur du zouzou et le nom de la track)
        public void drawCircle(string trackId, string trackPath, string zouzouColor)
        {
            string trackName;
            char[] delimiterChars = { '/', '.' };
            string[] words = trackPath.Split(delimiterChars);
            trackName = words[words.Length - 2];

            //Console.WriteLine(trackName);
            //Console.WriteLine(zouzouColor);

            myScatterView.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(delegate()
                {
                    Border border = new Border();
                    var converter = new System.Windows.Media.BrushConverter();
                    var color = (Brush)converter.ConvertFromString("#" + zouzouColor);
                    border.BorderBrush = color;
                    border.Background = color;
                    //border.BorderBrush = Brushes.White;
                    //border.BorderThickness = new Thickness(5);
                    border.CornerRadius = new CornerRadius(130);
                    border.Height = 130;
                    border.Width = 130;

                    TextBlock content = new TextBlock();
                    content.Text = trackName;
                    content.Foreground = new SolidColorBrush(Colors.White);
                    content.FontWeight = FontWeights.Bold;
                    content.FontSize = 14;
                    content.Height = 40;
                    content.Width = 100;
                    content.HorizontalAlignment = HorizontalAlignment.Center;
                    content.VerticalAlignment = VerticalAlignment.Center;
                    content.TextAlignment = TextAlignment.Center;
                    content.TextWrapping = TextWrapping.Wrap;

                    border.Child = content;
                    ScatterViewItem item = new ScatterViewItem();
                    item.Name = beginningLetter + trackId;
                    myScatterView.RegisterName(item.Name, item);
                    //item.PreviewTouchDown += new EventHandler<TouchEventArgs>(handle_TouchDown);
                    //item.PreviewMouseDown += new MouseButtonEventHandler(handle_MouseDown);
                    //item.PreviewTouchUp += new EventHandler<TouchEventArgs>(handle_TouchUp);
                    //item.PreviewMouseUp += new MouseButtonEventHandler(handle_MouseUp);
                    TouchExtensions.AddPreviewHoldGestureHandler(item, new EventHandler<TouchEventArgs>(handle_HoldGesture));
                    TouchExtensions.AddHoldGestureHandler(item, new EventHandler<TouchEventArgs>(handle_HoldGesture));
                    TouchExtensions.AddTapGestureHandler(item, new EventHandler<TouchEventArgs>(handle_TapGesture));
                    item.PreviewMouseUp += new MouseButtonEventHandler(handle_MouseUp);
                    item.Content = border;
                    item.Background = new SolidColorBrush(Colors.Transparent);
                    item.Opacity = 0.5;
                    myScatterView.Items.Add(item);
                })
            );
        }

        // Supprime le cercle sur la table surface, correspondant � une track 
        public void removeCircle(string trackId)
        {
            myScatterView.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(delegate()
                {
                    object objectScatterViewItem = myScatterView.FindName(beginningLetter + trackId);
                    if (objectScatterViewItem is ScatterViewItem)
                    {
                        ScatterViewItem wantedChild = objectScatterViewItem as ScatterViewItem;
                        myScatterView.Items.Remove(wantedChild);
                    }
                })
            );
        }

        private void handle_HoldGesture(object sender, TouchEventArgs e)
        {
            // Do nothing
            e.Handled = true;
        }

        private void handle_TapGesture(object sender, TouchEventArgs e)
        {
            ScatterViewItem item = sender as ScatterViewItem;
            string trackId = item.Name.Substring(1, item.Name.Length - 1);
            manager.toggleLoop(trackId);
            if (item.Opacity == 0.5)
            {
                item.Opacity = 1;
            }
            else
            {
                item.Opacity = 0.5;
            }
        }

        private void handle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ScatterViewItem item = sender as ScatterViewItem;
            string trackId = item.Name.Substring(1, item.Name.Length - 1);
            manager.toggleLoop(trackId);
            //string pos = item.ActualCenter.ToString();
            //Console.WriteLine(pos);

            if (item.Opacity == 0.5)
            {
                item.Opacity = 1;
            }
            else
            {
                item.Opacity = 0.5;
            }
        }

        private void handle_TouchUp(object sender, TouchEventArgs e)
        {
            ScatterViewItem item = sender as ScatterViewItem;
            string trackId = item.Name.Substring(1, item.Name.Length - 1);
            manager.toggleLoop(trackId);
            if (item.Opacity == 0.5)
            {
                item.Opacity = 1;
            }
            else
            {
                item.Opacity = 0.5;
            }
        }

        public class MeteorSubscriber : IDataSubscriber
        {
            //
            List<Sample> sampleList;
            ConnexionData samplesMap;
            jamTracksData jamTracksList;
            JamData jamList;
            ZouzouData zouzouList;

            
            Logger info = new Logger("MeteorSubscriber.log");
            

            private static List<Message> _messages = new List<Message>();

            public DDPClient Client { get; set; }

            private readonly Dictionary<string, List<IBinding<object>>> _bindings = new Dictionary<string, List<IBinding<object>>>();

            public MeteorSubscriber(ref ConnexionData samplesMap,ref jamTracksData jamTracksList, ref JamData jamList, ref ZouzouData zouzouList)
            {
                this.samplesMap = samplesMap;
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
                                    sampleList = new List<Sample>();
                                    for (int i = 0; i < childsK.Count(); i++)
                                    {
                                        var childsI = childs[k].childs.ElementAt(i);
                                        sampleList.Add(new Sample(childsI.Values.ElementAt(0).ToString(), childsI.Values.ElementAt(1).ToString()));
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
                                    samplesMap.Add(instrumentName, sampleList);
                                }
                            }
                            else if (added.Collection == "jam")
                            {
                                string id = added.Id;
                                string jamName = added.Fields["name"].ToString();
                                jamList.Add(new Jam(id, jamName));
                                if (jamName == "Jam 2")
                                {
                                    myJamId = id;
                                }
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

        /// <summary>
        /// Interaction logic for SurfaceWindow1.xaml
        /// </summary>

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

            this.manager = new SoundManager();

            jamList = new JamData();
            jamList.PropertyChanged += new PropertyChangedEventHandler(jamChangedHandler);
            zouzouList = new ZouzouData();
            zouzouList.PropertyChanged += new PropertyChangedEventHandler(zouzouChangedHandler);
            jamTracksList = new jamTracksData();
            jamTracksList.PropertyChanged += new PropertyChangedEventHandler(jamTracksChangedHandler);
            samplesMap = new ConnexionData();
            samplesMap.PropertyChanged += new PropertyChangedEventHandler(samplesChangedHandler);

            var subscriber = new MeteorSubscriber(ref samplesMap,ref jamTracksList,ref jamList,ref zouzouList);
            var client = new DDPClient(subscriber);

            // TODO; hack
            subscriber.Client = client;

            client.Connect("superchill.meteor.com");

            // ..., nom de la collection, nom de la subscription
            subscriber.Bind(_messages, "samples", "samples");
            //subscriber.Bind(_messages, "jam", "jam", "Zx4duhaPxeL9WRf7u");
            subscriber.Bind(_messages, "jam", "jamList");
            //subscriber.Bind(_messages, "jam", "jam-tracks", "Zx4duhaPxeL9WRf7u");
            subscriber.Bind(_messages, "zouzous", "zouzouList");
        }

        protected void samplesChangedHandler(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Added")
            {

            }
        }

        protected void jamTracksChangedHandler(Object sender, PropertyChangedEventArgs e)
        {
            JamTracks jamTracks = sender as JamTracks;
            if (e.PropertyName == "Added")
            {
                drawCircle(jamTracks.Id, jamTracks.Path, jamTracks.ZouzouColor);
                manager.addLoop(jamTracks.Id, "../.." + jamTracks.Path, false);
            }
            else if (e.PropertyName == "Removed")
            {
                removeCircle(jamTracks.Id);
                manager.removeLoop(jamTracks.Id);
            }
        }

        protected void jamChangedHandler(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Added")
            {
                
            }
        }

        protected void zouzouChangedHandler(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Added")
            {

            }
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
