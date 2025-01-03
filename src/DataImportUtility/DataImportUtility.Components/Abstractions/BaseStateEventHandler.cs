using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataImportUtility.Components.Abstractions;

/// <summary>
/// The base class for state event handlers.
/// </summary>
public abstract class BaseStateEventHandler : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>
    /// Event raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Event raised when a property value is changing.
    /// </summary>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Event raised when the state changes.
    /// </summary>
    public event Func<Task>? OnNotifyStateChanged;

    /// <summary>
    /// Event raised when a property on the state class has changed.
    /// </summary>
    public event Func<string, Task>? OnStatePropertyChanged;

    /// <summary>
    /// Notifies that the state has changed.
    /// </summary>
    public virtual void NotifyStateChanged()
    {
        OnNotifyStateChanged?.Invoke();
    }
    /// <summary>
    /// Notifies that the state has changed.
    /// </summary>
    public virtual Task NotifyStateChangedAsync() => Task.Run(NotifyStateChanged);

    /// <summary>
    /// Notifies that a property has changed.
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed.</param>
    public virtual void NotifyStatePropertyChanged(string propertyName)
    {
        OnStatePropertyChanged?.Invoke(propertyName);
    }
    /// <summary>
    /// Notifies that a property has changed.
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed.</param>
    public virtual Task NotifyStatePropertyChangedAsync(string propertyName) => Task.Run(() => NotifyStatePropertyChanged(propertyName));

    /// <summary>
    /// Notifies that a property has changed.
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed.</param>
    public virtual void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        OnStatePropertyChanged?.Invoke(propertyName);
        OnNotifyStateChanged?.Invoke();
    }
    /// <summary>
    /// Notifies that a property has changed.
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed.</param>
    public virtual Task NotifyPropertyChangedAsync(string propertyName) => Task.Run(() => NotifyPropertyChanged(propertyName));

    /// <summary>
    /// Notifies that a property is changing.
    /// </summary>
    /// <param name="propertyName">The name of the property that is changing.</param>
    public virtual void NotifyPropertyChanging(string propertyName)
    {
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }
    /// <summary>
    /// Notifies that a property is changing.
    /// </summary>
    /// <param name="propertyName">The name of the property that is changing.</param>
    public virtual Task NotifyPropertyChangingAsync(string propertyName) => Task.Run(() => NotifyPropertyChanging(propertyName));

    /// <summary>
    /// Sets the property value and notifies that the property has changed, raising the PropertyChanging 
    /// event and the PropertyChanged event.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The property's backing field that will be updated if the value is different.</param>
    /// <param name="value">
    /// The new value for the property. If the value is different than the current value, the property will 
    /// be updated.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property that is changing. This value is optional and can be provided automatically 
    /// when calling the method.
    /// </param>
    public virtual void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(storage, value))
        {
            return;
        }

        NotifyPropertyChanging(propertyName);
        storage = value;
        NotifyPropertyChanged(propertyName);
    }
}
