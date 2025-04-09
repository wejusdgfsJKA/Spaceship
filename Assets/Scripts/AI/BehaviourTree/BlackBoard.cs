using System;
using System.Collections.Generic;

namespace BTree
{
    public class BlackBoard
    {
        /// <summary>
        /// This is a wrapper for a resource which decorators can keep track of.
        /// </summary>
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
                    //this is used so listeners know when the value is changed
                    currentValue = value;
                    if (OnValueChanged != null)
                    {
                        OnValueChanged();
                    }
                }
            }

            public event Action OnValueChanged;
            public Data(object value)
            {
                Value = value;
            }
        }
        protected Dictionary<string, Data> data = new();
        /// <summary>
        /// Set the value of a resource given by a key. If the key cannot be found, 
        /// a new pair will be added to the internal HashMap.
        /// </summary>
        /// <typeparam name="T">The datatype of the resource.</typeparam>
        /// <param name="key">The key which will identify the resource.</param>
        /// <param name="value">The value of the resource.</param>
        public void SetData<T>(string key, T value)
        {
            Data resource;
            if (data.TryGetValue(key, out resource))
            {
                resource.Value = value;
            }
            else
            {
                data.Add(key, new Data(value));
            }
        }
        /// <summary>
        /// When the value of the resource given by the key changes, the given action will fire.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="key"></param>
        public void AddListener(Action action, string key)
        {
            Data resource;
            if (data.TryGetValue(key, out resource))
            {
                resource.OnValueChanged += action;
            }
        }
        /// <summary>
        /// Remove an action that was tied to this resource.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="key"></param>
        public void RemoveListener(Action action, string key)
        {
            Data resource;
            if (data.TryGetValue(key, out resource))
            {
                resource.OnValueChanged -= action;
            }
        }
        /// <summary>
        /// Get data stored at this key.
        /// </summary>
        /// <typeparam name="T">The datatype of the resource.</typeparam>
        /// <param name="key">The key which identifies the resource.</param>
        /// <returns>Returns the value of the resource if the key is found, 
        /// or the default value otherwise.</returns>
        public T GetData<T>(string key)
        {
            Data resource;
            if (data.TryGetValue(key, out resource))
            {
                return (T)resource.Value;
            }
            return default;
        }
        public BlackBoard()
        {
            data = new();
        }
    }
}