using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MySurfaceApplication
{

        public partial class ConnexionData : Dictionary<string, Sample >, INotifyCollectionChanged, INotifyPropertyChanged
        {
            public ConnexionData() : base() { }
            public ConnexionData(int capacity) : base(capacity) { }
            public ConnexionData(IEqualityComparer<string> comparer) : base(comparer) { }
            public ConnexionData(IDictionary<string, Sample > dictionary) : base(dictionary) { }
            public ConnexionData(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer) { }
            public ConnexionData(IDictionary<string, Sample > dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer) { }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public event PropertyChangedEventHandler PropertyChanged;

            public new void Add(string key, Sample value)
            {
                if (!base.ContainsKey(key))
                {
                    var item = new KeyValuePair<string, Sample>(key, value);
                    base.Add(key, value);
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, base.Keys.ToList().IndexOf(key)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Added"));
                }
            }

            public new bool Remove(string key)
            {
                Sample value;
                if (base.TryGetValue(key, out value))
                {
                    var item = new KeyValuePair<string, Sample>(key, base[key]);
                    bool result = base.Remove(key);
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, base.Keys.ToList().IndexOf(key)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Removed"));
                    return result;
                }
                return false;
            }

            public Sample findSample(string path)
            {
                Sample sample;
                base.TryGetValue(path, out sample);
                return sample;
            }

            public new int count()
            {
                return this.Count();
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
