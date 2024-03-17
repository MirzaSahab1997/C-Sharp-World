using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Security.Claims;

namespace UserManagement.Application.Filters
{
    public class AuthenticationFilterAttribute : IActionFilter
    {
        private readonly ILogger<AuthenticationFilterAttribute> _logger;
        private readonly IConfiguration _configuration;

        public AuthenticationFilterAttribute(ILogger<AuthenticationFilterAttribute> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Request.Headers["Authorization"].ToString();
            var jwtToken = token.Replace("Bearer ", "");
            string url = _configuration["BaseUrl:AuthService"] + "claim";
            try
            {
                HttpClient client = new();

                HttpResponseMessage webResponse = client.GetAsync(url + "?token=" + jwtToken + "&claimName=Id").Result;
                if (webResponse.IsSuccessStatusCode)
                {
                    var URI = context.HttpContext.Request.Path.Value;
                    object Id = null;

                    if (URI.Contains("user"))
                    {
                        Id = context.HttpContext.Request.RouteValues
                       .FirstOrDefault(kvp => kvp.Key.Equals("id", StringComparison.OrdinalIgnoreCase)).Value;
                    }

                    if (Id != null && Guid.Parse(Id.ToString()) != Guid.Parse(webResponse.Content.ReadAsStringAsync().Result))
                    {
                        context.Result = new ForbidResult();
                    }

                    var usernameClaim = context.HttpContext.User.FindFirst("UserName");
                    if (usernameClaim != null)
                    {
                        var claimsIdentity = (ClaimsIdentity)context.HttpContext.User.Identity;
                        claimsIdentity.AddClaim(new Claim("UserName", usernameClaim.Value));
                    }
                }
                else
                {
                    context.Result = new ForbidResult();
                }
            }

            catch (WebException)
            {
                context.Result = new ForbidResult();
                _logger.LogError("Authentication failed");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // our code after action executes
        }
    }
}
