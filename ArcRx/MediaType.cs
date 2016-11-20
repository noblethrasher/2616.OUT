using System;
using System.Web;

namespace ArcRx
{
    public sealed class ExplicitlyQualifiedMediaType<T> : MediaType
        where T : MediaType
    {
        readonly T media_type;

        public override decimal RelativeQualitativeFactor { get; }

        public ExplicitlyQualifiedMediaType(T media_type, decimal factor)
        {
            this.media_type = media_type;

            RelativeQualitativeFactor = factor;
        }

        public T GetUnderlyingMediaType => media_type;

        public override Representation Convert(Representation rep) => media_type.Convert(rep);
    }

    public abstract class MediaType
    {
        public abstract decimal RelativeQualitativeFactor { get; }

        public abstract class Representation<T> : Representation
            where T : MediaType { }

        public abstract Representation Convert(Representation rep);            
    }

    public abstract class MediaType<T> : MediaType
        where T : MediaType<T>
    {
        public override decimal RelativeQualitativeFactor => 1.0m;  

        public abstract class Representation : MediaType.Representation<T>,
            Compatible
        {
            public Representation Convert(T type) => this;

            protected abstract string ContentType { get; }

            public sealed override void ProcessRequest(HttpContext context)
            {
                context.Response.ContentType = ContentType;
                _ProcessRequest(context);
            }

            protected abstract void _ProcessRequest(HttpContext context);
        }

        public override ArcRx.Representation Convert(ArcRx.Representation rep)
        {
            return _Convert(rep);
        }

        protected abstract Representation _Convert(ArcRx.Representation rep);        

        public interface Compatible
        {
            Representation Convert(T type);
        }
    }

    public sealed class PlainText : MediaType<PlainText>
    {
        public new abstract class Representation : MediaType<PlainText>.Representation
        {
            protected override string ContentType => "text/plain";
        }

        protected override MediaType<PlainText>.Representation _Convert(ArcRx.Representation rep)
        {
            return (rep as PlainText.Compatible)?.Convert(this) as PlainText.Representation;
        }
    }

    public sealed class Html : MediaType<Html>
    {
        public new abstract class Representation : MediaType<Html>.Representation
        {
            protected override string ContentType => "text/html";
        }

        protected override MediaType<Html>.Representation _Convert(ArcRx.Representation rep)
        {
            return (rep as Html.Compatible)?.Convert(this) as Html.Representation;
        }
    }

    public sealed class XHtml : MediaType<XHtml>
    {
        public new abstract class Representation : MediaType<XHtml>.Representation
        {
            protected override string ContentType => "application/xhtml+xml";
        }

        protected override MediaType<XHtml>.Representation _Convert(ArcRx.Representation rep)
        {
            return (rep as XHtml.Compatible)?.Convert(this) as XHtml.Representation;
        }
    }
}