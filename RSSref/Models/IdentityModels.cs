using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RSSref.Models
{

    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {

            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            return userIdentity;
        }
    }

    public interface IRepository
    {
        ApplicationDbContext context { get; set; }

        void SaveCollection(MainCollection mainCollection);

        void SaveResource(MainResource mainResource);

        Task<List<MainCollection>> GetCollectionsJoinResourcesAsync();

        Task<MainCollection> FindCollectionByNameAsync(string CollectionName);

        Task<MainResource> FindResourceByNameAsync(string ResourceName);

        void SaveChanges();

        IEnumerable<MainCollection> CollectionList();

        IEnumerable<MainResource> ResourceList();

        MainCollection GetCollection(int id);

        MainResource GetResource(int id);
    }


    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public virtual DbSet<MainCollection> MainCollections { get; set; }

        public virtual DbSet<MainResource> MainResources { get; set; }


        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    public class RSSrepository : IDisposable, IRepository
    {
        public ApplicationDbContext db = new ApplicationDbContext();

        public ApplicationDbContext context
        {
            get { return db; }
            set { db = value; }
        }

        public async Task<List<MainCollection>> GetCollectionsJoinResourcesAsync()
        {
            return await context.MainCollections.Include(c => c.MainResources).ToListAsync();
        }

        public IEnumerable<MainCollection> CollectionList()
        {
            return db.MainCollections.ToList();
        }

        public IEnumerable<MainResource> ResourceList()
        {
            return db.MainResources.ToList();
        }

        public MainCollection GetCollection(int id)
        {
            return db.MainCollections.Find(id);
        }

        public MainResource GetResource(int id)
        {
            return db.MainResources.Find(id);
        }

        public void SaveCollection(MainCollection mainCollection)
        {
            db.MainCollections.AddIfNotExists(mainCollection);
        }

        public void SaveResource(MainResource mainResource)
        {
            db.MainResources.AddIfNotExists(mainResource);
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        public async Task<MainCollection> FindCollectionByNameAsync(string CollectionName)
        {
            return await context.MainCollections.Where(c => c.Name == CollectionName).FirstOrDefaultAsync();
        }

        public async Task<MainResource> FindResourceByNameAsync(string ResourceName)
        {
            return await context.MainResources.Where(r => r.ResourceName == ResourceName).FirstOrDefaultAsync();
        }


        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}