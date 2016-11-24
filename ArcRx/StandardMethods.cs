//Author:Rodrick Chapman
//rodrick.chapman@okstate.edu | rodrick@rodlogic.com


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

        public virtual Representation Apply<T>(AppRoute<T>.AppState exp, HttpContextEx ctx)
        {
            return exp.ApplyUknown(this, ctx);
        }
    }

    public abstract class StandardMethod<T> : RequestMethod
        where T : StandardMethod<T>, new()
    {
        public static T Instance = new T();

        public static Func<T> Production = () => new T();      
    }

    public struct AbstractStandardMethod
    {
        readonly RequestMethod method;

        public AbstractStandardMethod(HttpContextEx ctx)
        {
            RequestMethod.memo.TryGetValue(ctx.Request.HttpMethod, out method);
        }

        public Representation Apply<T>(AppRoute<T>.AppState exp, HttpContext ctx) => method.Apply(exp, ctx);

        public static implicit operator RequestMethod (AbstractStandardMethod some) => some.method;
    }

    /// <summary>
    /// Represents the standard HTTP GET method. Applications should create only a single instance.
    /// </summary>
    public sealed class Get : StandardMethod<Get>
    {
        public interface Allowed { Representation Accept(Get method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContextEx ctx) => (exp as Allowed)?.Accept(this, ctx) ?? exp.ApplyUknown(this, ctx);
    }

    /// <summary>
    /// Represents the standard HTTP HEAD method. Applications should create only a single instance.
    /// </summary>
    public sealed class Head : StandardMethod<Head>
    {
        public interface Allowed { Representation Accept(Head method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContextEx ctx) => (exp as Allowed)?.Accept(this, ctx) ?? exp.ApplyUknown(this, ctx);
    }

    /// <summary>
    /// Represents the standard HTTP DELETE method. Applications should create only a single instance.
    /// </summary>
    public sealed class Delete : StandardMethod<Delete>
    {
        public interface Allowed { Representation Accept(Delete method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContextEx ctx) => (exp as Allowed)?.Accept(this, ctx) ?? exp.ApplyUknown(this, ctx);
    }

    /// <summary>
    /// Represents the standard HTTP CONNECT method. Applications should create only a single instance.
    /// </summary>
    public sealed class Connect : StandardMethod<Connect>
    {
        public interface Allowed { Representation Accept(Connect method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContextEx ctx) => (exp as Allowed)?.Accept(this, ctx) ?? exp.ApplyUknown(this, ctx);
    }

    /// <summary>
    /// Represents the standard HTTP POST method. This method guards against cross site request forgery. Applications should create only a single instance.
    /// </summary>
    public sealed class Post : StandardMethod<Post>
    {
        public interface Allowed { Representation Accept(Post method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContextEx ctx) => (exp as Allowed)?.Accept(this, new AntiForgeryGuarded<T>(ctx, exp)) ?? exp.ApplyUknown(this, ctx);
    }

    /// <summary>
    /// Represents the standard HTTP TRACE method. Applications should create only a single instance.
    /// </summary>
    public sealed class Trace : StandardMethod<Trace>
    {
        public interface Allowed { Representation Accept(Trace method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContextEx ctx) => (exp as Allowed)?.Accept(this, ctx) ?? exp.ApplyUknown(this, ctx);
    }

    /// <summary>
    /// Represents the standard HTTP PUT method. Applications should create only a single instance.
    /// </summary>
    public sealed class Put : StandardMethod<Put>
    {
        public interface Allowed { Representation Accept(Put method, HttpContextEx ctx); }
        public override Representation Apply<T>(AppRoute<T>.AppState exp, HttpContextEx ctx) => (exp as Allowed)?.Accept(this, new AntiForgeryGuarded<T>(ctx, exp)) ?? exp.ApplyUknown(this, ctx);
    }
}
