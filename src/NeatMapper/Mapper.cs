using NeatMapper.Common.Mapper;
using NeatMapper.Configuration;

namespace NeatMapper{
    public class Mapper : BaseMapper, IMapper{
        protected readonly MappingContext _mappingContext;

        protected override MatchingContext MatchingContext => _mappingContext;

        public Mapper(MapperConfigurationOptions configuration, IServiceProvider serviceProvider) :
            base(new MapperConfiguration(i => i == typeof(INewMap<,>)
#if NET7_0_OR_GREATER
                || i == typeof(INewMapStatic<,>)
#endif
                ,
                i => i == typeof(IMergeMap<,>)
#if NET7_0_OR_GREATER
                || i == typeof(IMergeMapStatic<,>)
#endif
                , configuration), serviceProvider)
        {

            _mappingContext = new MappingContext
            {
                ServiceProvider = serviceProvider,
                Mapper = this,
                Matcher = this
            };
        }


        public object? Map(object? source, Type sourceType, Type destinationType)
        {
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));
            if (source?.GetType().IsAssignableTo(sourceType) == false)
                throw new ArgumentException($"Object of type {source.GetType().FullName} is not assignable to type {sourceType.FullName}", nameof(source));
            if (destinationType == null)
                throw new ArgumentNullException(nameof(destinationType));

            var types = (From: sourceType, To: destinationType);
            object? result;
            try
            {
                result = MapInternal(types, newMaps)
                    .Invoke(new object?[] { source, _mappingContext });
            }
            catch (MapNotFoundException exc)
            {
                try
                {
                    result = MapCollectionNewRecursiveInternal(types).Invoke(new object[] { source!, _mappingContext });
                }
                catch (MapNotFoundException)
                {
                    object destination;
                    try
                    {
                        destination = CreateDestinationFactory(destinationType).Invoke();
                    }
                    catch (DestinationCreationException)
                    {
                        throw exc;
                    }

                    result = Map(source, sourceType, destination, destinationType);
                }
            }

            // Should not happen
            if (result?.GetType().IsAssignableTo(destinationType) == false)
                throw new InvalidOperationException($"Object of type {result.GetType().FullName} is not assignable to type {destinationType.FullName}");

            return result;
        }

        public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null)
        {
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));
            if (source?.GetType().IsAssignableTo(sourceType) == false)
                throw new ArgumentException($"Object of type {source.GetType().FullName} is not assignable to type {sourceType.FullName}", nameof(source));
            if (destinationType == null)
                throw new ArgumentNullException(nameof(destinationType));
            if (destination?.GetType().IsAssignableTo(destinationType) == false)
                throw new ArgumentException($"Object of type {destination.GetType().FullName} is not assignable to type {destinationType.FullName}", nameof(destination));

            var types = (From: sourceType, To: destinationType);
            object? result;
            try
            {
                result = MapInternal(types, mergeMaps)
                    .Invoke(new object?[] { source, destination, _mappingContext });
            }
            catch (MapNotFoundException exc)
            {
                try
                {
                    result = MapCollectionMergeRecursiveInternal(types, destination, mappingOptions).Invoke(new object[] { source!, destination!, _mappingContext });
                }
                catch (MapNotFoundException)
                {
                    throw exc;
                }
            }

            // Should not happen
            if (result?.GetType().IsAssignableTo(destinationType) == false)
                throw new InvalidOperationException($"Object of type {result.GetType().FullName} is not assignable to type {destinationType.FullName}");

            return result;
        }
    }
}
