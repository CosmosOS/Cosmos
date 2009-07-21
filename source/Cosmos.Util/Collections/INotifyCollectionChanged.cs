using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDAm.Framework
{
    public interface INotifyCollectionChanged<T>
    {
        event NotifyCollectionChangedEventHandler<T> CollectionChanged;

    }


    public delegate void NotifyCollectionChangedEventHandler<T>(
    object sender,
    NotifyCollectionChangedEventArgs<T> e
);

    public class NotifyCollectionChangedEventArgs<T> : EventArgs
    {
        //start with something simple
        T objectChanged;
        NotifyCollectionChangedAction action;

        public NotifyCollectionChangedEventArgs(T change, NotifyCollectionChangedAction action)
        {
            this.objectChanged = change;
            this.action = action;
        }
        
        public NotifyCollectionChangedAction Action
        {
            get { return action; }
            set { action = value; }
        }
        
        public T ObjectChanged
        {
            get { return objectChanged; }

        }
    }

    public enum NotifyCollectionChangedAction
    {

        Add,//One or more items were added to the collection.
        Remove, //One or more items were removed from the collection.
        Replace,//One or more items were replaced in the collection.
        Move,//One or more items were moved within the collection.
        Reset
    }

}
