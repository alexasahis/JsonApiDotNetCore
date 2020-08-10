using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal.QueryStrings;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Entry point for processing all query string parameters.
    /// </summary>
    public interface IQueryStringActionFilter : IAsyncActionFilter { }
    public sealed class QueryStringActionFilter : IQueryStringActionFilter
    {
        private readonly IQueryStringReader _queryStringReader;

        public QueryStringActionFilter(IQueryStringReader queryStringReader)
        {
            _queryStringReader = queryStringReader;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.IsJsonApiRequest())
            {
                return;
            }
            
            var disableQueryAttribute = context.Controller.GetType().GetCustomAttribute<DisableQueryAttribute>();

            _queryStringReader.ReadAll(disableQueryAttribute);
            await next();
        }
    }
}
