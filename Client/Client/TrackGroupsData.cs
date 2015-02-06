using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MySurfaceApplication
{

    public partial class TrackGroupsData : List<TrackGroups>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public TrackGroupsData() : base() { }
        public TrackGroupsData(int capacity) : base(capacity) { }
        public TrackGroupsData(IList<TrackGroups> dictionary) : base(dictionary) { }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public new void Add(TrackGroups trackGroups)
        {
            if (!base.Contains(trackGroups))
            {
                base.Add(trackGroups);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, trackGroups, base.IndexOf(trackGroups)));
                this.OnPropertyChanged(trackGroups, new PropertyChangedEventArgs("Added"));
            }
        }

        public new bool Remove(string id)
        {
            TrackGroups trackGroups = this.findById(id);
            if (trackGroups != null || base.Contains(trackGroups))
            {
                bool result = base.Remove(trackGroups);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, trackGroups, base.IndexOf(trackGroups)));
                this.OnPropertyChanged(trackGroups, new PropertyChangedEventArgs("Removed"));
                return result;
            }
            return false;
        }

        public new void EffectUpdate(ref TrackGroups newTrackGroups)
        {
            TrackGroups trackGroups = this.findById(newTrackGroups.Id);
            if (trackGroups != null || base.Contains(trackGroups))
            {
                base.Remove(trackGroups);
                base.Add(newTrackGroups);
                this.OnPropertyChanged(newTrackGroups, new PropertyChangedEventArgs("EffectUpdated"));
            }
        }

        public new void LeapUpdate(ref TrackGroups newTrackGroups)
        {
            TrackGroups trackGroups = this.findById(newTrackGroups.Id);
            if (trackGroups != null || base.Contains(trackGroups))
            {
                base.Remove(trackGroups);
                base.Add(newTrackGroups);
                this.OnPropertyChanged(newTrackGroups, new PropertyChangedEventArgs("LeapUpdated"));
            }
        }

        public new void TracksUpdate(ref TrackGroups newTrackGroups)
        {
            TrackGroups trackGroups = this.findById(newTrackGroups.Id);
            if (trackGroups != null || base.Contains(trackGroups))
            {
                base.Remove(trackGroups);
                base.Add(newTrackGroups);                
                this.OnPropertyChanged(newTrackGroups, new PropertyChangedEventArgs("TracksUpdated"));
            }
        }

        public new void Update(TrackGroups newTrackGroups)
        {
            TrackGroups trackGroups = this.findById(newTrackGroups.Id);
            if (trackGroups != null || base.Contains(trackGroups))
            {
                //base.Remove(trackGroups);
                //base.Add(newTrackGroups);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, newTrackGroups, base.IndexOf(newTrackGroups)));
                this.OnPropertyChanged(newTrackGroups, new PropertyChangedEventArgs("Updated"));
            }
        }

        public TrackGroups findById(string id)
        {
            foreach(var trackGroups in this){
                if (trackGroups.Id == id)
                {
                    return trackGroups;
                }
            }
            return null;
        }

        public new void Clear()
        {
            base.Clear();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Cleared"));
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, e);
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, e);
            }
        }

        protected virtual void OnPropertyChanged(TrackGroups trackGroups,PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(trackGroups, e);
            }
        }
    }
}
