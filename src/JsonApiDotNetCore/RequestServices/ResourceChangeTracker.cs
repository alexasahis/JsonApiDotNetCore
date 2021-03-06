using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Serialization;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.RequestServices
{
    /// <summary>
    /// Used to determine whether additional changes to a resource, not specified in a PATCH request, have been applied.
    /// </summary>
    public interface IResourceChangeTracker<in TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Sets the exposed resource attributes as stored in database, before applying changes.
        /// </summary>
        void SetInitiallyStoredAttributeValues(TResource resource);

        /// <summary>
        /// Sets the subset of exposed attributes from the PATCH request.
        /// </summary>
        void SetRequestedAttributeValues(TResource resource);

        /// <summary>
        /// Sets the exposed resource attributes as stored in database, after applying changes.
        /// </summary>
        void SetFinallyStoredAttributeValues(TResource resource);

        /// <summary>
        /// Validates if any exposed resource attributes that were not in the PATCH request have been changed.
        /// And validates if the values from the PATCH request are stored without modification.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the attribute values from the PATCH request were the only changes; <c>false</c>, otherwise.
        /// </returns>
        bool HasImplicitChanges();
    }

    public sealed class ResourceChangeTracker<TResource> : IResourceChangeTracker<TResource> where TResource : class, IIdentifiable
    {
        private readonly IJsonApiOptions _options;
        private readonly IResourceContextProvider _contextProvider;
        private readonly ITargetedFields _targetedFields;

        private IDictionary<string, string> _initiallyStoredAttributeValues;
        private IDictionary<string, string> _requestedAttributeValues;
        private IDictionary<string, string> _finallyStoredAttributeValues;

        public ResourceChangeTracker(IJsonApiOptions options, IResourceContextProvider contextProvider,
            ITargetedFields targetedFields)
        {
            _options = options;
            _contextProvider = contextProvider;
            _targetedFields = targetedFields;
        }

        public void SetInitiallyStoredAttributeValues(TResource resource)
        {
            var resourceContext = _contextProvider.GetResourceContext<TResource>();
            _initiallyStoredAttributeValues = CreateAttributeDictionary(resource, resourceContext.Attributes);
        }

        public void SetRequestedAttributeValues(TResource resource)
        {
            _requestedAttributeValues = CreateAttributeDictionary(resource, _targetedFields.Attributes);
        }

        public void SetFinallyStoredAttributeValues(TResource resource)
        {
            var resourceContext = _contextProvider.GetResourceContext<TResource>();
            _finallyStoredAttributeValues = CreateAttributeDictionary(resource, resourceContext.Attributes);
        }

        private IDictionary<string, string> CreateAttributeDictionary(TResource resource,
            IEnumerable<AttrAttribute> attributes)
        {
            var result = new Dictionary<string, string>();

            foreach (var attribute in attributes)
            {
                object value = attribute.GetValue(resource);
                var json = JsonConvert.SerializeObject(value, _options.SerializerSettings);
                result.Add(attribute.PublicName, json);
            }

            return result;
        }

        public bool HasImplicitChanges()
        {
            foreach (var key in _initiallyStoredAttributeValues.Keys)
            {
                if (_requestedAttributeValues.ContainsKey(key))
                {
                    var requestedValue = _requestedAttributeValues[key];
                    var actualValue = _finallyStoredAttributeValues[key];

                    if (requestedValue != actualValue)
                    {
                        return true;
                    }
                }
                else
                {
                    var initiallyStoredValue = _initiallyStoredAttributeValues[key];
                    var finallyStoredValue = _finallyStoredAttributeValues[key];

                    if (initiallyStoredValue != finallyStoredValue)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
