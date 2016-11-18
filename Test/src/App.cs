using ArcRx;
using System.Web;
using System;

[assembly: PreApplicationStartMethod(typeof(App.App), "Start")]

namespace App
{
    public sealed class App : ArcRx.UrlAppRoute
    {
        public static void Start() => Utils.RegisterModule<App>();

        protected override AppState GetRoot(HttpContext context) => new ArcApp.Foo.Root();
    }
}