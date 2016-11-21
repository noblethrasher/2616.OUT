using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Instrumentation;
using System.Web.Profile;
using System.Web.Routing;
using System.Web.SessionState;
using System.Web.WebSockets;

namespace ArcRx
{
    public abstract partial class HttpContextEx : HttpContextBase
    {
        sealed class HttpRequest : HttpRequestBase
        {
            readonly HttpContextBase context;
            readonly HttpRequestBase request;

            public HttpRequest(HttpContextBase context, HttpRequestBase request)
            {
                this.request = request;
                this.context = context;
            }

            public override void Abort() => request.Abort();

            public override string[] AcceptTypes
            {
                get
                {
                    IReadOnlyList<string> custom_request;

                    var all_requested_types = new List<string>();

                    var enumerator = (custom_request = context.Items["REQUESTED_MIME_TYPES"] as IReadOnlyList<string>)?.GetEnumerator();

                    try
                    {
                        while (enumerator?.MoveNext() == true)
                            all_requested_types.Add(enumerator?.Current);

                        foreach (var type in request.AcceptTypes)
                            all_requested_types.Add(type);

                        return all_requested_types.ToArray();
                    }
                    finally
                    {
                        enumerator?.Dispose();
                    }
                }
            }

            public override string AnonymousID => request.AnonymousID;
            public override string ApplicationPath => request.ApplicationPath;
            public override string AppRelativeCurrentExecutionFilePath => request.AppRelativeCurrentExecutionFilePath;

            public override byte[] BinaryRead(int count) => request.BinaryRead(count);

            public override HttpBrowserCapabilitiesBase Browser => request.Browser;

            public override HttpClientCertificate ClientCertificate => request.ClientCertificate;

            public override Encoding ContentEncoding
            {
                get
                {
                    return request.ContentEncoding;
                }

                set
                {
                    request.ContentEncoding = value;
                }
            }

            public override int ContentLength => request.ContentLength;

            public override string ContentType
            {
                get
                {
                    return request.ContentType;
                }

                set
                {
                    request.ContentType = value;
                }
            }

            public override HttpCookieCollection Cookies => request.Cookies;

            public override string CurrentExecutionFilePath => request.CurrentExecutionFilePath;

            public override string CurrentExecutionFilePathExtension => request.CurrentExecutionFilePathExtension;

            public override bool Equals(object obj) => request.Equals(obj);

            public override string FilePath => request.FilePath;

            public override HttpFileCollectionBase Files => request.Files;

            public override Stream Filter
            {
                get
                {
                    return request.Filter;
                }

                set
                {
                    request.Filter = value;
                }
            }

            public override NameValueCollection Form => request.Form;

            public override Stream GetBufferedInputStream() => request.GetBufferedInputStream();

            public override Stream GetBufferlessInputStream() => request.GetBufferlessInputStream();

            public override Stream GetBufferlessInputStream(bool disableMaxRequestLength) => request.GetBufferlessInputStream(disableMaxRequestLength);

            public override int GetHashCode() => request.GetHashCode();

            public override NameValueCollection Headers => request.Headers;

            public override ChannelBinding HttpChannelBinding => request.HttpChannelBinding;

            public override string HttpMethod => request.HttpMethod;

            public override Stream InputStream => request.InputStream;

            public override void InsertEntityBody() => request.InsertEntityBody();

            public override void InsertEntityBody(byte[] buffer, int offset, int count) => InsertEntityBody(buffer, offset, count);

            public override bool IsAuthenticated => request.IsAuthenticated;

            public override bool IsLocal => request.IsLocal;

            public override bool IsSecureConnection => request.IsSecureConnection;

            public override WindowsIdentity LogonUserIdentity => request.LogonUserIdentity;

            public override int[] MapImageCoordinates(string imageFieldName) => request.MapImageCoordinates(imageFieldName);

            public override string MapPath(string virtualPath) => request.MapPath(virtualPath);

            public override string MapPath(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping) => request.MapPath(virtualPath, baseVirtualDir, allowCrossAppMapping);

            public override double[] MapRawImageCoordinates(string imageFieldName) => request.MapRawImageCoordinates(imageFieldName);

            public override NameValueCollection Params => request.Params;

            public override string Path => request.Path;

            public override string PathInfo => request.PathInfo;

            public override string PhysicalApplicationPath => request.PhysicalApplicationPath;

            public override string PhysicalPath => request.PhysicalPath;

            public override NameValueCollection QueryString => request.QueryString;

            public override string RawUrl => request.RawUrl;

            public override ReadEntityBodyMode ReadEntityBodyMode => request.ReadEntityBodyMode;

            public override RequestContext RequestContext
            {
                get
                {
                    return request.RequestContext;
                }

                set
                {
                    request.RequestContext = value;
                }
            }

            public override string RequestType
            {
                get
                {
                    return request.RequestType;
                }

                set
                {
                    request.RequestType = value;
                }
            }

            public override void SaveAs(string filename, bool includeHeaders) => request.SaveAs(filename, includeHeaders);

            public override NameValueCollection ServerVariables => request.ServerVariables;

            public override string this[string key] => base[key];

            public override CancellationToken TimedOutToken => request.TimedOutToken;

            public override ITlsTokenBindingInfo TlsTokenBindingInfo => request.TlsTokenBindingInfo;

            public override string ToString() => request.ToString();

            public override int TotalBytes => request.TotalBytes;

            public override UnvalidatedRequestValuesBase Unvalidated => request.Unvalidated;

            public override Uri Url => request.Url;

            public override Uri UrlReferrer => request.UrlReferrer;

            public override string UserAgent => request.UserAgent;

            public override string UserHostAddress => request.UserHostAddress;

            public override string UserHostName => request.UserHostName;

            public override string[] UserLanguages => request.UserLanguages;

            public override void ValidateInput() => request.ValidateInput();
        }
    }

    public abstract partial class HttpContextEx : HttpContextBase
    {
        readonly HttpContextBase context;

        public HttpContextEx(HttpContextBase ctx)
        {
            context = ctx;
        }

        public HttpContextEx(HttpContext ctx) : this(new HttpContextWrapper(ctx)) { }

        public static implicit operator HttpContextEx(HttpContext ctx) => new DefaultImpl(new HttpContextWrapper(ctx));

        sealed class DefaultImpl : HttpContextEx
        {
            public DefaultImpl(HttpContextBase ctx) : base(ctx) { }
        }

        public virtual HttpContextBase Unwrap => this;
    }

    public abstract partial class HttpContextEx : HttpContextBase
    {
        public override void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc)
        {
            context.AcceptWebSocketRequest(userFunc);
        }

        public override void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc, AspNetWebSocketOptions options)
        {
            context.AcceptWebSocketRequest(userFunc, options);
        }

        public override void AddError(Exception errorInfo) => context.AddError(errorInfo);

        public override ISubscriptionToken AddOnRequestCompleted(Action<HttpContextBase> callback)
        {
            return context.AddOnRequestCompleted(callback);
        }

        public override Exception[] AllErrors => context.AllErrors;

        public override bool AllowAsyncDuringSyncStages
        {
            get
            {
                return context.AllowAsyncDuringSyncStages;
            }

            set
            {
                context.AllowAsyncDuringSyncStages = value;
            }
        }

        public override HttpApplicationStateBase Application => context.Application;

        public override HttpApplication ApplicationInstance
        {
            get
            {
                return context.ApplicationInstance;
            }

            set
            {
                context.ApplicationInstance = value;
            }
        }

        public override AsyncPreloadModeFlags AsyncPreloadMode
        {
            get
            {
                return context.AsyncPreloadMode;
            }

            set
            {
                context.AsyncPreloadMode = value;
            }
        }

        public override Cache Cache => context.Cache;

        public override void ClearError() => context.ClearError();

        public override IHttpHandler CurrentHandler => context.CurrentHandler;

        public override RequestNotification CurrentNotification => context.CurrentNotification;

        public override ISubscriptionToken DisposeOnPipelineCompleted(IDisposable target) => context.DisposeOnPipelineCompleted(target);

        public override bool Equals(object obj)
        {
            return context.Equals(obj);
        }

        public override Exception Error => context.Error;

        public override object GetGlobalResourceObject(string classKey, string resourceKey) => context.GetGlobalResourceObject(classKey, resourceKey);

        public override object GetGlobalResourceObject(string classKey, string resourceKey, CultureInfo culture)
        {
            return context.GetGlobalResourceObject(classKey, resourceKey, culture);
        }

        public override int GetHashCode()
        {
            return context.GetHashCode();
        }

        public override object GetLocalResourceObject(string virtualPath, string resourceKey)
        {
            return context.GetLocalResourceObject(virtualPath, resourceKey);
        }

        public override object GetLocalResourceObject(string virtualPath, string resourceKey, CultureInfo culture)
        {
            return context.GetLocalResourceObject(virtualPath, resourceKey, culture);
        }

        public override object GetSection(string sectionName)
        {
            return context.GetSection(sectionName);
        }

        public override object GetService(Type serviceType)
        {
            return context.GetService(serviceType);
        }

        public override IHttpHandler Handler
        {
            get
            {
                return context.Handler;
            }

            set
            {
                context.Handler = value;
            }
        }

        public override bool IsCustomErrorEnabled => context.IsCustomErrorEnabled;

        public override bool IsDebuggingEnabled => context.IsCustomErrorEnabled;

        public override bool IsPostNotification => context.IsPostNotification;

        public override bool IsWebSocketRequest => context.IsWebSocketRequest;

        public override bool IsWebSocketRequestUpgrading => context.IsWebSocketRequestUpgrading;

        public override IDictionary Items => context.Items;

        public override PageInstrumentationService PageInstrumentation => context.PageInstrumentation;

        public override IHttpHandler PreviousHandler => context.PreviousHandler;

        public override ProfileBase Profile => context.Profile;

        public override void RemapHandler(IHttpHandler handler) => context.RemapHandler(handler);

        public override HttpRequestBase Request => new HttpRequest(this.context, this.context.Request);

        public override HttpResponseBase Response => context.Response;


        public override void RewritePath(string filePath, string pathInfo, string queryString) => context.RewritePath(filePath, pathInfo, queryString);

        public override void RewritePath(string filePath, string pathInfo, string queryString, bool setClientFilePath) => context.RewritePath(filePath, pathInfo, queryString, setClientFilePath);

        public override void RewritePath(string path) => context.RewritePath(path);

        public override void RewritePath(string path, bool rebaseClientPath) => context.RewritePath(path, rebaseClientPath);

        public override HttpServerUtilityBase Server => context.Server;

        public override HttpSessionStateBase Session => context.Session;

        public override void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior) => context.SetSessionStateBehavior(sessionStateBehavior);

        public override bool SkipAuthorization
        {
            get
            {
                return context.SkipAuthorization;
            }

            set
            {
                context.SkipAuthorization = value;
            }
        }

        public override bool ThreadAbortOnTimeout
        {
            get
            {
                return context.ThreadAbortOnTimeout;
            }

            set
            {
                context.ThreadAbortOnTimeout = value;
            }
        }

        public override DateTime Timestamp => context.Timestamp;

        public override string ToString() => context.ToString();

        public override TraceContext Trace => context.Trace;

        public override IPrincipal User
        {
            get
            {
                return context.User;
            }

            set
            {
                context.User = value;
            }
        }

        public override string WebSocketNegotiatedProtocol =>  context.WebSocketNegotiatedProtocol;

        public override IList<string> WebSocketRequestedProtocols => context.WebSocketRequestedProtocols;
    }
}