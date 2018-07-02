using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reactive;
using System.Reactive.Linq;


namespace Homie.Models
{
    public class PropertyChangedEventArgs
    {
        public string PropertyName { get; }

        public PropertyChangedEventArgs(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    public class ObservableObject
    {
        public event EventHandler<PropertyChangedEventArgs> PropertyChanged;
        public IObservable<PropertyChangedEventArgs> ObservablePropertyChanged { get; }

        private Dictionary<string, object> PropertyValues { get; }


        public ObservableObject()
        {
            PropertyValues = new Dictionary<string, object>();

            ObservablePropertyChanged = Observable.FromEventPattern<PropertyChangedEventArgs>(
                h => PropertyChanged += h,
                h => PropertyChanged -= h
            )
            .Select(propertyEvent => propertyEvent.EventArgs);
        }

        protected T Get<T>([CallerMemberName] string propertyName = null)
        {
            if (PropertyValues.ContainsKey(propertyName))
                return (T)PropertyValues[propertyName];
            return default(T);
        }

        protected void Set<T>(T Value, [CallerMemberName] string propertyName = null)
        {
            if (!(Value != null && Value.Equals(Get<T>(propertyName))))
            {
                PropertyValues[propertyName] = Value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
