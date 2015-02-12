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
using System.Windows.Media.Animation;
using System.Threading;


namespace MySurfaceApplication
{
    public partial class SurfaceWindow1 : SurfaceWindow
    {
        private static readonly Dictionary<string, SoundEffect> effectMap = new Dictionary<string, SoundEffect>
        {
            { "volume", SoundEffect.Volume },
            { "chorus", SoundEffect.Chorus },
            { "echo", SoundEffect.Echo },
            { "flanger", SoundEffect.Flanger },
            { "gargle", SoundEffect.Gargle },
            { "reverb", SoundEffect.WavesReverb }
        };


        ConnexionData samplesMap;
        SampleData samplesList;
        jamTracksData jamTracksList;
        JamData jamList;
        ZouzouData zouzouList;
        TrackGroupsData trackGroupsList;
        private SoundManager manager;
        ObservableCollection<Jam> jams = new ObservableCollection<Jam>();
        MeteorSubscriber subscriber;

        TimerCallback callback;
        Timer t;
        Object lockTimer = new Object();

        LeapListener leapListener;
        Leap.Controller leapController;

        Object thisLock = new Object();

        // Name d'un ScatterViewItem = beginningLetter + trackId 
        // car Name doit obligatoirement commencer par une lettre
        string beginningLetter = "k";

        // Scatterviewitems position
        Random rand = new Random();
        TouchDevice ellipseControlTouchDevice;
        Point lastPoint;

        Logger info = new Logger("Surface.log");
        private static List<Message> _messages = new List<Message>();
        private long serverUpdateInterval = 200;

        public SurfaceWindow1()
        {
            InitializeComponent();

            // Initialize tag definitions
            InitializeDefinitions();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            this.manager = new SoundManager(this);
            
            samplesMap = new ConnexionData();
            samplesMap.PropertyChanged += new PropertyChangedEventHandler(samplesChangedHandler);

            // Create a sample listener and controller
            leapListener = new LeapListener();
            leapController = new Leap.Controller();


            leapListener.OnHandVariation += new LeapListener.onHandVariation(handleHandVariation);
            leapListener.OnHandLeaving += new LeapListener.onHandLeaving(handleHandLeaving);
            // Have the sample listener receive events from the controller
            leapController.AddListener(leapListener);
                        
            jamList = new JamData();
            jamList.PropertyChanged += new PropertyChangedEventHandler(jamChangedHandler);
            zouzouList = new ZouzouData();
            zouzouList.PropertyChanged += new PropertyChangedEventHandler(zouzouChangedHandler);
            jamTracksList = new jamTracksData();
            jamTracksList.PropertyChanged += new PropertyChangedEventHandler(jamTracksChangedHandler);
            samplesList = new SampleData();
            samplesList.PropertyChanged += new PropertyChangedEventHandler(samplesChangedHandler);
            trackGroupsList = new TrackGroupsData();
            trackGroupsList.PropertyChanged += new PropertyChangedEventHandler(trackGroupsChangedHandler);

            // Timer
            callback = new TimerCallback(Tick);
            t = new Timer(delegate { callback(trackGroupsList); }, null, 0, serverUpdateInterval);

            subscriber = new MeteorSubscriber(ref thisLock,ref samplesList, ref jamTracksList, ref jamList, ref zouzouList, ref samplesMap, ref trackGroupsList);
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

        public void refreshEffectView(String trackId, SoundEffect e, float val)
        {
            myScatterView.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                ((Image)myScatterView.FindName("fx_" + trackId + "_" + e.ToString())).Source = new BitmapImage(new Uri(@"../../Resources/loader/Untitled-" + (int)(10 * val) + ".png", UriKind.Relative));
            }));
        }
            
        private void handleHandLeaving(bool isNotPresent)
        {
            trackGroupsList.beingModified = !isNotPresent;
        }

        public void Tick(Object o)
        {
            lock (thisLock)
            {
                var tgl = (TrackGroupsData)o;
                foreach (TrackGroups tg in tgl)
                {
                    if (tg.Modified)
                    {
                        trackGroupsUpdate(tg);
                        tg.Modified = false;
                    }
                }
            }
        }

        // LEAP MOTION
        void handleLeapMotion(float pitch, float roll, float yaw)
        {
            lock (thisLock)
            {
                Console.WriteLine("pitch : " + pitch + " roll : " + roll + " yaw : " + yaw);
            }
        }

        private Point RandCenter(int centerX, int centerY)
        {
            int randx, randy;

            if (centerX > 200)
            {
                randx = rand.Next(centerX - 200, centerX + 200);
            }
            else
            {
                randx = rand.Next(centerX, centerX + 200);
            }
            if (centerY > 200)
            {
                randy = rand.Next(centerY - 200, centerY + 200);
            }
            else
            {
                randy = rand.Next(centerY, centerY + 200);
            }
            return new Point(randx, randy);
        }

        private int RandOrientation(int low, int up)
        {
            int randor = rand.Next(low, up);
            return randor;
        }

        // Draw timer in the middle of the scatterview
        public void drawTimer()
        {
            List<Ellipse> ellipseList = new List<Ellipse>();
            Canvas c = new Canvas();
            int ellipseDiameter = 12;
            int canvasDiameter = 100;
            int i;
            int j = 0;
            //var converter = new System.Windows.Media.BrushConverter();
            //var grayColor = (SolidColorBrush)converter.ConvertFromString("#293133");

            c.Height = canvasDiameter;
            c.Width = canvasDiameter;
            c.Background = new SolidColorBrush(Colors.Transparent);

            for (i = 0; i < 8; i++)
            {
                Ellipse e = new Ellipse();
                e.Stroke = System.Windows.Media.Brushes.White; //System.Windows.Media.Brushes.White;
                e.Fill = System.Windows.Media.Brushes.White;   //System.Windows.Media.Brushes.DimGray;
                e.Opacity = 0.2;
                e.Width = ellipseDiameter;
                e.Height = ellipseDiameter;
                e.Name = "ellipse" + i;
                myScatterView.RegisterName(e.Name, e);

                double x = (canvasDiameter / 2) + 30 * Math.Cos(i * Math.PI / 4);
                double y = (canvasDiameter / 2) + 30 * Math.Sin(i * Math.PI / 4);

                e.SetValue(Canvas.LeftProperty, x);
                e.SetValue(Canvas.TopProperty, y);

                c.Children.Add(e);
                ellipseList.Add(e);
            }

            ScatterViewItem item = new ScatterViewItem();
            item.Width = canvasDiameter;
            item.Height = canvasDiameter;
            item.Content = c;
            item.Background = new SolidColorBrush(Colors.Transparent);
            item.CanScale = false;
            item.CanRotate = false;
            item.CanMove = false;
            item.Center = new Point(myScatterView.ActualWidth / 2, myScatterView.ActualHeight / 2);

            item.ApplyTemplate();
            Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
            ssc = item.Template.FindName("shadow", item) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
            ssc.Visibility = Visibility.Hidden;

            myScatterView.Items.Add(item);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
            timer.Tick += (s, ev) =>
            {
                foreach (Ellipse e in c.Children)
                {
                    if (j == 8)
                    {
                        j = 0;
                    }
                    DoubleAnimation opacityAnimation = null;
                    if (Math.Round(e.Opacity, 1) == 0.2)
                    {
                        opacityAnimation = new DoubleAnimation(0.2, 1.0, TimeSpan.FromSeconds(1), FillBehavior.Stop);
                    }
                    else
                    {
                        opacityAnimation = new DoubleAnimation(1.0, 0.2, TimeSpan.FromSeconds(1), FillBehavior.Stop);
                    }
                    opacityAnimation.BeginTime = TimeSpan.FromSeconds(j);
                    opacityAnimation.AccelerationRatio = 0.5;
                    opacityAnimation.DecelerationRatio = 0.5;
                    opacityAnimation.FillBehavior = FillBehavior.Stop;
                    opacityAnimation.Completed += delegate(object send, EventArgs evt)
                    {
                        if (Math.Round(e.Opacity, 1) == 0.2)
                        {
                            e.Opacity = 1.0;
                        }
                        else
                        {
                            e.Opacity = 0.2;
                        }
                    };
                    e.BeginAnimation(Ellipse.OpacityProperty, opacityAnimation);
                    j++;
                }
            };
            timer.Start();
        }

        // Dessine un cercle sur la table surface correspondant à une track (avec la couleur du zouzou et le nom de la track)
        public void drawCircle(string trackId, string trackPath, string zouzouColor)
        {
            string trackName;
            string trackType;

            trackName = samplesMap.findSample(trackPath).Name;
            trackType = samplesMap.findSample(trackPath).Type;

            myScatterView.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(delegate()
                {
                    var converter = new System.Windows.Media.BrushConverter();
                    var color = (Brush)converter.ConvertFromString("#" + zouzouColor);
                    Point newPosition = new Point();

                    // Verifier si un scatterViewItem de la meme couleur a deja ete pose
                    foreach (object obj in myScatterView.Items)
                    {
                        ScatterViewItem svi = myScatterView.ItemContainerGenerator.ContainerFromItem(obj) as ScatterViewItem;
                        object objectSvi = svi.Content;
                        if (objectSvi is Canvas)
                        {
                            Canvas sviContent = objectSvi as Canvas;
                            foreach (object canvasChild in sviContent.Children)
                            {
                                if (canvasChild is Border)
                                {
                                    Border circle = canvasChild as Border;
                                    // Si oui, positionner le nouveau scatterViewItem aux alentours du svi de meme couleur
                                    if (circle.Background.ToString() == color.ToString())
                                    {
                                        newPosition = RandCenter((int)svi.ActualCenter.X, (int)svi.ActualCenter.Y);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Ellipse
                    Border border = new Border();
                    border.BorderBrush = color;
                    border.Background = color;
                    border.CornerRadius = new CornerRadius(100);
                    border.Height = 120;
                    border.Width = 120;

                    // Content : icon + track name
                    StackPanel stack = new StackPanel();
                    stack.Width = 100;
                    stack.Height = 100;
                    stack.HorizontalAlignment = HorizontalAlignment.Center;
                    stack.VerticalAlignment = VerticalAlignment.Top;

                    TextBlock content = new TextBlock();
                    content.Text = trackName;
                    content.Foreground = new SolidColorBrush(Colors.White);
                    content.FontWeight = FontWeights.Bold;
                    content.FontSize = 14;
                    content.Height = 40;
                    content.Width = 98;
                    content.HorizontalAlignment = HorizontalAlignment.Stretch;
                    content.TextAlignment = TextAlignment.Center;
                    content.TextWrapping = TextWrapping.Wrap;

                    Image icon = new Image();
                    icon.Source = new BitmapImage(new Uri(@"../../Resources/" + trackType + ".png", UriKind.Relative));
                    icon.Width = 40;
                    icon.Height = 40;
                    icon.Margin = new Thickness(0, 15, 0, 5);

                    stack.Children.Add(icon);
                    stack.Children.Add(content);
                    border.Child = stack;

                    Canvas c = new Canvas();
                    c.Height = 100;
                    c.Width = 100;
                    c.Children.Add(border);
                    c.Background = new SolidColorBrush(Colors.Transparent);

                    // Effects
                    var effects = Enum.GetValues(typeof(SoundEffect)).Cast<SoundEffect>();
                    Console.Out.WriteLine(trackId);
                    Loop l = manager.getLoop(trackId);
                    int i = 0;
                    foreach (SoundEffect effect in effects)
                    {
                        if (l == null) continue;
                        float value = l.getEffect(effect);
                        if (true || value > 0)
                        {
                            Image effectVisualizer = new Image();
                            effectVisualizer.Source = new BitmapImage(new Uri(@"../../Resources/loader/Untitled-" + (int)(10 * value) + ".png", UriKind.Relative));

                            effectVisualizer.Width = 20;
                            effectVisualizer.Height = 20;
                            myScatterView.RegisterName("fx_" + trackId + "_" + effect.ToString(), effectVisualizer);
                            double x = 50 + 70 * Math.Cos(i * Math.PI / 5);
                            double y = 50 + 70 * Math.Sin(i * Math.PI / 5);
                            effectVisualizer.SetValue(Canvas.LeftProperty, x);
                            effectVisualizer.SetValue(Canvas.TopProperty, y);

                            Image effectIcon = new Image();
                            effectIcon.Source = new BitmapImage(makeUriForEffect(effect));
                            effectIcon.Width = 16;
                            effectIcon.Height = 16;

                            x = 50 + 72 * Math.Cos(i * Math.PI / 5);
                            y = 50 + 72 * Math.Sin(i * Math.PI / 5);
                            effectIcon.SetValue(Canvas.LeftProperty, x);
                            effectIcon.SetValue(Canvas.TopProperty, y);

                            c.Children.Add(effectIcon);
                            c.Children.Add(effectVisualizer);

                            i++;
                        }
                    }

                    // Item creation
                    ScatterViewItem item = new ScatterViewItem();
                    item.Name = beginningLetter + trackId;
                    myScatterView.RegisterName(item.Name, item);
                    item.Content = c;
                    item.Background = new SolidColorBrush(Colors.Transparent);
                    item.Opacity = 0;
                    item.Height = 170;
                    item.Width = 170;
                    item.CanScale = false;
                    item.ClipToBounds = false;
                    //item.Orientation = 0;
                    if (newPosition != (new Point(0, 0)))
                    {
                        item.Center = newPosition;
                    }

                    // Events
                    item.PreviewTouchDown += new EventHandler<TouchEventArgs>(handle_TouchDown);
                    item.PreviewTouchMove += new EventHandler<TouchEventArgs>(handle_TouchMove);
                    item.PreviewTouchUp += new EventHandler<TouchEventArgs>(handle_TouchLeave);
                    TouchExtensions.AddTapGestureHandler(item, new EventHandler<TouchEventArgs>(handle_TapGesture));
                    item.PreviewMouseUp += new MouseButtonEventHandler(handle_MouseUp);

                    item.ApplyTemplate();
                    Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
                    ssc = item.Template.FindName("shadow", item) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
                    ssc.Visibility = Visibility.Hidden;

                    myScatterView.Items.Add(item);

                    // Animation : FadeIn
                    DoubleAnimation opacityAnimation = null;
                    opacityAnimation = new DoubleAnimation(0, 0.5, TimeSpan.FromSeconds(0.5), FillBehavior.Stop);
                    opacityAnimation.AccelerationRatio = 0.5;
                    opacityAnimation.DecelerationRatio = 0.5;
                    opacityAnimation.FillBehavior = FillBehavior.Stop;
                    opacityAnimation.Completed += delegate(object send, EventArgs ev)
                    {
                        item.Opacity = 0.5;
                    };
                    item.BeginAnimation(ScatterViewItem.OpacityProperty, opacityAnimation);

                })
            );
        }

        private Uri makeUriForEffect(SoundEffect effect)
        {
            string prefix = @"../../Resources/effects/";
            switch (effect)
            {
                case SoundEffect.Chorus:
                    return new Uri(prefix + "chorus.png", UriKind.Relative);
                case SoundEffect.Flanger:
                    return new Uri(prefix + "flanger.png", UriKind.Relative);
                case SoundEffect.Volume:
                    return new Uri(prefix + "volume.png", UriKind.Relative);
                case SoundEffect.WavesReverb:
                    return new Uri(prefix + "reverb.png", UriKind.Relative);
                case SoundEffect.Gargle:
                    return new Uri(prefix + "gargle.png", UriKind.Relative);
                case SoundEffect.Echo:
                    return new Uri(prefix + "echo.png", UriKind.Relative);
                default:
                    return new Uri(prefix + "volume.png", UriKind.Relative);
            }
        }

        // Supprime le cercle sur la table surface, correspondant à une track 
        public void removeCircle(string trackId, string zouzouColor)
        {
            myScatterView.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(delegate()
                {
                    object objectScatterViewItem = myScatterView.FindName(beginningLetter + trackId);
                    if (objectScatterViewItem is ScatterViewItem)
                    {
                        ScatterViewItem wantedChild = objectScatterViewItem as ScatterViewItem;

                        // Animation : FadeOut
                        DoubleAnimation opacityAnimation = null;
                        opacityAnimation = new DoubleAnimation(wantedChild.Opacity, 0, TimeSpan.FromSeconds(0.5), FillBehavior.Stop);
                        opacityAnimation.AccelerationRatio = 0.5;
                        opacityAnimation.DecelerationRatio = 0.5;
                        opacityAnimation.FillBehavior = FillBehavior.Stop;
                        opacityAnimation.Completed += delegate(object send, EventArgs ev)
                        {
                            myScatterView.Items.Remove(wantedChild);
                        };
                        wantedChild.BeginAnimation(ScatterViewItem.OpacityProperty, opacityAnimation);
                    }
                })
            );
        }

        private void handle_TouchDown(object sender, TouchEventArgs e)
        {
            ScatterViewItem item = sender as ScatterViewItem;

            // Capture to the ellipse.  
            e.TouchDevice.Capture(item);

            // Remember this contact if a contact has not been remembered already.  
            // This contact is then used to move the ellipse around.
            if (ellipseControlTouchDevice == null)
            {
                ellipseControlTouchDevice = e.TouchDevice;

                // Remember where this contact took place.  
                lastPoint = item.ActualCenter;
            }

            // Mark this event as handled.  
            e.Handled = true;
        }

        private void handle_TouchMove(object sender, TouchEventArgs e)
        {
            ScatterViewItem currentItem = sender as ScatterViewItem;

            if (e.TouchDevice == ellipseControlTouchDevice)
            {
                // Get the current position of the contact.  
                Point currentTouchPoint = currentItem.ActualCenter;

                // Get the change between the controlling contact point and
                // the changed contact point.  
                double deltaX = currentTouchPoint.X - lastPoint.X;
                double deltaY = currentTouchPoint.Y - lastPoint.Y;

                object objCurrentItem = currentItem.Content;
                string currentColor = "";
                /*
                if (objCurrentItem is Border)
                {
                    Border sviContent = new Border();
                    sviContent = objCurrentItem as Border;
                    currentColor = sviContent.Background.ToString();
                }*/

                if (objCurrentItem is Canvas)
                {
                    Canvas c = objCurrentItem as Canvas;
                    foreach (object child in c.Children)
                    {
                        if (child is Border)
                        {
                            Border b = child as Border;
                            currentColor = b.Background.ToString();
                        }
                    }
                }

                Point itemPosition = new Point();

                // Parcourir les sci de meme couleur
                foreach (object obj in myScatterView.Items)
                {
                    ScatterViewItem svi = myScatterView.ItemContainerGenerator.ContainerFromItem(obj) as ScatterViewItem;
                    object objectSvi = svi.Content;
                    if (objectSvi is Canvas)
                    {
                        Canvas sviContent = objectSvi as Canvas;
                        foreach (object canvasChild in sviContent.Children)
                        {
                            if (canvasChild is Border)
                            {
                                Border circle = canvasChild as Border;
                                // Si oui, positionner le nouveau scatterViewItem aux alentours du svi de meme couleur
                                if ((circle.Background.ToString() == currentColor) && (svi != currentItem))
                                {
                                    itemPosition = svi.ActualCenter;
                                    svi.Center = new Point(itemPosition.X + deltaX, itemPosition.Y + deltaY);
                                }
                            }
                        }
                    }
                }

                // Forget the old contact point, and remember the new contact point.  
                lastPoint = currentTouchPoint;

                // Mark this event as handled.  
                e.Handled = true;
            }
        }

        private void handle_TouchLeave(object sender, TouchEventArgs e)
        {
            // If this contact is the one that was remembered  
            if (e.TouchDevice == ellipseControlTouchDevice)
            {
                // Forget about this contact.
                ellipseControlTouchDevice = null;
            }

            // Mark this event as handled.  
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

        private void trackGroupsUpdate(TrackGroups g)
        {
            subscriber.Client.Update("/track-groups/update", "{\"_id\":\"" + g.Id + "\"}", "{\"$set\":" + g.serializeEffectsToJSon() + "}", "{}");
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
                manager.addLoop(jamTracks.Id, "../.." + jamTracks.Path, false);
                drawCircle(jamTracks.Id, jamTracks.Path, jamTracks.ZouzouColor);
            }
            else if (e.PropertyName == "Removed")
            {
                removeCircle(jamTracks.Id, jamTracks.ZouzouColor);
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

        protected void trackGroupsChangedHandler(Object sender, PropertyChangedEventArgs e)
        {
            TrackGroups trackGroups = sender as TrackGroups;
            if (e.PropertyName == "Added")
            {
                info.log("hey name" + trackGroups.Name);
            }
            else if (e.PropertyName == "EffectUpdated")
            {
                foreach(string loopId in trackGroups.TracksId){
                    foreach (Effect effect in trackGroups.Effects)
                    {
                        SoundEffect soundEffect = effectMap[effect.Name];
                        manager.setEffectOnLoop(loopId, soundEffect, effectTresholdValue(effect.Value));
                    }
                }
                //subscriber.Client.Update("/track-groups/update", "{\"_id\":\""+trackGroups.Id+"\"}","{\"$set\":{\"name\":\""+trackGroups.Name+"\"}}","{}");
            }
            else if (e.PropertyName == "TracksUpdated")
            {

            }
            else if(e.PropertyName == "LeapUpdated"){

            }
            else if (e.PropertyName == "Removed")
            {
                foreach (string loopId in trackGroups.TracksId)
                {
                    foreach (Effect effect in trackGroups.Effects)
                    {
                        if(effect.Name == "volume")
                            manager.setEffectOnLoop(loopId, effectMap[effect.Name], 1);
                        else
                            manager.setEffectOnLoop(loopId, effectMap[effect.Name], 0);
                    }
                }
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
            leapController.RemoveListener(leapListener);
            leapController.Dispose();

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

        private float effectTresholdValue(float value)
        {
            if (value < 0.099)
            {
                if (trackGroupsList.beingModified)
                {
                    return 0.099f;
                }
                else
                {
                    return 0;
                }
            }
            return value;
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
                    TouchExtensions.AddTapGestureHandler(item, new EventHandler<TouchEventArgs>(handle_JamTapGesture));
                    item.PreviewMouseUp += new MouseButtonEventHandler(handle_JamMouseUp);
                    item.Content = border;
                    item.Background = new SolidColorBrush(Colors.Transparent);
                    item.Opacity = 0.85;
                    item.MinHeight = 200;
                    item.MinWidth = 200;

                    item.ApplyTemplate();
                    Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
                    ssc = item.Template.FindName("shadow", item) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
                    ssc.Visibility = Visibility.Hidden;

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
            subscriber.Bind(_messages, "track-groups", "track-groups", jamId);

            // Draw timer in the center of myScatterView
            drawTimer();
        }

        private void handle_JamMouseUp(object sender, MouseButtonEventArgs e)
        {
            ScatterViewItem item = sender as ScatterViewItem;
            string jamId = item.Name.Substring(1, item.Name.Length - 1);
            myScatterView.Items.Clear();
            subscriber.Bind(_messages, "jam", "jam", jamId);
            subscriber.Bind(_messages, "jam-tracks", "jam-tracks", jamId);
            subscriber.Bind(_messages, "track-groups", "track-groups", jamId);  

            // Draw timer in the center of myScatterView
            drawTimer();
        }

        //TAG

        private void InitializeDefinitions()
        {
            byte[] tags = { 0x6A, 0x7A, 0x8A, 0x9A, 0xAA, 0xAB, 0x6B, 0x7B, 0x8B, 0x9B, 0xBB, 0xBA };
            for (int k = 1; k <= 12; k++)
            {
                TagVisualizationDefinition tagDef = new TagVisualizationDefinition();
                // The tag value that this definition will respond to.
                tagDef.Value = tags[k-1];
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
            filter.Opacity = 0;
            ScatterViewItem item = OnItem(filter.Center);
            if (item != null)
            {
                JamTracks jamTracks = jamTracksList.findById(item.Name.Substring(1, item.Name.Length - 1));
                filter.associatedJamTracks = jamTracks;
            }
            long tagValue = filter.VisualizedTag.Value;
            if (tagValue == 0x6A || tagValue == 0x6B)
            {
                filter.FilterType.Content = "Volume";
                filter.myEllipse.Fill = SurfaceColors.Accent1Brush;
                filter.Effect = SoundEffect.Volume;
            }
            else if (tagValue == 0x7A || tagValue == 0x7B)
            {
                filter.FilterType.Content = "Gargle";
                filter.myEllipse.Fill = SurfaceColors.BulletBrush;
                filter.Effect = SoundEffect.Gargle;
            }
            else if (tagValue == 0x8A || tagValue == 0x8B)
            {
                filter.FilterType.Content = "Echo";
                filter.myEllipse.Fill = SurfaceColors.Accent3Brush;
                filter.Effect = SoundEffect.Echo;
            }
            else if (tagValue == 0x9A || tagValue == 0x9B)
            {
                filter.FilterType.Content = "Waves Reverb";
                filter.myEllipse.Fill = SurfaceColors.BulletDisabledBrush;
                filter.Effect = SoundEffect.WavesReverb;
            }
            else if (tagValue == 0xAA || tagValue == 0xBB)
            {
                filter.FilterType.Content = "Flanger";
                filter.myEllipse.Fill = SurfaceColors.Accent4Brush;
                filter.Effect = SoundEffect.Flanger;
            }
            else if (tagValue == 0xAB || tagValue == 0xBA)
            {
                filter.FilterType.Content = "Chorus";
                filter.myEllipse.Fill = SurfaceColors.Accent2Brush;
                filter.Effect = SoundEffect.Chorus;
            }
            else
            {
                filter.FilterType.Content = "Volume";
                filter.myEllipse.Fill = SurfaceColors.Accent1Brush;
                filter.Effect = SoundEffect.Volume;
            }            
        }

        private void MyTagVisualizer_VisualizationMoved(object sender, TagVisualizerEventArgs e)
        {
            TagVisualization1 filter = (TagVisualization1)e.TagVisualization;
            double val = 0;
            
            if (filter.associatedJamTracks != null)
            {
                /*
                // Quand on tourne le tag à droite
                if (filter.Valeur - filter.Orientation < 0)
                {                    
                    if (filter.Orientation >= filter.OriginalOrientation)
                    {
                        val = (float)Math.Abs((filter.Orientation - filter.OriginalOrientation));                        
                    }
                    else // on depasse 360 et on repasse à 0
                    {
                        val = (float)Math.Abs(360 - (filter.OriginalOrientation - filter.Orientation));
                    }
                }
                // Quand on tourne le tag à gauche
                else
                {
                    if (filter.Orientation <= filter.OriginalOrientation)
                    {
                        val = (float) (filter.Orientation - filter.OriginalOrientation);                        
                    }
                    else // on passe en dessous de 0 et on repasse à 360
                    {
                        val = (float) (filter.Orientation - 360 - filter.OriginalOrientation);
                    }
                }*/
                val = filter.Orientation - filter.Valeur;
                if (val > 300)
                {
                    val -= 360;
                }
                else if (val < -300)
                {
                    val += 360;
                }

                var total = manager.applyDeltaToEffectOnLoop(filter.associatedJamTracks.Id, filter.Effect, (float) val / 360);
                //filter.GeneralEffectValue = filter.associatedJamTracks.Effect1;
                filter.Valeur = filter.Orientation;
            }
            else
            {
                filter.Opacity = 1;
            }
        }

        // LEAP MOTION
        void handleHandVariation(float dPitch, float dX, float dY)
        {
            lock (thisLock)
            {
                dPitch = 2 * dPitch / (float)Math.PI;
                dX = dX / 200;
                dY = dY / 200;

                foreach (TrackGroups g in trackGroupsList)
                {
                    foreach (string trackId in g.TracksId)
                    {
                        foreach (string fx in g.LeapGesturesMapping.X)
                        {
                            float v = manager.applyDeltaToEffectOnLoop(trackId, effectMap[fx], dX);
                            g.Effects[effectMapper(fx)].Value = v;
                        }
                        foreach (string fx in g.LeapGesturesMapping.Y)
                        {
                            float v = manager.applyDeltaToEffectOnLoop(trackId, effectMap[fx], dY);
                            g.Effects[effectMapper(fx)].Value = v;
                        }
                        foreach (string fx in g.LeapGesturesMapping.Pitch)
                        {
                            float v = manager.applyDeltaToEffectOnLoop(trackId, effectMap[fx], dPitch);
                            g.Effects[effectMapper(fx)].Value = v;
                        }
                    }
                    g.Modified = true;
                }
            }
        }

        private int effectMapper(string fx)
        {
            switch (fx)
            {
                case "volume":
                    return 0;
                case "chorus":
                    return 5;
                case "echo":
                    return 2;
                case "flanger":
                    return 4;
                case "gargle":
                    return 1;
                case "reverb":
                    return 3;
                default:
                    return 0;
            }
        }
    }
}
