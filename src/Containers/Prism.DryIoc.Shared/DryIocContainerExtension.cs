using System;
using System.Linq;
using DryIoc;
using Prism.Ioc;
using Prism.Ioc.Internals;

namespace Prism.DryIoc
{
    /// <summary>
    /// The <see cref="IContainerExtension" /> Implementation to use with DryIoc
    /// </summary>
    public partial class DryIocContainerExtension : IContainerExtension<IContainer>, IContainerInfo
    {
        private IResolverContext _currentScope;

        /// <summary>
        /// Gets the Default DryIoc Container Rules used by Prism
        /// </summary>
        public static Rules DefaultRules => Rules.Default.WithAutoConcreteTypeResolution()
                                                                       .With(Made.Of(FactoryMethod.ConstructorWithResolvableArguments))
#if HAS_WINUI || __IOS__
                                                                       .WithoutFastExpressionCompiler()
#endif
#if HAS_WINUI
                                                                       .WithTrackingDisposableTransients()
#endif
                                                                       .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace);

        /// <summary>
        /// The instance of the wrapped container
        /// </summary>
        public IContainer Instance { get; }

        /// <summary>
        /// Constructs a new <see cref="DryIocContainerExtension" />
        /// </summary>
        /// <param name="container">The <see cref="IContainer" /> instance to use.</param>
        public DryIocContainerExtension(IContainer container)
        {
            Instance = container;
            Instance.RegisterInstanceMany(new[]
            {
                typeof(IContainerExtension),
                typeof(IContainerProvider)
            }, this);
        }

        /// <summary>
        /// Used to perform any final steps for configuring the extension that may be required by the container.
        /// </summary>
        public void FinalizeExtension() { }

        /// <summary>
        /// Registers an instance of a given <see cref="Type"/>
        /// </summary>
        /// <param name="type">The service <see cref="Type"/> that is being registered</param>
        /// <param name="instance">The instance of the service or <see cref="Type" /></param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterInstance(Type type, object instance)
        {
            Instance.RegisterInstance(type, instance);
            return this;
        }

        /// <summary>
        /// Registers an instance of a given <see cref="Type"/> with the specified name or key
        /// </summary>
        /// <param name="type">The service <see cref="Type"/> that is being registered</param>
        /// <param name="instance">The instance of the service or <see cref="Type" /></param>
        /// <param name="name">The name or key to register the service</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterInstance(Type type, object instance, string name)
        {
            Instance.RegisterInstance(type, instance, serviceKey: name);
            return this;
        }

        /// <summary>
        /// Registers a Singleton with the given service and mapping to the specified implementation <see cref="Type" />.
        /// </summary>
        /// <param name="from">The service <see cref="Type" /></param>
        /// <param name="to">The implementation <see cref="Type" /></param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterSingleton(Type from, Type to)
        {
            Instance.Register(from, to, Reuse.Singleton);
            return this;
        }

        /// <summary>
        /// Registers a Singleton with the given service and mapping to the specified implementation <see cref="Type" />.
        /// </summary>
        /// <param name="from">The service <see cref="Type" /></param>
        /// <param name="to">The implementation <see cref="Type" /></param>
        /// <param name="name">The name or key to register the service</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterSingleton(Type from, Type to, string name)
        {
            Instance.Register(from, to, Reuse.Singleton, serviceKey: name);
            return this;
        }

        /// <summary>
        /// Registers a Singleton with the given service <see cref="Type" /> factory delegate method.
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <param name="factoryMethod">The delegate method.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterSingleton(Type type, Func<object> factoryMethod)
        {
            Instance.RegisterDelegate(type, r => factoryMethod(), Reuse.Singleton);
            return this;
        }

        /// <summary>
        /// Registers a Singleton with the given service <see cref="Type" /> factory delegate method.
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <param name="factoryMethod">The delegate method using <see cref="IContainerProvider"/>.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterSingleton(Type type, Func<IContainerProvider, object> factoryMethod)
        {
            Instance.RegisterDelegate(type, factoryMethod, Reuse.Singleton);
            return this;
        }

        /// <summary>
        /// Registers a Singleton Service which implements service interfaces
        /// </summary>
        /// <param name="type">The implementation <see cref="Type" />.</param>
        /// <param name="serviceTypes">The service <see cref="Type"/>'s.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        /// <remarks>Registers all interfaces if none are specified.</remarks>
        public IContainerRegistry RegisterManySingleton(Type type, params Type[] serviceTypes)
        {
            if (serviceTypes.Length == 0)
            {
                serviceTypes = type.GetInterfaces();
            }

            Instance.RegisterMany(serviceTypes, type, Reuse.Singleton);
            return this;
        }

        /// <summary>
        /// Registers a scoped service
        /// </summary>
        /// <param name="from">The service <see cref="Type" /></param>
        /// <param name="to">The implementation <see cref="Type" /></param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterScoped(Type from, Type to)
        {
            Instance.Register(from, to, Reuse.ScopedOrSingleton);
            return this;
        }

        /// <summary>
        /// Registers a scoped service using a delegate method.
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <param name="factoryMethod">The delegate method.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterScoped(Type type, Func<object> factoryMethod)
        {
            Instance.RegisterDelegate(type, r => factoryMethod(), Reuse.ScopedOrSingleton);
            return this;
        }

        /// <summary>
        /// Registers a scoped service using a delegate method.
        /// </summary>
        /// <param name="type">The service <see cref="Type"/>.</param>
        /// <param name="factoryMethod">The delegate method using the <see cref="IContainerProvider"/>.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterScoped(Type type, Func<IContainerProvider, object> factoryMethod)
        {
            Instance.RegisterDelegate(type, factoryMethod, Reuse.ScopedOrSingleton);
            return this;
        }

        /// <summary>
        /// Registers a Transient with the given service and mapping to the specified implementation <see cref="Type" />.
        /// </summary>
        /// <param name="from">The service <see cref="Type" /></param>
        /// <param name="to">The implementation <see cref="Type" /></param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry Register(Type from, Type to)
        {
            Instance.Register(from, to);
            return this;
        }

        /// <summary>
        /// Registers a Transient with the given service and mapping to the specified implementation <see cref="Type" />.
        /// </summary>
        /// <param name="from">The service <see cref="Type" /></param>
        /// <param name="to">The implementation <see cref="Type" /></param>
        /// <param name="name">The name or key to register the service</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry Register(Type from, Type to, string name)
        {
            Instance.Register(from, to, serviceKey: name);
            return this;
        }

        /// <summary>
        /// Registers a Transient Service using a delegate method
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <param name="factoryMethod">The delegate method.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry Register(Type type, Func<object> factoryMethod)
        {
            Instance.RegisterDelegate(type, r => factoryMethod());
            return this;
        }

        /// <summary>
        /// Registers a Transient Service using a delegate method
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <param name="factoryMethod">The delegate method using <see cref="IContainerProvider"/>.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry Register(Type type, Func<IContainerProvider, object> factoryMethod)
        {
            Instance.RegisterDelegate(type, factoryMethod);
            return this;
        }

        /// <summary>
        /// Registers a Transient Service which implements service interfaces
        /// </summary>
        /// <param name="type">The implementing <see cref="Type" />.</param>
        /// <param name="serviceTypes">The service <see cref="Type"/>'s.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        /// <remarks>Registers all interfaces if none are specified.</remarks>
        public IContainerRegistry RegisterMany(Type type, params Type[] serviceTypes)
        {
            if (serviceTypes.Length == 0)
            {
                serviceTypes = type.GetInterfaces();
            }

            Instance.RegisterMany(serviceTypes, type, Reuse.Transient);
            return this;
        }

        /// <summary>
        /// Resolves a given <see cref="Type"/>
        /// </summary>
        /// <param name="type">The service <see cref="Type"/></param>
        /// <returns>The resolved Service <see cref="Type"/></returns>
        public object Resolve(Type type) =>
            Resolve(type, Array.Empty<(Type, object)>());

        /// <summary>
        /// Resolves a given <see cref="Type"/>
        /// </summary>
        /// <param name="type">The service <see cref="Type"/></param>
        /// <param name="name">The service name/key used when registering the <see cref="Type"/></param>
        /// <returns>The resolved Service <see cref="Type"/></returns>
        public object Resolve(Type type, string name) =>
            Resolve(type, name, Array.Empty<(Type, object)>());

        /// <summary>
        /// Resolves a given <see cref="Type"/>
        /// </summary>
        /// <param name="type">The service <see cref="Type"/></param>
        /// <param name="parameters">Typed parameters to use when resolving the Service</param>
        /// <returns>The resolved Service <see cref="Type"/></returns>
        public object Resolve(Type type, params (Type Type, object Instance)[] parameters)
        {
            try
            {
                var container = _currentScope ?? Instance;
                return container.Resolve(type, args: parameters.Select(p => p.Instance).ToArray());
            }
            catch (Exception ex)
            {
                throw new ContainerResolutionException(type, ex);
            }
        }

        /// <summary>
        /// Resolves a given <see cref="Type"/>
        /// </summary>
        /// <param name="type">The service <see cref="Type"/></param>
        /// <param name="name">The service name/key used when registering the <see cref="Type"/></param>
        /// <param name="parameters">Typed parameters to use when resolving the Service</param>
        /// <returns>The resolved Service <see cref="Type"/></returns>
        public object Resolve(Type type, string name, params (Type Type, object Instance)[] parameters)
        {
            try
            {
                var container = _currentScope ?? Instance;
                return container.Resolve(type, name, args: parameters.Select(p => p.Instance).ToArray());
            }
            catch (Exception ex)
            {
                throw new ContainerResolutionException(type, name, ex);
            }
        }

        /// <summary>
        /// Determines if a given service is registered
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <returns><c>true</c> if the service is registered.</returns>
        public bool IsRegistered(Type type)
        {
            return Instance.IsRegistered(type);
        }

        /// <summary>
        /// Determines if a given service is registered with the specified name
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <param name="name">The service name or key used</param>
        /// <returns><c>true</c> if the service is registered.</returns>
        public bool IsRegistered(Type type, string name)
        {
            return Instance.IsRegistered(type, name);
        }

        Type IContainerInfo.GetRegistrationType(string key)
        {
            var matchingRegistration = Instance.GetServiceRegistrations().Where(r => key.Equals(r.OptionalServiceKey?.ToString(), StringComparison.Ordinal)).FirstOrDefault();
            if (matchingRegistration.OptionalServiceKey == null)
                matchingRegistration = Instance.GetServiceRegistrations().Where(r => key.Equals(r.ImplementationType.Name, StringComparison.Ordinal)).FirstOrDefault();

            return matchingRegistration.ImplementationType;
        }

        /// <summary>
        /// Creates a new Scope
        /// </summary>
        public virtual void CreateScope() =>
            CreateScopeInternal();

        /// <summary>
        /// Creates a new Scope and provides the updated ServiceProvider
        /// </summary>
        /// <returns>The Scoped <see cref="IResolverContext" />.</returns>
        /// <remarks>
        /// This should be called by custom implementations that Implement IServiceScopeFactory
        /// </remarks>
        protected IResolverContext CreateScopeInternal()
        {
            if (_currentScope != null)
            {
                _currentScope.Dispose();
                _currentScope = null;
                GC.Collect();
            }

            _currentScope = Instance.OpenScope();
            return _currentScope;
        }
    }
}
