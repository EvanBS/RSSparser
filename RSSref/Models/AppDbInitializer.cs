using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace RSSref.Models
{
    public static class DbSetExtensions
    {
        public static T AddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {
            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
            return !exists ? dbSet.Add(entity) : null;
        }
    }

    public class AppDbInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var role1 = new IdentityRole();

            var role2 = new IdentityRole();

            if (!roleManager.RoleExists("admin"))
            {
                role1 = new IdentityRole { Name = "admin" };

                roleManager.Create(role1);
            }

            if (!roleManager.RoleExists("user"))
            {
                role2 = new IdentityRole { Name = "user" };

                roleManager.Create(role2);
            }

            if (userManager.FindByEmail("admin@mail.ru") == null)
            {
                var admin = new ApplicationUser { Email = "admin@mail.ru", UserName = "admin@mail.ru" };
                string password = "ad46D_ewr3csTjb1232322";
                var result = userManager.Create(admin, password);

                if (result.Succeeded)
                {
                    userManager.AddToRole(admin.Id, role1.Name);
                    userManager.AddToRole(admin.Id, role2.Name);
                }
            }

            var testCol = new MainCollection();

            // Set default collections and resources
            List<MainCollection> mainCollectionsDefault = new List<MainCollection>()
            {
                new MainCollection { Name = "Tech" },
                new MainCollection { Name = "News" },
                new MainCollection { Name = "Games"}
            };
            context.MainCollections.AddRange(mainCollectionsDefault);
            context.SaveChanges();

            List<MainResource> mainResourcesDefault = new List<MainResource>()
            {
                new MainResource{ ResourceName = "SpaceX", URL = "https://www.space.com/home/feed/site.xml", MainCollection_Id = mainCollectionsDefault[0].Id },
                new MainResource{ ResourceName = "Lentach", URL = "https://lenta.ru/rss/news", MainCollection_Id = mainCollectionsDefault[1].Id },
                new MainResource{ ResourceName = "RT", URL = "https://www.rt.com/rss/", MainCollection_Id = mainCollectionsDefault[1].Id },
                new MainResource{ ResourceName = "DTF", URL = "https://dtf.ru/rss/all", MainCollection_Id = mainCollectionsDefault[2].Id },
            };
            context.MainResources.AddRange(mainResourcesDefault);

            context.SaveChanges();
            base.Seed(context);
        }
    }
}