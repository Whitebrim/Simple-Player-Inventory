using System;
using System.Collections.Generic;

namespace Game.Infrastructure
{
    public class DIContainer
    {
        private readonly Dictionary<Type, object> _registrations = new();

        public void Register<T>(T instance)
        {
            _registrations[typeof(T)] = instance;
        }

        public T Resolve<T>()
        {
            if (_registrations.TryGetValue(typeof(T), out var instance))
                return (T)instance;

            throw new InvalidOperationException($"Type {typeof(T).Name} is not registered in the container.");
        }
    }
}
