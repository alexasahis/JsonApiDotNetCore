using System.Linq;
using System.Net.Http;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Action filter used to verify the incoming type matches the target type, else return a 409
    /// </summary>
    public interface IJsonApiTypeMatchFilter : IActionFilter { }
    public sealed class JsonApiTypeMatchFilter : IJsonApiTypeMatchFilter
    {
        private readonly IResourceContextProvider _provider;

        public JsonApiTypeMatchFilter(IResourceContextProvider provider)
        {
            _provider = provider;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.IsJsonApiRequest())
            {
                return;
            }

            var request = context.HttpContext.Request;
            if (request.Method != "PATCH" && request.Method != "POST")
            {
                return;
            }
            
            var deserializedType = context.ActionArguments.FirstOrDefault().Value?.GetType();
            var targetType = context.ActionDescriptor.Parameters.FirstOrDefault()?.ParameterType;

            if (deserializedType == null || targetType == null || deserializedType == targetType)
            {
                return;
            }
            
            var resourceFromEndpoint = _provider.GetResourceContext(targetType);
            var resourceFromBody = _provider.GetResourceContext(deserializedType);

            throw new ResourceTypeMismatchException(new HttpMethod(request.Method), request.Path, resourceFromEndpoint, resourceFromBody);
        }

        public void OnActionExecuted(ActionExecutedContext context) { /* noop */ }
    }
}
