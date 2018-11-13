using Ninject.Modules;
using RSSref.Models;

namespace RSSref.Util
{
    public class NinjectRegistrations : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository>().To<RSSrepository>().WithPropertyValue("context", new ApplicationDbContext());
        }
    }
}