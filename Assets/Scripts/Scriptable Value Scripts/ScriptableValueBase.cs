using System.Collections.Generic;
using UnityEngine;

public delegate void ObservableValueChangedDelegate<TValue>(IObservableValue<TValue> observable);

/// <summary>
/// A value that will notify its change
/// </summary>
/// <typeparam name="TValue"></typeparam>
public interface IObservableValue<TValue>
{
    /// <summary>
    /// Called when <see cref="Value"/> changes
    /// </summary>
    event ObservableValueChangedDelegate<TValue> ValueChanged;

    TValue Value { get; set; }
}

public class ScriptableValueBase<T> : ScriptableObject, IObservableValue<T>
{
    [Multiline]
    public string Description;//the value description, for what is this value used

    [SerializeField]
    protected T _value;//the serialized value

    public event ObservableValueChangedDelegate<T> ValueChanged;
    public T Value
    {
        get => _value;
        set
        {
            var changed = EqualityComparer<T>.Default.Equals(_value, value) == false;

            _value = value;

            if (changed)
            {
                Changed();
                ValueChanged?.Invoke(this);
            }
        }
    }

    public virtual void Changed()
    {
    }
}
