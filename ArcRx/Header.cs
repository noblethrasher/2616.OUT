using System;
using System.Web;

namespace ArcRx
{
    public abstract class ResponseHeader
    {
        public abstract void Apply(HttpResponseBase response);
    }

    public sealed class Redirect : ResponseHeader
    {
        public int Code { get; }
        public string Location { get; }

        public Redirect(string location, int code)
        {
            this.Code = code;
            this.Location = location;
        }

        public override void Apply(HttpResponseBase response)
        {
            response.AppendHeader("Location", Location);
            response.StatusCode = Code;
        }
    }
}