using System;
using System.Collections.Generic;
using SalesApp.Core.Extensions;

namespace SalesApp.Core.Services.DependancyInjection
{
    /// <summary>
    /// This class functions as a simple IoC container to allow for inversion of control for specific classes who need device specific implementations.
    /// </summary>
    public class Resolver
    {
        private static readonly Lazy<Resolver> Lazy = new Lazy<Resolver>(() => new Resolver());
        private static readonly Dictionary<string, Type> Types = new Dictionary<string, Type>();
        private static readonly Dictionary<string, object> Instances = new Dictionary<string, object>();
        private static readonly Dictionary<string, Action<object>> PostInstantiationCallbacks = new Dictionary<string, Action<object>>();

        public event EventHandler<EventArgs> TypeResolved;

        private Resolver()
        {
        }

        public static Resolver Instance
        {
            get { return Lazy.Value; }
        }

        /// <summary>
        /// Get a instance of Type T, the real implementation will be returned.
        /// </summary>
        /// <typeparam name="T">Interface type T</typeparam>
        /// /// <param name="args">Object args to be injected</param>
        /// <returns>Device specific implementation of type T</returns>
        public T Get<T>(object args = null)
        {
            string name = typeof(T).AssemblyQualifiedName;
            if (Instances.ContainsKey(name))
            {
                return (T)Instances[name];
            }

            if (Types.ContainsKey(name))
            {
                T result = (T)Activator.CreateInstance(Types[name]);
                if (this.TypeResolved != null)
                {
                    this.TypeResolved.Invoke(result, EventArgs.Empty);
                }

                if (args != null)
                {
                    ArgumentReceiverBase argReceiver = result as ArgumentReceiverBase;

                    if (argReceiver != null)
                    {
                        argReceiver.Receive(args);
                    }
                }

                if (PostInstantiationCallbacks.ContainsKey(name))
                {
                    PostInstantiationCallbacks[name](result);
                }

                return result;
            }

            throw new Exception(string.Format("Type [{0}] not bound to real implementation, cannot be resolved.", name));
        }

        /// <summary>
        /// Register a specific implementation TType which implements TInterface
        /// </summary>
        /// <typeparam name="TInterface">Generic interface</typeparam>
        /// <typeparam name="TType">Specific implementation</typeparam>
        public void Register<TInterface, TType>(Action<object> postInstantiationCallback = null)
            where TInterface : class
            where TType : TInterface
        {
            // we need to store per AssemblyQualifiedName, dictionaries do not have keys of Type
            string key = typeof(TInterface).AssemblyQualifiedName;
            if (!Types.ContainsKey(key))
            {
                Types.Add(key, typeof(TType));
            }

            if (postInstantiationCallback != null)
            {
                if (PostInstantiationCallbacks.ContainsKey(key))
                {
                    PostInstantiationCallbacks.Remove(key);
                }

                PostInstantiationCallbacks.Add(key, postInstantiationCallback);
            }
        }

        /// <summary>
        /// Register a specific implementation TType which implements TInterface
        /// </summary>
        /// <typeparam name="TInterface">Generic interface</typeparam>
        public void RegisterSingleton<TInterface>(TInterface instance)
            where TInterface : class
        {
            // we need to store per AssemblyQualifiedName, dictionaries do not have keys of Type
            string key = typeof(TInterface).AssemblyQualifiedName;
            if (Instances.ContainsKey(key))
            {
                "Key called ~ already exists, lets overwrite with instance".WriteLine(key);
                Instances.Remove(key);
            }

            Instances.Add(key, instance);
        }

        public void RegisterClass<TClass>()
        {
            string key = typeof(TClass).AssemblyQualifiedName;
            if (Types.ContainsKey(key))
            {
                "Key called ~ already exists, lets exit".WriteLine(key);
                return;
            }

            "Key called ~ does not exist, lets add it".WriteLine(key);
            Types.Add(key, typeof(TClass));
        }

        public TType GetInstance<TType>()
        {
            if (!Types.ContainsKey(typeof(TType).AssemblyQualifiedName))
            {
                RegisterClass<TType>();
            }

            return Get<TType>();
        }

        /*public void RegisterName<TType>(string name)
        {
            _types.Add(name, typeof(TType));
        }

        public object GetNamed(string name)
        {
            if (_types.ContainsKey(name))
            {
                return Activator.CreateInstance(_types[name]);
            }

            throw new Exception(String.Format("Type [{0}] not bound to real implementation, cannot be resolved.", name));
        }

        public bool HasSingleton<TInterface>()
        {
            return _instances.ContainsKey(typeof(TInterface).AssemblyQualifiedName);
        }*/

        public void Clear()
        {
            Types.Clear();
        }
    }
}