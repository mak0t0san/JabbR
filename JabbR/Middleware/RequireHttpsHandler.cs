﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Owin;

namespace JabbR.Middleware
{ 
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RequireHttpsHandler
    {
        private readonly AppFunc _next;

        public RequireHttpsHandler(AppFunc next)
        {
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var request = new ServerRequest(env);
            var response = new Gate.Response(env);

            string scheme = GetScheme(request);

            if (!scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                var builder = new UriBuilder(request.Url);
                builder.Scheme = "https";

                if (request.Url.IsDefaultPort)
                {
                    builder.Port = -1;
                }

                response.SetHeader("Location", builder.ToString());
                response.StatusCode = 302;

                return TaskAsyncHelper.Empty;
            }
            else
            {
                return _next(env);
            }
        }

        private string GetScheme(ServerRequest request)
        {
            var scheme = request.Headers["X-Forwarded-Proto"];
            if (String.IsNullOrEmpty(scheme))
            {
                return request.Url.Scheme;
            }

            return scheme;
        }
    }
}