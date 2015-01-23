using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MySurfaceApplication
{

    public partial class jamTracksData : List<JamTracks>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public jamTracksData() : base() { }
        public jamTracksData(int capacity) : base(capacity) { }
        public jamTracksData(IList<JamTracks> dictionary) : base(dictionary) { }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public new void Add(JamTracks jamTracks)
        {
            if (!base.Contains(jamTracks))
            {
                base.Add(jamTracks);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, jamTracks, base.IndexOf(jamTracks)));
                this.OnPropertyChanged(jamTracks, new PropertyChangedEventArgs("Added"));
            }
        }

        public new bool Remove(string id)
        {
            JamTracks jamTracks = this.findById(id);
            if (jamTracks != null || base.Contains(jamTracks))
            {
                bool result = base.Remove(jamTracks);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, jamTracks, base.IndexOf(jamTracks)));
                this.OnPropertyChanged(jamTracks, new PropertyChangedEventArgs("Removed"));
                return result;
            }
            return false;
        }

        public JamTracks findById(string id)
        {
            foreach(var jamTrack in this){
                if (jamTrack.Id == id)
                {
                    return jamTrack;
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

        protected virtual void OnPropertyChanged(JamTracks jamTracks,PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(jamTracks, e);
            }
        }
    }
}
