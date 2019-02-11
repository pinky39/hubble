using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Hubble.Web
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ThrottleAttribute : ActionFilterAttribute
    {     
        public int Seconds { get; set; }        
        public override void OnActionExecuting(ActionExecutingContext c)
        {
            var cache = (IMemoryCache)c.HttpContext.RequestServices
                .GetService(typeof(IMemoryCache));
                           
            var key = string.Concat(
                c.HttpContext.Request.Path, 
                "-", 
                c.HttpContext.Connection.RemoteIpAddress);
            
            var allowExecute = false;

            if (cache.Get(key) == null)
            {
                var options = new MemoryCacheEntryOptions()
                  .SetAbsoluteExpiration(DateTime.Now.AddSeconds(Seconds))
                  .SetPriority(CacheItemPriority.Low);

                cache.Set(key, true, options);

                allowExecute = true;
            }

            if (!allowExecute)
            {
                c.Result = new ContentResult { 
                    Content = $"You may only perform this action every {Seconds} seconds." 
                };
                // see 409 - http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html
                c.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
            }
        }
    }
}
