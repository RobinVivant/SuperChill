using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySurfaceApplication
{
    public partial class ZouzouData : List<Zouzou>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public ZouzouData() : base() { }
        public ZouzouData(int capacity) : base(capacity) { }
        public ZouzouData(IList<Zouzou> dictionary) : base(dictionary) { }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public new void Add(Zouzou Zouzou)
        {
            if (!base.Contains(Zouzou))
            {
                base.Add(Zouzou);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Zouzou, base.IndexOf(Zouzou)));
                this.OnPropertyChanged(Zouzou,new PropertyChangedEventArgs("Added"));
            }
        }

        private void OnPropertyChanged(Zouzou Zouzou, PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(Zouzou, e);
            }
        }

        public new bool Remove(string id)
        {
            Zouzou zouzou = this.findById(id);
            if (base.Contains(zouzou))
            {
                bool result = base.Remove(zouzou);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, zouzou, base.IndexOf(zouzou)));
                this.OnPropertyChanged(new PropertyChangedEventArgs("Removed"));
                return result;
            }
            return false;
        }

        public Zouzou findById(string id)
        {
            foreach (var zouzou in this)
            {
                if (zouzou.Id == id)
                {
                    return zouzou;
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
