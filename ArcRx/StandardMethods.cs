using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace ArcRx
{
    public abstract class RequestMethod
    {
        internal static Dictionary<string, RequestMethod> memo =
            new Dictionary<string, RequestMethod>
            {
                ["GET"] = Get.Instance,
                ["PUT"] = Put.Instance,
                ["HEAD"] = Head.Instance,
                ["POST"] = Post.Instance,
                ["TRACE"] = Trace.Instance,
                ["DELETE"] = Delete.Instance,
                ["CONNECT"] = Connect.Instance,
            };

        public virtual Representation Apply<T>(AppRoute<T>.AppState exp, HttpContext ctx)
        {
            return exp.ApplyUknown(this, ctx);
        }
    }

    public abstract class StandardMethod<T> : RequestMethod
        where T : StandardMethod<T>, new()
    {
        public static T Instance = new T();        
    }

    public struct AbstractStandardMethod
    {
        readonly RequestMethod method;

        public AbstractStandardMethod(HttpContext ctx)
        {
            RequestMethod.memo.TryGetValue(ctx.Request.HttpMethod, out method);
        }

        public Representation Apply<T>(AppRoute<T>.AppState exp, HttpContext ctx) => method.Apply(exp, ctx);

        public static implicit operator RequestMethod (AbstractStandardMethod some) => some.method;
    }

    public sealed class Get : StandardMethod<Get>
    {
        public interface Allowed { Representation Accept(Get method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContext ctx) => (exp as Allowed)?.Accept(this, ctx) ?? exp.ApplyUknown(this, ctx);
    }

    public sealed class Head : StandardMethod<Head>
    {
        public interface Allowed { Representation Accept(Head method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContext ctx) => (exp as Allowed)?.Accept(this, ctx) ?? exp.ApplyUknown(this, ctx);
    }

    public sealed class Delete : StandardMethod<Delete>
    {
        public interface Allowed { Representation Accept(Delete method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContext ctx) => (exp as Allowed)?.Accept(this, ctx) ?? exp.ApplyUknown(this, ctx);
    }

    public sealed class Connect : StandardMethod<Connect>
    {
        public interface Allowed { Representation Accept(Connect method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContext ctx) => (exp as Allowed)?.Accept(this, ctx) ?? exp.ApplyUknown(this, ctx);
    }

    public sealed class Post : StandardMethod<Post>
    {
        sealed class AntiForgeryGuarded<T> : HttpContextEx
        {
            public bool FooBar => true;

            public AntiForgeryGuarded(HttpContextEx ctx, AppRoute<T>.AppState app_state) : base(ctx) { }

            sealed class GuardedRequest :HttpRequestBase
            {
                readonly GuardedForm guarded_form;
                readonly GuardedForm guarded_querystring;

                public GuardedRequest(HttpContextEx ctx)
                {
                    guarded_form = new GuardedForm(ctx.Request.Form);
                    guarded_querystring = new GuardedForm(ctx.Request.QueryString);
                }

                sealed class GuardedForm : NameValueCollection
                {
                    string antiforgery;

                    public GuardedForm(NameValueCollection xs) : base(xs) { }
                    
                    public override string Get(int index)
                    {
                        if (antiforgery == null)
                        {
                            antiforgery = base["ANTIFORGERY"];

                            if (string.IsNullOrWhiteSpace(antiforgery))
                                throw new Exception("Missing Anti Forgery Token");

                            try
                            {
                                antiforgery = Encoding.ASCII.GetString(MachineKey.Unprotect(Encoding.ASCII.GetBytes(antiforgery), "CSRF Mitigation"));
                               
                                //TODO: check anti_forgery for validity.
                            }

                            #pragma warning disable
                            catch (Exception ex)
                            {
                                throw;
                            }

                            #pragma warning restore
                        }

                        return Get(index);
                    }

                    public override string Get(string name)
                    {
                        for (var i = 0; i < this.AllKeys.Length; i++)
                            if (this.AllKeys[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                                return Get(i);

                        throw new IndexOutOfRangeException($"The key {name} is not present.");
                    }
                }

                public override NameValueCollection Form => guarded_form;
                public override NameValueCollection QueryString => guarded_querystring;
            }
        }

        public interface Allowed { Representation Accept(Post method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContext ctx) => (exp as Allowed)?.Accept(this, new AntiForgeryGuarded<T>(ctx, exp)) ?? exp.ApplyUknown(this, ctx);
    }

    public sealed class Trace : StandardMethod<Trace>
    {
        public interface Allowed { Representation Accept(Trace method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContext ctx) => (exp as Allowed)?.Accept(this, ctx) ?? exp.ApplyUknown(this, ctx);
    }

    public sealed class Put : StandardMethod<Put>
    {
        public interface Allowed { Representation Accept(Put method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContext ctx) => (exp as Allowed)?.Accept(this, ctx) ?? exp.ApplyUknown(this, ctx);
    }
}
