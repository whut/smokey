## System.IDisposable (sealed class) ##

```
internal sealed class SealedNoFinalizer : IDisposable
{ 
    public SealedNoFinalizer()
    {
        // allocate resources
    }
    
    public void Work()
    {
        if (m_disposed)        
            throw new ObjectDisposedException(GetType().Name);
    }

    public void Dispose()
    {
        if (!m_disposed)
        {
            // We have no finalizer so we can safely call methods
            // on any of our fields when cleaning up.
            m_disposed = true;
        }
    }
    
    // resource fields go here
    private bool m_disposed; 
}

internal sealed class SealedWithFinalizer : IDisposable
{ 
    ~SealedWithFinalizer()
    {
        // Note that this is called even if the ctor throws. Also
        // note that this will execute within a worker thread.
        Dispose(false);
    }
    
    public SealedWithFinalizer()
    {
        // allocate resources
    }
    
    public void Work()
    {
        if (m_disposed)        
            throw new ObjectDisposedException(GetType().Name);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    private void Dispose(bool disposing)
    {
        if (!m_disposed)
        {
            if (disposing)
            {
                // This code will execute if we were called from Dispose()
                // so we can call methods on our managed fields.
            }
                            
            // There are only very weak guarantees on the order in which
            // finalizers are called so we should not use any managed
            // objects here unless we are sure that they (and all objects 
            // they may use) are not finalizeable. Note that System.Console
            // may be used.
            m_disposed = true;
        }
    }
    
    // resource fields go here
    private bool m_disposed; 
}      
```


## System.IDisposable (base/derived class) ##

```
internal class BaseClass : IDisposable
{ 
    ~BaseClass()        
    {
        // Note that this is called even if the ctor throws. Also
        // note that this will execute within a worker thread. If your
        // base class does't have any unmanaged resources you can omit
        // the finalizer.
        Dispose(false);
    }
    
    public BaseClass()
    {
        // allocate resources
    }
    
    public void Work()
    {
        // All public methods (except Dispose) should throw 
        // ObjectDisposedException if Dispose has been called.
        if (m_disposed)        
            throw new ObjectDisposedException(GetType().Name);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected bool Disposed
    {
        get {return m_disposed;}
    }
            
    protected virtual void Dispose(bool disposing)
    {
        if (!m_disposed)
        {
            if (disposing)
            {
                // This code will execute if we were called from Dispose()
                // so we can call methods on our managed fields.
            }
    
            // There are only very weak guarantees on the order in which
            // finalizers are called so we should not use any managed
            // objects here unless we are sure that they (and all objects 
            // they may use) are not finalizeable. Note that System.Console
            // may be used.
            m_disposed = true;
        }
    }
    
    // resource fields go here
    private bool m_disposed; 
}

internal class DerivedClass : BaseClass
{ 
    public DerivedClass()
    {
        // allocate more resources (in addition to base's)
    }
    
    public void Work()
    {
        // All public methods (except Dispose) should throw 
        // ObjectDisposedException if Dispose has been called.
        if (m_disposed)        
            throw new ObjectDisposedException(GetType().Name);
    }

    protected override void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                // This code will execute if we were called from Dispose()
                // so we can call methods on our managed fields.
            }
    
            // There are only very weak guarantees on the order in which
            // finalizers are called so we should not use any managed
            // objects here unless we are sure that they (and all objects 
            // they may use) are not finalizeable. Note that System.Console
            // may be used.
            base.Dispose(disposing);
        }
    }
    
    // some new fields that require cleanup
}
       
```


## System.IEquatable< T > (class version) ##

```
internal class Customer : IEquatable<Customer>
{        
    public override bool Equals(object rhsObj)
    {
        if (rhsObj == null)            // as is a little expensive, so skip it if we can
            return false;
        
        Customer rhs = rhsObj as Customer;
        return this == rhs;
    }
        
    public bool Equals(Customer rhs)    // provide a typed overload for efficiency
    {
        return this == rhs;
    }

    public static bool operator==(Customer lhs, Customer rhs)
    {
        // If both are null, or both are the same instance, return true.
        if (object.ReferenceEquals(lhs, rhs))
            return true;
        
        // If one is null, but not both, return false.
        if ((object) lhs == null || (object) rhs == null)
            return false;
        
        // Return true if the fields match:
        return lhs.name == rhs.name && lhs.address == rhs.address;
    }
    
    public static bool operator!=(Customer lhs, Customer rhs)
    {
        return !(lhs == rhs);
    }
    
    public override int GetHashCode()
    {
        int hash;
        
        unchecked
        {
            hash = name.GetHashCode() + address.GetHashCode();
        }
        
        return hash;
    }
    
    private string name = "ted";
    private string address = "main street";
}
```


## System.IEquatable< T > (struct version) ##

```
internal struct TwoDPoint : IEquatable<TwoDPoint>
{        
    public override bool Equals(object rhsObj)
    {
        if (rhsObj == null)                        // objects may be null
            return false;
        
        if (GetType() != rhsObj.GetType()) 
            return false;
    
        TwoDPoint rhs = (TwoDPoint) rhsObj;                    
        return this == rhs;
    }
        
    public bool Equals(TwoDPoint rhs)    // provide a typed overload for efficiency
    {                    
        return this == rhs;
    }

    public static bool operator==(TwoDPoint lhs, TwoDPoint rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y;
    }
    
    public static bool operator!=(TwoDPoint lhs, TwoDPoint rhs)
    {
        return !(lhs == rhs);
    }
    
    public override int GetHashCode()
    {
        int hash;
        
        unchecked
        {
            hash = x.GetHashCode() + y.GetHashCode();
        }
        
        return hash;
    }
    
    private int x, y;
}
```


## System.IFormattable ##

```
using System.Text;

internal class Customer : IFormattable
{
    public override string ToString()
    {
        return ToString("G", null);
    }
    
    // Support custom formatting via "{0:L}" String.Format syntax.
    public string ToString(string format, IFormatProvider provider)
    {
        if (provider != null)
        {
            ICustomFormatter formatter = provider.GetFormat(GetType()) as ICustomFormatter;
            if (formatter != null)
                return formatter.Format(format, this, provider);
        }
        
        StringBuilder builder = new StringBuilder(m_name.Length + 10 * m_orders.Length);
        switch (format)
        {    
            case "":            
            case null:
            case "G":
                builder.Append("name = ");
                builder.Append(m_name);
                break;

            case "L":
                builder.Append("name = ");
                builder.Append(m_name);

                builder.Append(", orders = ");
                foreach (string order in m_orders)
                {
                    builder.Append(order);
                    builder.Append(' ');
                }
                break;
                                            
            default:
                throw new ArgumentException(format + " isn't a valid Customer format string");
        }
        
        return builder.ToString();
    }
    
    private string m_name = "bob";
    private string[] m_orders = new string[]{"order1"};
} 
```


## System.Threading.Monitor ##

```
internal class ProducerConsumer
{
    public void Produce(object o)
    {
        lock (m_mutex)
        {
            m_queue.Enqueue(o);
            
            // Pulse when a change is made to our wait predicate.
            // If no thread is waiting then this will be a no-op. By
            // using PulseAll instead of Pulse we can ensure that as
            // many threads as possible wake up to handle the objects
            // we've added (and if only one thread is running Consume
            // PulseAll is no less efficient than Pulse).
            //
            // Similarly we need to pulse even if the queue was non-empty
            // because Consume may have missed the pulse when we first
            // added an item.
            //
            // Note that, unlike pthreads, the lock must be held when
            // Pulse or PulseAll is called.
            Monitor.PulseAll(m_mutex);
        }
    }
    
    public object Consume()
    {
        object result = null;
        
        lock (m_mutex)
        {
            // Block until we're happy with our wait predicate. If multiple 
            // threads are running Consume then we must use a loop to ensure 
            // another Consume thread hasn't stolen the new object. If Consume 
            // is only run by one thread then we could technically use an if, 
            // but it's better style and less fragile to use a loop anyway.
            while (m_queue.Count == 0)
            {
                // The wait call will release our mutex and wait for a Pulse.
                // When pulsed the mutex will be reacquired and wait will return
                // true (always true because we don't use a timeout).
                Ignore.Value = Monitor.Wait(m_mutex);
            }
            
            result = m_queue.Dequeue();
        }
        
        return result;
    }
    
    private readonly object m_mutex = new object();
    private Queue m_queue = new Queue();
}
```

CheatSheets