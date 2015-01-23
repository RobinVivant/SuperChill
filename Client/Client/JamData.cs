using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySurfaceApplication
{
    public partial class JamData : List<Jam>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public JamData() : base() { }
        public JamData(int capacity) : base(capacity) { }
        public JamData(IList<Jam> dictionary) : base(dictionary) { }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public new void Add(Jam Jam)
        {
            if (!base.Contains(Jam))
            {
                base.Add(Jam);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Jam, base.IndexOf(Jam)));
                this.OnPropertyChanged(Jam,new PropertyChangedEventArgs("Added"));
            }
        }

        private void OnPropertyChanged(Jam Jam, PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(Jam, e);
            }
        }

        public new bool Remove(string id)
        {
            Jam jam = this.findById(id);
            if (base.Contains(jam))
            {
                bool result = base.Remove(jam);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, jam, base.IndexOf(jam)));
                this.OnPropertyChanged(new PropertyChangedEventArgs("Removed"));
                return result;
            }
            return false;
        }

        public Jam findById(string id)
        {
            foreach (var jam in this)
            {
                if (jam.Id == id)
                {
                    return jam;
                }
            }
            return null;
        }

        public Jam findByName(string name)
        {
            foreach (var jam in this)
            {
                if (jam.Name == name)
                {
                    return jam;
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

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, e);
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, e);
            }
        }

    }
}
