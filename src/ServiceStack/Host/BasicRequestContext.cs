﻿using System.Collections.Generic;
using System.Globalization;
using System.Net;
using ServiceStack.Configuration;
using ServiceStack.Messaging;
using ServiceStack.Text;
using ServiceStack.Web;

namespace ServiceStack.Host
{
    public class BasicRequestContext : IRequestContext
    {
        public IMessage Message { get; set; }
        public BasicRequest Request { get; set; }
        public BasicResponse Response { get; set; }

        private IResolver resolver;
        public IResolver Resolver
        {
            get { return resolver ?? Service.GlobalResolver; }
            set { resolver = value; }
        }

        public BasicRequestContext(IMessage message=null)
        {
            Message = message ?? new Message();
            ContentType = this.ResponseContentType = MimeTypes.Json;
            if (Message.Body != null)
                PathInfo = "/json/oneway/" + OperationName;
            
            Request = new BasicRequest(this);
            Response = new BasicResponse(this);
        }

        private string operationName;
        public string OperationName
        {
            get { return operationName ?? (operationName = Message.Body != null ? Message.Body.GetType().Name : null); }
            set { operationName = value; }
        }

        public T Get<T>() where T : class
        {
            if (typeof(T) == typeof(IHttpRequest))
                return (T)(object)Request;

            if (typeof(T) == typeof(IHttpResponse))
                return (T)(object)Response;

            return Resolver.TryResolve<T>();
        }

        public string IpAddress { get; set; }

        public string GetHeader(string headerName)
        {
            string headerValue;
            Headers.TryGetValue(headerName, out headerValue);
            return headerValue;
        }

        private Dictionary<string, string> headers;
        public Dictionary<string, string> Headers
        {
            get
            {
                if (headers != null)
                {
                    headers = Message.ToHeaders();
                }
                return headers;
            }
        }

        public IDictionary<string, Cookie> Cookies
        {
            get { return new Dictionary<string, Cookie>(); }
        }

        public RequestAttributes RequestAttributes
        {
            get { return RequestAttributes.LocalSubnet | RequestAttributes.MessageQueue; }
        }

        public Web.IRequestPreferences RequestPreferences { get; set; }

        public string ContentType { get; set; }

        public string ResponseContentType { get; set; }

        public string CompressionType { get; set; }

        public string AbsoluteUri { get; set; }

        public string PathInfo { get; set; }

        public IHttpFile[] Files { get; set; }

        public void Dispose()
        {
        }
    }


    public static class MqExtensions
    {
        public static Dictionary<string,string> ToHeaders(this IMessage message)
        {
            var map = new Dictionary<string, string>
            {
                {"CreatedDate",message.CreatedDate.ToLongDateString()},
                {"Priority",message.Priority.ToString(CultureInfo.InvariantCulture)},
                {"RetryAttempts",message.RetryAttempts.ToString(CultureInfo.InvariantCulture)},
                {"ReplyId",message.ReplyId.HasValue ? message.ReplyId.Value.ToString() : null},
                {"ReplyTo",message.ReplyTo},
                {"Options",message.Options.ToString(CultureInfo.InvariantCulture)},
                {"Error",message.Error.Dump()},
            };
            return map;
        }
    }
}