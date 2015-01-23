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

        SampleData samplesMap;
        jamTracksData jamTracksList;
        JamData jamList;
        ZouzouData zouzouList;
        private SoundManager manager;

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

            jamList = new JamData();
            jamList.PropertyChanged += new PropertyChangedEventHandler(jamChangedHandler);
            zouzouList = new ZouzouData();
            zouzouList.PropertyChanged += new PropertyChangedEventHandler(zouzouChangedHandler);
            jamTracksList = new jamTracksData();
            jamTracksList.PropertyChanged += new PropertyChangedEventHandler(jamTracksChangedHandler);
            samplesMap = new SampleData();
            samplesMap.PropertyChanged += new PropertyChangedEventHandler(samplesChangedHandler);

            var subscriber = new MeteorSubscriber(ref samplesMap, ref jamTracksList, ref jamList, ref zouzouList);
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
                
        private void InitializeDefinitions()
        {
            for (byte k = 1; k <= 5; k++)
            {
                TagVisualizationDefinition tagDef =
                    new TagVisualizationDefinition();
                // The tag value that this definition will respond to.
                tagDef.Value = k;
                // The .xaml file for the UI
                tagDef.Source =
                    new Uri("TagVisualization1.xaml", UriKind.Relative);
                // The maximum number for this tag value.
                tagDef.MaxCount = 2;
                // The visualization stays for 2 seconds.
                tagDef.LostTagTimeout = 2000.0;
                // Orientation offset (default).
                tagDef.OrientationOffsetFromTag = 0.0;
                // Physical offset (horizontal inches, vertical inches).
                tagDef.PhysicalCenterOffsetFromTag = new Vector(2.0, 2.0);
                // Tag removal behavior (default).
                tagDef.TagRemovedBehavior = TagRemovedBehavior.Fade;
                // Orient UI to tag? (default).
                tagDef.UsesTagOrientation = true;
                // Add the definition to the collection.
                MyTagVisualizer.Definitions.Add(tagDef);
            }
        }

        private void OnVisualizationAdded(object sender, TagVisualizerEventArgs e)
        {
            TagVisualization1 filter = (TagVisualization1)e.TagVisualization;
            switch (filter.VisualizedTag.Value)
            {
                case 1:
                    filter.FilterType.Content = "Filtre 1";
                    filter.myEllipse.Fill = SurfaceColors.Accent1Brush;
                    break;
                case 2:
                    filter.FilterType.Content = "Filtre 2";
                    filter.myEllipse.Fill = SurfaceColors.Accent2Brush;
                    break;
                case 3:
                    filter.FilterType.Content = "Filtre 3";
                    filter.myEllipse.Fill = SurfaceColors.Accent3Brush;
                    break;
                case 4:
                    filter.FilterType.Content = "Filtre 4";
                    filter.myEllipse.Fill = SurfaceColors.Accent4Brush;
                    break;
                default:
                    filter.FilterType.Content = "UNKNOWN FILTER";
                    filter.myEllipse.Fill = SurfaceColors.ControlAccentBrush;
                    break;
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
