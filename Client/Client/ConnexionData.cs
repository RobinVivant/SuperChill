using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MySurfaceApplication
{

        public partial class ConnexionData : Dictionary<string, List<Sample> >, INotifyCollectionChanged, INotifyPropertyChanged
        {
            public ConnexionData() : base() { }
            public ConnexionData(int capacity) : base(capacity) { }
            public ConnexionData(IEqualityComparer<string> comparer) : base(comparer) { }
            public ConnexionData(IDictionary<string, List<Sample> > dictionary) : base(dictionary) { }
            public ConnexionData(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer) { }
            public ConnexionData(IDictionary<string, List<Sample> > dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer) { }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public event PropertyChangedEventHandler PropertyChanged;

            public new List<Sample> this[string key]
            {
                get
                {
                    return base[key];
                }
                set
                {
                    List<Sample> oldValue;
                    bool exist = base.TryGetValue(key, out oldValue);
                    var oldItem = new KeyValuePair<string, List<Sample> >(key, oldValue);
                    base[key] = value;
                    var newItem = new KeyValuePair<string, List<Sample> >(key, value);
                    if (exist)
                    {
                        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, base.Keys.ToList().IndexOf(key)));
                    }
                    else
                    {
                        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, base.Keys.ToList().IndexOf(key)));
                        this.OnPropertyChanged(new PropertyChangedEventArgs("Created"));
                    }
                }
            }

            public new void Add(string key, List<Sample> value)
            {
                if (!base.ContainsKey(key))
                {
                    var item = new KeyValuePair<string, List<Sample>>(key, value);
                    base.Add(key, value);
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, base.Keys.ToList().IndexOf(key)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Added"));
                }
            }

            public new bool Remove(string key)
            {
                List<Sample> value;
                if (base.TryGetValue(key, out value))
                {
                    var item = new KeyValuePair<string, List<Sample>>(key, base[key]);
                    bool result = base.Remove(key);
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, base.Keys.ToList().IndexOf(key)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Removed"));
                    return result;
                }
                return false;
            }

            public KeyValuePair<string, Sample> findSample(string path)
            {
                foreach (var keyValue in this)
                {
                    foreach (var sample in keyValue.Key)
                    {
                        Console.WriteLine(sample+"  "+path);
                        /*if (sample.Path == path)
                        {
                            Console.WriteLine("ooooooooooooo");
                            return new KeyValuePair<string, Sample>(keyValue.Key, sample);
                        }*/
                    }
                }
                Console.WriteLine("nnnnnnnnnnnnnnnnnnnnn");
                return new KeyValuePair<string, Sample>("null", null);
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
