using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State<T> : IState<T>
{
    Dictionary<T, IState<T>> _transitions = new Dictionary<T, IState<T>>();
    public IState<T> GetTransition(T input)
    {
        if (_transitions.ContainsKey(input))
            return _transitions[input];
        return null;
    }
    public void AddTransition(T input, IState<T> state)
    {
        _transitions[input] = state;
    }
    public void RemoveTransition(IState<T> state)
    {
        foreach (var item in _transitions)
        {
            if (item.Value == state)
            {
                _transitions.Remove(item.Key);
                break;
            }
        }
    }
    public void RemoveTransition(T input)
    {
        if (_transitions.ContainsKey(input))
        {
            _transitions.Remove(input);
        }
    }
    public virtual void Awake()
    {
    }
    public virtual void Execute()
    {

    }
    public virtual void Sleep()
    {

    } 
}
