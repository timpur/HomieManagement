using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Conventions;
using GraphQL.Conventions.Adapters;
using GraphQL.Conventions.Adapters.Engine.Listeners.DataLoader;
using GraphQL.Conventions.Builders;
using GraphQL.Conventions.Execution;
using GraphQL.Conventions.Types.Descriptors;
using GraphQL.Conventions.Types.Resolution;
using GraphQL.Execution;
using GraphQL.Http;
using GraphQL.Instrumentation;
using GraphQL.Types;
using GraphQL.Utilities;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;

namespace HomieManagement
{
    public class CustomGraphQLEngine
    {
        readonly TypeResolver _typeResolver = new TypeResolver();

        readonly GraphTypeAdapter _graphTypeAdapter = new GraphTypeAdapter();

        readonly SchemaConstructor<ISchema, IGraphType> _constructor;

        readonly DocumentExecuter _documentExecutor = new DocumentExecuter();

        readonly IDocumentBuilder _documentBuilder = new GraphQLDocumentBuilder();

        readonly DocumentValidator _documentValidator = new DocumentValidator();

        readonly DocumentWriter _documentWriter = new DocumentWriter();

        SchemaPrinter _schemaPrinter;

        ISchema _schema;

        List<System.Type> _schemaTypes = new List<System.Type>();

        List<System.Type> _middleware = new List<System.Type>();

        bool _includeFieldDescriptions;

        bool _includeFieldDeprecationReasons;

        class NoopValidationRule : IValidationRule
        {
            public INodeVisitor Validate(ValidationContext context)
            {
                return new EnterLeaveListener(_ => { });
            }
        }

        class WrappedDependencyInjector : IDependencyInjector
        {
            readonly Func<System.Type, object> _typeResolutionDelegate;

            public WrappedDependencyInjector(Func<System.Type, object> typeResolutionDelegate)
            {
                _typeResolutionDelegate = typeResolutionDelegate;
            }

            public object Resolve(System.Reflection.TypeInfo typeInfo)
            {
                return _typeResolutionDelegate(typeInfo.AsType());
            }
        }

        public CustomGraphQLEngine(Func<System.Type, object> typeResolutionDelegate = null, TypeResolver typeResolver = null)
        {
            _typeResolver = typeResolver ?? _typeResolver;
            _constructor = new SchemaConstructor<ISchema, IGraphType>(_graphTypeAdapter, _typeResolver);
            _constructor.TypeResolutionDelegate = typeResolutionDelegate != null
                ? (Func<System.Type, object>)(type => typeResolutionDelegate(type) ?? CreateInstance(type))
                : (Func<System.Type, object>)CreateInstance;
        }

        public static CustomGraphQLEngine New(Func<System.Type, object> typeResolutionDelegate = null)
        {
            return new CustomGraphQLEngine(typeResolutionDelegate);
        }

        public static CustomGraphQLEngine New<TQuery>(Func<System.Type, object> typeResolutionDelegate = null)
        {
            return New(typeResolutionDelegate)
                .WithQuery<TQuery>()
                .BuildSchema();
        }

        public static CustomGraphQLEngine New<TQuery, TMutation>(Func<System.Type, object> typeResolutionDelegate = null)
        {
            return New(typeResolutionDelegate)
                .WithQueryAndMutation<TQuery, TMutation>()
                .BuildSchema();
        }

        // public CustomGraphQLEngine WithFieldResolutionStrategy(FieldResolutionStrategy strategy)
        // {
        //     switch (strategy)
        //     {
        //         default:
        //             _graphTypeAdapter.FieldResolverFactory = (FieldInfo) => new FieldResolver(FieldInfo);
        //             break;
        //         case FieldResolutionStrategy.WrappedAsynchronous:
        //             _graphTypeAdapter.FieldResolverFactory = (FieldInfo) => new WrappedAsyncFieldResolver(FieldInfo);
        //             break;
        //         case FieldResolutionStrategy.WrappedSynchronous:
        //             _graphTypeAdapter.FieldResolverFactory = (FieldInfo) => new WrappedSyncFieldResolver(FieldInfo);
        //             break;
        //     }
        //     return this;
        // }

        public CustomGraphQLEngine WithQuery<TQuery>()
        {
            _schemaTypes.Add(typeof(SchemaDefinition<TQuery>));
            return this;
        }

        public CustomGraphQLEngine WithMutation<TMutation>()
        {
            _schemaTypes.Add(typeof(SchemaDefinitionWithMutation<TMutation>));
            return this;
        }

        public CustomGraphQLEngine WithQueryAndMutation<TQuery, TMutation>()
        {
            _schemaTypes.Add(typeof(SchemaDefinition<TQuery, TMutation>));
            return this;
        }

        public CustomGraphQLEngine WithSubscription<TSubscription>()
        {
            _schemaTypes.Add(typeof(SchemaDefinitionWithSubscription<TSubscription>));
            return this;
        }

        public CustomGraphQLEngine WithAttributesFromAssembly(System.Type assemblyType)
        {
            _typeResolver.RegisterAttributesInAssembly(assemblyType);
            return this;
        }

        public CustomGraphQLEngine WithAttributesFromAssembly<TAssemblyType>()
        {
            return WithAttributesFromAssembly(typeof(TAssemblyType));
        }

        public CustomGraphQLEngine WithAttributesFromAssemblies(IEnumerable<System.Type> assemblyTypes)
        {
            foreach (var assemblyType in assemblyTypes)
            {
                WithAttributesFromAssembly(assemblyType);
            }
            return this;
        }

        public CustomGraphQLEngine WithMiddleware(System.Type type)
        {
            _middleware.Add(type);
            return this;
        }

        public CustomGraphQLEngine WithMiddleware<T>()
        {
            return WithMiddleware(typeof(T));
        }

        public CustomGraphQLEngine PrintFieldDescriptions(bool include = true)
        {
            _includeFieldDescriptions = include;
            return this;
        }

        public CustomGraphQLEngine PrintFieldDeprecationReasons(bool include = true)
        {
            _includeFieldDeprecationReasons = include;
            return this;
        }

        public CustomGraphQLEngine BuildSchema(params System.Type[] types)
        {
            if (_schema == null)
            {
                if (types.Length > 0)
                {
                    _schemaTypes.AddRange(types);
                }
                _schema = _constructor.Build(_schemaTypes.ToArray());
                _schemaPrinter = new SchemaPrinter(
                    _schema,
                    new SchemaPrinterOptions
                    {
                        CustomScalars = new List<string> { TypeNames.Url, TypeNames.Uri, TypeNames.TimeSpan, TypeNames.Guid },
                        IncludeDescriptions = _includeFieldDescriptions,
                        IncludeDeprecationReasons = _includeFieldDeprecationReasons,
                    }
                );
            }
            return this;
        }

        public string Describe()
        {
            BuildSchema(); // Ensure that the schema has been constructed
            return _schemaPrinter.Print();
        }

        // public GraphQLExecutor NewExecutor(IRequestDeserializer requestDeserializer = null)
        // {
        //     BuildSchema(); // Ensure that the schema has been constructed
        //     return new GraphQLExecutor(this, requestDeserializer ?? new RequestDeserializer());
        // }

        public CustomGraphQLEngine RegisterScalarType<TType, TGraphType>(string name = null)
        {
            _typeResolver.RegisterScalarType<TType>(name ?? typeof(TType).Name);
            _graphTypeAdapter.RegisterScalarType<TGraphType>(name ?? typeof(TType).Name);
            return this;
        }

        public string SerializeResult(ExecutionResult result)
        {
            return _documentWriter.Write(result);
        }

        public ISchema GetSchema()
        {
            return _schema;
        }

        internal async Task<ExecutionResult> Execute(
            object rootObject,
            string query,
            string operationName,
            Inputs inputs,
            IUserContext userContext,
            IDependencyInjector dependencyInjector,
            ComplexityConfiguration complexityConfiguration,
            bool enableValidation = true,
            bool enableProfiling = false,
            IEnumerable<IValidationRule> rules = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!enableValidation)
            {
                rules = new[] { new NoopValidationRule() };
            }
            var configuration = new ExecutionOptions
            {
                Schema = _schema,
                Root = rootObject,
                Query = query,
                OperationName = operationName,
                Inputs = inputs,
                // UserContext = UserContextWrapper.Create(userContext, dependencyInjector ?? new WrappedDependencyInjector(_constructor.TypeResolutionDelegate)),
                ValidationRules = rules != null && rules.Any() ? rules : null,
                ComplexityConfiguration = complexityConfiguration,
                CancellationToken = cancellationToken,
            };

            if (userContext is IDataLoaderContextProvider)
            {
                // configuration.Listeners.Add(new DataLoaderListener());
            }

            if (enableProfiling)
            {
                configuration.FieldMiddleware.Use<InstrumentFieldsMiddleware>();
            }

            foreach (var middleware in _middleware)
            {
                configuration.FieldMiddleware.Use(middleware);
            }

            var result = await _documentExecutor.ExecuteAsync(configuration).ConfigureAwait(false);

            if (result.Errors != null)
            {
                var errors = new ExecutionErrors();
                foreach (var executionError in result.Errors)
                {
                    var exception = new FieldResolutionException(executionError);
                    var error = new ExecutionError(exception.Message, exception);
                    foreach (var location in executionError.Locations ?? new ErrorLocation[0])
                    {
                        error.AddLocation(location.Line, location.Column);
                    }
                    error.Path = executionError.Path;
                    errors.Add(error);
                }
                result.Errors = errors;
            }

            return result;
        }

        internal IValidationResult Validate(string queryString)
        {
            var document = _documentBuilder.Build(queryString);
            return _documentValidator.Validate(queryString, _schema, document);
        }

        object CreateInstance(System.Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsAbstract && !typeInfo.ContainsGenericParameters)
            {
                var ctors = typeInfo.GetConstructors().ToList();
                if (ctors.All(ctor => ctor.GetParameters().Any()))
                {
                    var ctor = ctors.FirstOrDefault();
                    if (ctor == null)
                    {
                        return null;
                    }
                    var parameters = ctor.GetParameters();
                    var parameterValues = parameters
                        .Select(parameter => _constructor.TypeResolutionDelegate(parameter.ParameterType))
                        .ToArray();
                    return ctor.Invoke(parameterValues);
                }
            }

            return Activator.CreateInstance(type);
        }
    }
}
