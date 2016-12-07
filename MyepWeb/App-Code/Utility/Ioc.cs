using System.Web.Mvc;

namespace Site
{
    public static class Ioc
    {
        public static T Get<T>()
        {
            return DependencyResolver.Current.GetService<T>();
        }
    };
}
