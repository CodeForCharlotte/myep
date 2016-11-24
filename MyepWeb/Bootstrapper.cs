/*
Getting started with Unity.Mvc4
-------------------------------

To get started, just add a call to Bootstrapper.Initialize() in the Application_Start method of Global.asax.cs
and the MVC framework will then use the Unity.Mvc4 DependencyResolver to resolve your components.

There is code in the bootstrapper to initialise the Unity container. Any components that you need to inject should be
registered in the BuildUnityContainer method of the Bootstrapper. All components that implement IDisposable should be
registered with the HierarchicalLifetimeManager to ensure that theey are properly disposed at the end of the request.

It is typically not necessary to register your controllers with Unity.

The readme has been copyied from Unity.MVC3 as this is a package just to support MVC4, the unity portion is identical.
You can find out more about Unity.Mvc3 by visiting:

http://devtrends.co.uk/blog/introducing-the-unity.mvc3-nuget-package-to-reconcile-mvc3-unity-and-idisposable
*/
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using Unity.Mvc4;

namespace Site
{
    public static class Bootstrapper
    {
        public static void Initialize()
        {
            ConfigureContainer(new UnityContainer());
            ConfigureRoutes(RouteTable.Routes);
            ConfigureGlobalFilters(GlobalFilters.Filters);
            ModelMetadataProviders.Current = new SiteModelMetadataProvider();
            ModelBinders.Binders.DefaultBinder = new SiteModelBinder();
        }

        public static void ConfigureContainer(IUnityContainer container)
        {
            container.RegisterType<SiteDb>(new HierarchicalLifetimeManager());

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        public static void ConfigureRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        public static void ConfigureGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    };
}
