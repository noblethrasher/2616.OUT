using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;

[assembly: Microsoft.FSharp.Core.AutoOpen("ArcRx")]

namespace ArcRx
{
    public static class Utils
    {
        public static void RegisterModule<AppRoute>()
        {
            HttpApplication.RegisterModule(typeof(AppRoute));
        }
    }

    public abstract partial class AppRoute<Token>
    {
        public abstract class AppException : Exception
        {
            public AppException() : base() { }
            public AppException(string message) : base(message) { }
            public AppException(string message, Exception ex) : base(message, ex) { }
            public AppException(SerializationInfo info, StreamingContext context) : base(info, context) { }

            protected abstract AppState GetState();
            protected abstract Representation GetRepresentation();

            public virtual bool HandleAsHTTP { get; }

            public static implicit operator Representation(AppException ex) => ex.GetRepresentation();
            public static implicit operator AppState(AppException ex) => ex.GetState();
        }
    }

    public abstract partial class AppRoute<Token>        
    {
        public abstract class AppState
        {
            public virtual AppState Consider(Token token) => null;
            protected abstract Representation GetRepresentation(HttpContextEx context);

            public virtual Representation ApplyUknown<T>(T method, HttpContextEx ctx)
            where T : RequestMethod
            {
                return new AdHocRepresentation(c =>
                {
                    c.Response.StatusCode = 405;
                    c.Response.Write($"The method '{typeof(T).Name}' is not allowed.");
                });
            }
        }

        public /*unsealed*/ class MessageNotUnderstood : AppState, Get.Allowed, Post.Allowed
        {
            protected readonly HttpContextEx context;
            protected readonly Token token;


            public MessageNotUnderstood(HttpContextEx context, Token t)
            {
                this.context = context;
                this.token = t;
            }

            public Representation Accept(Get method, HttpContextEx ctx) => GetRepresentation(ctx);
            public Representation Accept(Post method, HttpContextEx ctx) => GetRepresentation(ctx);
            

            public override AppState Consider(Token token) => this;

            protected override Representation GetRepresentation(HttpContextEx context)
            {

                return new AdHocRepresentation(ctx =>
                            {
                                ctx.Response.StatusCode = 404;
                                ctx.Response.Write("Not Found.");
                            });               
            }   
        }
    }

    public abstract class Representation : IHttpHandler
    {
        public virtual bool IsReusable => false;

        public abstract void ProcessRequest(HttpContext context);

        public T Become<T>() where T : class => this as T;

        public virtual Representation GetNegotiatedRepresentation(MediaType media_type) => media_type.Convert(this);

        public static implicit operator Representation (Action<HttpContextEx> process_request) => new AdHocRepresentation(process_request);
    }

    public sealed class AdHocRepresentation : Representation
    {
        readonly Action<HttpContextEx> process_request;

        public AdHocRepresentation(Action<HttpContextEx> process_request)
        {
            this.process_request = process_request;
        }

        public sealed override bool IsReusable => false;

        public override void ProcessRequest(HttpContext context) => process_request(context);
    }
    
    public abstract partial class AppRoute<Token> : IHttpModule        
    {
        protected static readonly List<string> ApplicationMimeTypes = new List<string>();

        HttpApplication app;

        public void Dispose() { }

        protected virtual IEnumerable<KeyValuePair<string, MediaType>> GetRecognizedMimeTypes()
        {

            var app_domain = AppDomain.CurrentDomain;

            var asms = app_domain.GetAssemblies();

            var types = from asm in asms
                        from type in asm.GetTypes()
                        let mt = type.GetCustomAttribute<RecognizedMimeTypeAttribute>()
                        where mt != null
                        select new { type, mt };


            var xs = new List<KeyValuePair<string, MediaType>>();

            foreach (var t in types)
            {
                var media_type = (MediaType)t.type.GetConstructor(Type.EmptyTypes).Invoke(null);

                foreach (var type in t.mt.MimeTypes)
                    yield return new KeyValuePair<string, MediaType>(type, media_type);
            }
        }

        public void Init(HttpApplication app)
        {
            var mime_mapping_cache = new Dictionary<string, MediaType>();

            foreach (var type in GetRecognizedMimeTypes())
                mime_mapping_cache.Add(type.Key, type.Value);

            app.Application.Add("MIME_MAPPING_MEMO", mime_mapping_cache);


            (this.app = app).PostResolveRequestCache += MapRequest;

            
        }

        
        protected abstract AppState GetRoot(HttpContext context);
        protected virtual AppState TranslateNull(HttpContextEx context, AppState appState, Token token) => appState ?? new MessageNotUnderstood(context, token);
        protected abstract MessageAnalysis GetMessageAnalysis(HttpContextEx context);

        protected virtual RequestMethod GetMethod(HttpContextEx ctx)
        {
            return new AbstractStandardMethod(ctx);
        }

        protected virtual void MapRequest(object sender, EventArgs e)
        {
            var context = (HttpContextEx)app.Context;

            foreach (var message in GetMessageAnalysis(context))
            {
                var curr = GetRoot(app.Context);

                foreach (var token in message)
                {
                    try
                    {
                        curr = TranslateNull(context, curr.Consider(token), token);
                    }
                    catch (AppException ex)
                    {
                        if (ex.HandleAsHTTP)
                            curr = ex;
                    }
                }

                if (curr != null)
                {
                    try
                    {
                        var rep = GetMethod(context).Apply(curr, context);

                        var mime_mapping = app.Application.Get("MIME_MAPPING_MEMO") as Dictionary<string, MediaType>;

                        if(mime_mapping !=null)
                        {
                            var requested_types = context.Request.AcceptTypes;

                            var media_type_objects = new List<MediaType>();

                            foreach (var type in requested_types)
                            {
                                MediaType media_type_obj = null;

                                if (mime_mapping.TryGetValue(type, out media_type_obj))
                                {
                                    var alt = rep.GetNegotiatedRepresentation(media_type_obj);

                                    if(alt !=null)
                                    {
                                        rep = alt;
                                        break;
                                    }
                                }
                            }
                        }


                        context.RemapHandler(rep);                        
                    }
                    catch (AppException ex)
                    {
                        if (ex.HandleAsHTTP)
                            context.RemapHandler((Representation)ex);
                    }
                }
            }
        }

        protected abstract class MessageAnalysis            
        {
            public abstract RecognizedMessageEnumeration GetEnumerator();

            public abstract class RecognizedMessageEnumeration
            {
                public abstract bool MoveNext();
                public abstract RecognizedMessage Current { get; }
            }
        }

        protected sealed class EmptyRecognizedMessage : MessageAnalysis.RecognizedMessageEnumeration
        {
            public override RecognizedMessage Current => null;

            public override bool MoveNext() => false;
        }

        public abstract class RecognizedMessage
        {
            public abstract TokenEnumeration GetEnumerator();

            public abstract class TokenEnumeration
            {
                public abstract bool MoveNext();
                public abstract Token Current { get; }
            }
        }
    }

    public sealed class MessageToken
    {
        readonly string[] segments;

        public string Segment => segments[state];
        public HttpContextEx Context { get; }

        int state;

        internal int State
        {
            set
            {
                state = value;
            }
        }

        internal MessageToken(HttpContextEx context, string[] segments)
        {
            Context = context;
            this.segments = segments;
        }

        public override string ToString() => Segment;

        public static bool operator ==(MessageToken t, string s) => t.Segment.Equals(s, StringComparison.OrdinalIgnoreCase);
        public static bool operator !=(MessageToken t, string s) => !t.Segment.Equals(s, StringComparison.OrdinalIgnoreCase);

        public static bool operator ==(MessageToken x, MessageToken y) => !ReferenceEquals(y, null) && x == y.Segment;
        public static bool operator !=(MessageToken x, MessageToken y) => ReferenceEquals(y, null) || x != y.Segment;

        public override int GetHashCode() => Segment.GetHashCode();

        public override bool Equals(object that) => that is MessageToken && this == (that as MessageToken);
    }

    public abstract class UrlAppRoute : AppRoute<MessageToken>
    {
        protected override MessageAnalysis GetMessageAnalysis(HttpContextEx context) => new DefaultMessageAnalysis(context);

        protected sealed class DefaultMessageAnalysis : MessageAnalysis
        {
            readonly HttpContextEx context;

            static readonly RecognizedMessageEnumeration empty_enumeration_of_recognized_messages = new EmptyRecognizedMessage();

            public DefaultMessageAnalysis(HttpContextEx context)
            {
                this.context = context;
            }

            sealed class MessageComposedOfURLSegments : RecognizedMessage
            {
                readonly HttpContextEx context;

                public MessageComposedOfURLSegments(HttpContextEx context)
                {
                    this.context = context;
                }

                public override TokenEnumeration GetEnumerator()
                {
                    return new Enumeration(context);
                }

                sealed class Enumeration : TokenEnumeration
                {
                    static readonly char[] delimiters = new[] { '/' };

                    readonly string[] segments;

                    readonly HttpContextEx context;
                    int state = -1;

                    readonly MessageToken token;

                    public Enumeration(HttpContextEx context)
                    {
                        this.segments = context.Request.Url.AbsolutePath.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                        List<string> requested_mime_types = null;

                        for(var i = 0; i < segments.Length; i++)
                        {
                            if(segments[i].StartsWith("."))
                                (requested_mime_types = requested_mime_types ?? new List<string>()).Add(segments[i]);                            
                        }

                        if(requested_mime_types != null)
                        {
                            context.Items.Add("REQUESTED_MIME_TYPES", requested_mime_types);

                            segments = (from segment in segments where !segment.StartsWith(".") select segment).ToArray();
                        }

                        this.context = context;
                        token = new MessageToken(context, segments);
                    }

                    public override MessageToken Current
                    {
                        get
                        {
                            token.State = state;
                            return token;
                        }
                    }

                    public override bool MoveNext() => ++state < segments.Length;
                }
            }

            sealed class MessageComposedOfURLSegmentsEnumeration : RecognizedMessageEnumeration
            {
                readonly HttpContextEx context;
                byte state;

                public MessageComposedOfURLSegmentsEnumeration(HttpContextEx context)
                {
                    this.context = context;
                }

                public override RecognizedMessage Current => new MessageComposedOfURLSegments(context);

                public override bool MoveNext() => state++ == 0;
            }

            public override MessageAnalysis.RecognizedMessageEnumeration GetEnumerator()
            {
                //Ordinarily, we do not want to mediate request that can be directly "handled" by the file system.

                var segments = context.Request.Url.Segments;
                var last_segment = segments[segments.Length - 1];

                if (last_segment.IndexOf('.') > 0 && last_segment[last_segment.Length - 1] != '.') //looking for strings of the form .+\..+
                {
                    var ext = last_segment.Substring(last_segment.LastIndexOf('.'));

                    if (!ApplicationMimeTypes.Contains(ext) || MimeMapping.GetMimeMapping(ext) != "application/octet-stream")
                        return empty_enumeration_of_recognized_messages;
                }

                return new MessageComposedOfURLSegmentsEnumeration(context);

            }
        }
    }
}
