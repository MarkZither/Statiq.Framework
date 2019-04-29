﻿using System;
using System.Collections.Generic;
using Wyam.Common.Configuration;

namespace Wyam.App.Configuration
{
    internal class ConfiguratorCollection : IConfiguratorCollection
    {
        private readonly Dictionary<Type, List<object>> _configurators = new Dictionary<Type, List<object>>();

        public void Add<TConfigurable, TConfigurator>()
            where TConfigurable : class
            where TConfigurator : class, IConfigurator<TConfigurable> =>
            Get<TConfigurable>().Add(Activator.CreateInstance<TConfigurator>());

        public void Add<TConfigurable>(Action<TConfigurable> action)
            where TConfigurable : class =>
            Add(new DelegateConfigurator<TConfigurable>(action));

        public void Add<TConfigurable>(IConfigurator<TConfigurable> configurator)
            where TConfigurable : class =>
            Get<TConfigurable>().Add(configurator);

        public IList<IConfigurator<TConfigurable>> Get<TConfigurable>()
            where TConfigurable : class
        {
            if (!_configurators.TryGetValue(typeof(TConfigurable), out List<object> list))
            {
                list = new List<object>();
                _configurators.Add(typeof(TConfigurable), list);
            }
            return new ConfiguratorList<TConfigurable>(list);
        }

        public bool TryGet<TConfigurable>(out IList<IConfigurator<TConfigurable>> configurators)
            where TConfigurable : class
        {
            if (_configurators.TryGetValue(typeof(TConfigurable), out List<object> list))
            {
                configurators = new ConfiguratorList<TConfigurable>(list);
                return true;
            }
            configurators = null;
            return false;
        }
    }
}
