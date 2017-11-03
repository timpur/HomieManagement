using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HomieManagement.Model
{

  public delegate void ChangedParentEventHandler<P, T>(P parent, T diff);
  public delegate void ChangedEventHandler<T>(T diff);

  public class ChangeBase<C> where C : new()
  {
    public event ChangedEventHandler<C> ChangeEvent;
    protected Dictionary<string, object> PropertyValues { get; }
    protected bool TrigerEvents { get; private set; }

    public ChangeBase(bool trigerEvents)
    {
      PropertyValues = new Dictionary<string, object>();
      TrigerEvents = trigerEvents;
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
        if (TrigerEvents) Changed(propertyName);
      }
    }
    protected void Changed(string propertyName)
    {
      var diff = new C();
      var val = Get<dynamic>(propertyName);
      typeof(C).GetProperty(propertyName).SetValue(diff, val);
      ChangeEvent?.Invoke(diff);
    }
  }
}
