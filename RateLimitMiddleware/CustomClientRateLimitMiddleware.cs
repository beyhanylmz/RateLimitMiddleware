using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RateLimitMiddleware;

public class CustomClientRateLimitMiddleware : ClientRateLimitMiddleware
{
    public CustomClientRateLimitMiddleware(RequestDelegate next, IProcessingStrategy processingStrategy, IOptions<ClientRateLimitOptions> options, IClientPolicyStore policyStore, IRateLimitConfiguration config, ILogger<ClientRateLimitMiddleware> logger) : base(next, processingStrategy, options, policyStore, config, logger)
    {
    }

    public override Task ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitRule rule, string retryAfter)
    {
        string? requestPath = httpContext?.Request?.Path.Value;
        var result = JsonSerializer.Serialize( new RateLimitResponse()
        {
            IsSuccessful= false,
            Message = "Istek limiti doldu.",
            Errors = new List<string>()
            {
                $"Maksimum istek limit sayı doldu. {rule.Period} basina {rule.Limit} istek gönderebilirsiniz. Lutfen {retryAfter} saniye sonra yenidən deneyin."
                
            }
        });
        httpContext.Response.Headers["Retry-After"] = retryAfter;
        httpContext.Response.StatusCode = 402;
        httpContext.Response.ContentType = "application/json";

        Console.WriteLine("Istek limiti doldu. RequestPath: "+ requestPath +"Retryafter: "+retryAfter);
        return httpContext.Response.WriteAsync(result);
        //return base.ReturnQuotaExceededResponse(httpContext, rule, retryAfter);
    }

    private class RateLimitResponse
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
    }
}