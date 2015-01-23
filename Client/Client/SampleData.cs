using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySurfaceApplication
{
    public partial class SampleData : List<Sample>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public SampleData() : base() { }
        public SampleData(int capacity) : base(capacity) { }
        public SampleData(IList<Sample> dictionary) : base(dictionary) { }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public new void Add(Sample sample)
        {
            if (!base.Contains(sample))
            {
                base.Add(sample);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, sample, base.IndexOf(sample)));
                this.OnPropertyChanged(sample,new PropertyChangedEventArgs("Added"));
            }
        }

        private void OnPropertyChanged(Sample sample, PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(sample, e);
            }
        }

        public new bool Remove(string name)
        {
            Sample sample = this.findByName(name);
            if (base.Contains(sample))
            {
                bool result = base.Remove(sample);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, sample, base.IndexOf(sample)));
                this.OnPropertyChanged(new PropertyChangedEventArgs("Removed"));
                return result;
            }
            return false;
        }

        public Sample findByName(string name)
        {
            foreach (var sample in this)
            {
                if (sample.Name == name)
                {
                    return sample;
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
