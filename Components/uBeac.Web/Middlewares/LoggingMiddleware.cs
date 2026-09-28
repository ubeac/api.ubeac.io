using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace uBeac.Web.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<LoggingMiddleware> _logger;
        private readonly bool _needLog; 

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            if (logger is null)
            {
                _needLog = false;
            }
            else
            {
                _needLog = true;
                _logger = logger;
            }

            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!_needLog)
            {
                await next(context);
                return;
            }

            Stream originalBody = null;

            TraceInfo _traceInfo = new TraceInfo() { StartDate = DateTime.UtcNow };
            _traceInfo.Response = new ResponseInfo();

            try
            {

                originalBody = context.Response.Body;

                using (var memStream = new MemoryStream())
                {
                    context.Response.Body = memStream;

                    await next(context);

                    memStream.Position = 0;
                    string responseBody = new StreamReader(memStream).ReadToEnd();

                    _traceInfo.Response.Length = memStream.Length;

                    memStream.Position = 0;

                    await memStream.CopyToAsync(originalBody);
                }

                _traceInfo.TraceId = context.TraceIdentifier;
                _traceInfo.Request = GetRequestInfo(context);
                _traceInfo.Response.StatusCode = context.Response.StatusCode;

                Log(_traceInfo);

            }
            catch (Exception ex)
            {
                _traceInfo.Response.StatusCode = 500;
                _traceInfo.Exception = ex;
                Log(_traceInfo);
                throw ex;
            }
            finally
            {
                context.Response.Body = originalBody;
            }

        }

        private RequestInfo GetRequestInfo(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var data = new RequestInfo()
            {
                Method = request.Method,
                ContentType = request.ContentType,
                ContentLength = request.ContentLength,
                Host = request.Host.Host,
                Port = request.Host.Port,
                Scheme = request.Scheme,
                QueryString = request.QueryString.ToString(),
                Protocol = request.Protocol,
                HasFormContentType = request.HasFormContentType,
                Path = request.Path.Value,
                Headers = request.Headers.ToDictionary(item => item.Key, item => item.Value.ToList()),
                Length = request.ContentLength,
                Ip = context.Connection.RemoteIpAddress.ToString(),
                //UserId = applicationContext.User.UserId,
                Username = context.User.Identity.Name
            };

            // todo: remove this to the middleware pipeline after implementing JWT role based authentication
            if (context.User.Identity.IsAuthenticated)
            {
                var userIdString = context.User?.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userIdString))
                {
                    userIdString = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }

                if (!string.IsNullOrEmpty(userIdString))
                {
                    data.UserId = new Guid(userIdString);
                }
            }

            return data;
        }

        private void Log(TraceInfo traceInfo)
        {
            if (traceInfo is null)
            {
                _logger.LogCritical("Trace Info is null!");
                return;
            }

            var span = DateTime.UtcNow - traceInfo.StartDate;
            traceInfo.Duration = span.TotalMilliseconds;

            var logContent = "Duration: " + traceInfo.Duration.ToString() + "ms";
            
            if (!(traceInfo.TraceId is null))
            {
                logContent += " TraceId: " + traceInfo.TraceId;
            }
            if (!(traceInfo.Request is null))
            {
                logContent += ", RequestLength: " + traceInfo.Request.Length.ToString();
            }

            LogLevel logLevel = LogLevel.Information;

            if (traceInfo.Response.StatusCode < 500 && traceInfo.Response.StatusCode >= 400)
                logLevel = LogLevel.Warning;

            if (traceInfo.Response.StatusCode >= 500)
                logLevel = LogLevel.Error;

            switch (logLevel)
            {
                case LogLevel.Information:
                    using (_logger.BeginScope("{@Trace}", traceInfo))
                        _logger.LogInformation(logContent);
                    return;
                case LogLevel.Warning:
                    using (_logger.BeginScope("{@Trace}", traceInfo))
                        _logger.LogWarning(logContent);
                    return;
                case LogLevel.Error:
                    using (_logger.BeginScope("{@Trace}", traceInfo))
                        _logger.LogError(logContent);
                    return;
                default:
                    using (_logger.BeginScope("{@Trace}", traceInfo))
                        _logger.LogCritical(logContent);
                    return;
            }

        }
    }

}

