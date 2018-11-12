using RSSref.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace RSSref.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        [Authorize]
        public async Task<ActionResult> Index()
        {

            List<MainCollection> collections = await db.MainCollections.Include(c => c.MainResources).ToListAsync();
            return View(collections);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public ActionResult AddCollection()
        {
            return View();
        }


        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult AddCollection(MainCollection collection)
        {
            if (collection != null)
            {
                db.MainCollections.Add(collection);
                db.SaveChanges();
            }


            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult> Resource(string CollectionName, string ResourceName)
        {
            // find collection by name (made for user-friendly url despite the speed)
            MainCollection mainCollection = await db.MainCollections.Where(c => c.Name == CollectionName).FirstOrDefaultAsync();

            // find resource
            MainResource mainResource = await db.MainResources.Where(r => r.ResourceName == ResourceName).FirstOrDefaultAsync();
            
            if (mainResource == null) return RedirectToAction("Index");

            WebClient wclient = new WebClient();

            string RSSData = wclient.DownloadString(mainResource.URL);

            XDocument xml = XDocument.Parse(RSSData);

            var RSSFeedData = (from x in xml.Descendants("item")
                               let bytesTitle = Encoding.Default.GetBytes(((string)x.Element("title")))
                               let bytesDesc = Encoding.Default.GetBytes(((string)x.Element("description")))

                               select new RSSFeed
                               {
                                   Title = Encoding.UTF8.GetString(bytesTitle),
                                   Link = ((string)x.Element("link")),
                                   Description = Encoding.UTF8.GetString(bytesDesc),
                                   PubDate = ((string)x.Element("pubDate"))
                               });
            ViewBag.RSSFeed = RSSFeedData;
            ViewBag.URL = mainResource.URL;
            ViewBag.RSSName = ResourceName;
            return View();
        }


        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult> AddResource()
        {
            ViewBag.collections = await db.MainCollections.Include(c => c.MainResources).ToListAsync();

            return View();
        }


        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult AddResource(MainResource mainResource)
        {
            db.MainResources.Add(mainResource);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}