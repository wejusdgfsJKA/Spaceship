using System;
using System.Collections.Generic;

public class BlackBoard
{
    public class Data
    {
        protected object currentValue;
        public object Value
        {
            get
            {
                return currentValue;
            }
            set
            {
                currentValue = value;
                if (OnValueChanged != null)
                {
                    OnValueChanged();
                }
            }
        }

        public event Action OnValueChanged;
        public Data(object _value)
        {
            Value = _value;
        }
    }
    protected Dictionary<string, Data> data = new();
    public void SetData<T>(string _key, T _value)
    {
        if (data.ContainsKey(_key))
        {
            data[_key].Value = _value;
        }
        else
        {
            data.Add(_key, new Data(_value));
        }
    }
    public void AddListener(Action _action, string _key)
    {
        if (data.ContainsKey(_key))
        {
            data[_key].OnValueChanged += _action;
        }
    }
    public void RemoveListener(Action _action, string _key)
    {
        if (data.ContainsKey(_key))
        {
            data[_key].OnValueChanged -= _action;
        }
    }
    public T GetData<T>(string _key)
    {
        if (data.ContainsKey(_key))
        {
            return (T)data[_key].Value;
        }
        return default;
    }
    public BlackBoard()
    {
        data = new();
    }
}