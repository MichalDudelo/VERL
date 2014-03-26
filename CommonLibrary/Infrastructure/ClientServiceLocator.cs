using System;
using Autofac; 

namespace Common_Library
{
    /// <summary>
    /// Service locator for use by infrastructure classes.
    /// Avoid using in business scenarios.
    /// </summary>
    public class ClientServiceLocator
    {
        private static ClientServiceLocator _instance;

        public static ClientServiceLocator Instance
        {
            get { return _instance; }
        }

        private static object _syncRoot = new object();
        
        private readonly IContainer _container;

        private ClientServiceLocator(IContainer container)
        {
            _container = container;
        }


        public static void Setup(IContainer container, bool allowOverride = false)
        {
            lock (_syncRoot)
            {
                if (_instance != null && !allowOverride)
                {
                        throw new InvalidOperationException("Service locator already initialized");
                }

                _instance = new ClientServiceLocator(container);
            }
        }

        public T Find<T>()
        {
            return _container.Resolve<T>();
        }

        public object Find(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        public object TryFind(Type serviceType)
        {
            object obj;
            if (_container.TryResolve(serviceType, out obj))
            {
                return obj;
            }
            return null;
        }
    }
}