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
using System.Collections.ObjectModel;


namespace MySurfaceApplication
{
    public partial class SurfaceWindow1 : SurfaceWindow
    {
        ConnexionData samplesMap;
        SampleData samplesList;
        jamTracksData jamTracksList;
        JamData jamList;
        ZouzouData zouzouList;
        private SoundManager manager;
        ObservableCollection<Jam> jams = new ObservableCollection<Jam>();
        MeteorSubscriber subscriber;

        //LeapListener LeapListener;
        //Leap.Controller LeapController;

        Object thisLock = new Object();

        // Name d'un ScatterViewItem = beginningLetter + trackId 
        //car Name doit obligatoirement commencer par une lettre
        string beginningLetter = "k";

        Logger info = new Logger("Surface.log");
        private static List<Message> _messages = new List<Message>();

        public SurfaceWindow1()
        {
            InitializeComponent();

            // Initialize tag definitions
            InitializeDefinitions();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            this.manager = new SoundManager();
            samplesMap = new ConnexionData();
            samplesMap.PropertyChanged += new PropertyChangedEventHandler(samplesChangedHandler);

            // Create a sample listener and controller
            /*LeapListener = new LeapListener();
            LeapController = new Leap.Controller();

            LeapListener.OnHandOrientationChange += new LeapListener.onHandOrientationChange(handleLeapMotion);

            // Have the sample listener receive events from the controller
            LeapController.AddListener(LeapListener);*/

            jamList = new JamData();
            jamList.PropertyChanged += new PropertyChangedEventHandler(jamChangedHandler);
            zouzouList = new ZouzouData();
            zouzouList.PropertyChanged += new PropertyChangedEventHandler(zouzouChangedHandler);
            jamTracksList = new jamTracksData();
            jamTracksList.PropertyChanged += new PropertyChangedEventHandler(jamTracksChangedHandler);
            samplesList = new SampleData();
            samplesList.PropertyChanged += new PropertyChangedEventHandler(samplesChangedHandler);

            subscriber = new MeteorSubscriber(ref samplesList, ref jamTracksList, ref jamList, ref zouzouList, ref samplesMap);
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

        // LEAP MOTION
        void handleLeapMotion(float pitch, float roll, float yaw)
        {
            lock (thisLock)
            {
                Console.WriteLine("pitch : " + pitch + " roll : " + roll + " yaw : " + yaw);
            }
        }

        // Dessine un cercle sur la table surface correspondant à une track (avec la couleur du zouzou et le nom de la track)
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
                    //item.PreviewTouchUp += new EventHandler<TouchEventArgs>(handle_TouchUp);
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

        // Supprime le cercle sur la table surface, correspondant à une track 
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

            if (item.Opacity == 0.5)
            {
                item.Opacity = 1;
            }
            else
            {
                item.Opacity = 0.5;
            }
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
            Jam jam = sender as Jam;
            if (e.PropertyName == "Added")
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    jams.Add(jam);
                    drawJam(jam);
                });
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

            // Remove the sample listener when done
            //LeapController.RemoveListener(LeapListener);
            //LeapController.Dispose();

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

        // Choix initial du jam

        ObservableCollection<Jam> JamsListBox
        {
            get { return jams; }
        }

        public void drawJam(Jam jam)
        {

            myScatterView.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(delegate()
                {
                    Border border = new Border();
                    var converter = new System.Windows.Media.BrushConverter();
                    border.BorderBrush = Brushes.AliceBlue;
                    border.Background = Brushes.LightSkyBlue;
                    //border.BorderBrush = Brushes.White;
                    //border.BorderThickness = new Thickness(5);
                    border.CornerRadius = new CornerRadius(130);
                    border.Height = 200;
                    border.Width = 200;

                    TextBlock content = new TextBlock();
                    content.Text = jam.Name;
                    content.Foreground = new SolidColorBrush(Colors.White);
                    content.FontWeight = FontWeights.Bold;
                    content.FontSize = 30;
                    content.Height = 40;
                    content.Width = 100;
                    content.HorizontalAlignment = HorizontalAlignment.Center;
                    content.VerticalAlignment = VerticalAlignment.Center;
                    content.TextAlignment = TextAlignment.Center;
                    content.TextWrapping = TextWrapping.Wrap;

                    border.Child = content;
                    ScatterViewItem item = new ScatterViewItem();
                    item.Name = beginningLetter + jam.Id;
                    myScatterView.RegisterName(item.Name, item);
                    //item.PreviewTouchDown += new EventHandler<TouchEventArgs>(handle_TouchDown);
                    //item.PreviewTouchUp += new EventHandler<TouchEventArgs>(handle_TouchUp);
                    TouchExtensions.AddTapGestureHandler(item, new EventHandler<TouchEventArgs>(handle_JamTapGesture));
                    item.PreviewMouseUp += new MouseButtonEventHandler(handle_JamMouseUp);
                    item.Content = border;
                    item.Background = new SolidColorBrush(Colors.Transparent);
                    item.Opacity = 0.85;
                    item.MinHeight = 200;
                    item.MinWidth = 200;
                    myScatterView.Items.Add(item);
                })
            );
        }

        private void handle_JamTapGesture(object sender, TouchEventArgs e)
        {
            ScatterViewItem item = sender as ScatterViewItem;
            string jamId = item.Name.Substring(1, item.Name.Length - 1);
            myScatterView.Items.Clear();
            subscriber.Bind(_messages, "jam", "jam", jamId);
            subscriber.Bind(_messages, "jam-tracks", "jam-tracks", jamId);
        }

        private void handle_JamMouseUp(object sender, MouseButtonEventArgs e)
        {
            ScatterViewItem item = sender as ScatterViewItem;
            string jamId = item.Name.Substring(1, item.Name.Length - 1);
            myScatterView.Items.Clear();
            subscriber.Bind(_messages, "jam", "jam", jamId);
            subscriber.Bind(_messages, "jam-tracks", "jam-tracks", jamId);
            subscriber.Bind(_messages, "track-groups", "track-groups", jamId);  
        }

        //TAG

        private void InitializeDefinitions()
        {
            for (byte k = 1; k <= 7; k++)
            {
                TagVisualizationDefinition tagDef = new TagVisualizationDefinition();
                // The tag value that this definition will respond to.
                tagDef.Value = k;
                // The .xaml file for the UI
                tagDef.Source = new Uri("TagVisualization1.xaml", UriKind.Relative);
                // The maximum number for this tag value.
                tagDef.MaxCount = 2;
                // The visualization stays for 1 seconds.
                tagDef.LostTagTimeout = 1000.0;
                // Orientation offset (default).
                tagDef.OrientationOffsetFromTag = 0.0;
                // Tag removal behavior (default).
                tagDef.TagRemovedBehavior = TagRemovedBehavior.Fade;
                // Orient UI to tag? (default).
                tagDef.UsesTagOrientation = true;
                // Add the definition to the collection.
                MyTagVisualizer.Definitions.Add(tagDef);
            }
        }

        private ScatterViewItem OnItem(Point center)
        {
            for (int i = 0; i < myScatterView.Items.Count; i++)
            {
                ScatterViewItem item = (ScatterViewItem)myScatterView.Items.GetItemAt(i);
                Point p = item.ActualCenter;
                var distance = Math.Sqrt(Math.Pow(p.X - center.X, 2) + Math.Pow(p.Y - center.Y, 2));
                if (distance <= 50)
                {
                    return item;
                }
            }
            return null;
        }

        private void OnVisualizationAdded(object sender, TagVisualizerEventArgs e)
        {
            TagVisualization1 filter = (TagVisualization1)e.TagVisualization;
            filter.OriginalOrientation = filter.Orientation;
            ScatterViewItem item = OnItem(filter.Center);
            if (item != null)
            {
                JamTracks jamTracks = jamTracksList.findById(item.Name.Substring(1, item.Name.Length - 1));
                filter.associatedJamTracks = jamTracks;
            }
            switch (filter.VisualizedTag.Value)
            {
                case 1:
                    filter.FilterType.Content = "Volume";
                    filter.myEllipse.Fill = SurfaceColors.Accent1Brush;
                    filter.Effect = SoundEffect.Volume;
                    break;
                case 2:
                    filter.FilterType.Content = "Chorus";
                    filter.myEllipse.Fill = SurfaceColors.Accent2Brush;
                    filter.Effect = SoundEffect.Chorus;
                    break;
                case 3:
                    filter.FilterType.Content = "Echo";
                    filter.myEllipse.Fill = SurfaceColors.Accent3Brush;
                    filter.Effect = SoundEffect.Echo;
                    break;
                case 4:
                    filter.FilterType.Content = "Flanger";
                    filter.myEllipse.Fill = SurfaceColors.Accent4Brush;
                    filter.Effect = SoundEffect.Flanger;
                    break;
                case 5:
                    filter.FilterType.Content = "Gargle";
                    filter.myEllipse.Fill = SurfaceColors.BulletBrush;
                    filter.Effect = SoundEffect.Gargle;
                    break;
                case 6:
                    filter.FilterType.Content = "Waves Reverb";
                    filter.myEllipse.Fill = SurfaceColors.BulletDisabledBrush;
                    filter.Effect = SoundEffect.WavesReverb;
                    break;
                default:
                    filter.FilterType.Content = "UNKNOWN FILTER";
                    filter.myEllipse.Fill = SurfaceColors.ControlAccentBrush;
                    filter.Effect = SoundEffect.Volume;
                    break;
            }
        }

        private void MyTagVisualizer_VisualizationMoved(object sender, TagVisualizerEventArgs e)
        {
            TagVisualization1 filter = (TagVisualization1)e.TagVisualization;
            float val = 0;
            if (filter.associatedJamTracks != null)
            {
                // Quand on tourne le tag à droite
                if (filter.Valeur - filter.Orientation < 0)
                {                    
                    if (filter.Orientation >= filter.OriginalOrientation)
                    {
                        val = (float)Math.Abs((filter.Orientation - filter.OriginalOrientation)) / 360;                        
                    }
                    else // on depasse 360 et on repasse à 0
                    {
                        val = (float)Math.Abs(filter.Orientation + (360 - filter.OriginalOrientation)) / 360;
                    }
                }
                // Quand on tourne le tag à gauche
                else
                {
                    if (filter.Orientation <= filter.OriginalOrientation)
                    {
                        val = (float)Math.Abs(filter.OriginalOrientation - filter.Orientation) / 360;                        
                    }
                    else // on passe en dessous de 0 et on repasse à 360
                    {
                        val = (float)Math.Abs(Math.Abs(filter.OriginalOrientation + 360 - filter.Orientation)) / 360;
                    }
                }
                filter.Opacity = val/360;
                manager.setEffectOnLoop(filter.associatedJamTracks.Id, filter.Effect, val);
                //filter.GeneralEffectValue = filter.associatedJamTracks.Effect1;
            }
            filter.Opacity = 1;
        }
    }
}
