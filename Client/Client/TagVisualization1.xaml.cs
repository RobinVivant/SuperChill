using System;
using System.Collections.Generic;
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

namespace MySurfaceApplication
{
    /// <summary>
    /// Interaction logic for TagVisualization.xaml
    /// </summary>
    public partial class TagVisualization1 : TagVisualization
    {
        private double originalOrientation;
        private double valeur;
        private double generalEffectValue;
        private JamTracks jamT;
        private SoundEffect effect;

        public TagVisualization1()
        {
            InitializeComponent();
        }

        public double GeneralEffectValue
        {
            get
            {
                return generalEffectValue;
            }

            set
            {
                generalEffectValue = value;
            }
        }

        public double OriginalOrientation
        {
            get
            {
                return originalOrientation;
            }

            set
            {
                originalOrientation = value;
            }
        }

        public double Valeur
        {
            get
            {
                return valeur;
            }

            set
            {
                valeur = value;
            }
        }

        public SoundEffect Effect
        {
            get
            {
                return effect;
            }
            set
            {
                effect = value;
            }
        }

        private void TagVisualization_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO: customize TagVisualization1's UI based on this.VisualizedTag here
            //UserNotifications.RequestNotification("Tag", "Tag reconnu", TimeSpan.FromSeconds(2));
        }

        public JamTracks associatedJamTracks
        {
            get
            {
                return jamT;
            }
            set
            {
                jamT = value;
            }
        }
    }
}
