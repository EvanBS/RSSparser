using PagedList;
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

        MainCollection currentCollection = new MainCollection();

        MainResource currencResource = new MainResource();

        IEnumerable<RSSFeed> RSSFeedData = null;

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
        public async Task<ActionResult> Resource(string CollectionName, string ResourceName, int? page)
        {
            int pageSize = 4;
            int pageNumber = (page ?? 1);

            if (currencResource.ResourceName == ResourceName)
            {

                ViewBag.URL = currencResource.URL;
                ViewBag.RSSName = ResourceName;
                ViewBag.CollectionName = CollectionName;

                return View(RSSFeedData.ToPagedList(pageNumber, pageSize));
            }


            // find collection by name (made for user-friendly url despite the speed)
            currentCollection = await db.MainCollections.Where(c => c.Name == CollectionName).FirstOrDefaultAsync();

            // find resource
            currencResource = await db.MainResources.Where(r => r.ResourceName == ResourceName).FirstOrDefaultAsync();

            WebClient wclient = new WebClient();

            string RSSData = wclient.DownloadString(currencResource.URL);

            XDocument xml = XDocument.Parse(RSSData);

            RSSFeedData = (from x in xml.Descendants("item")
                               let bytesTitle = Encoding.Default.GetBytes(((string)x.Element("title")))
                               let bytesDesc = Encoding.Default.GetBytes(((string)x.Element("description")))

                               select new RSSFeed
                               {
                                   Title = Encoding.UTF8.GetString(bytesTitle),
                                   Link = ((string)x.Element("link")),
                                   Description = Encoding.UTF8.GetString(bytesDesc),
                                   PubDate = ((string)x.Element("pubDate"))
                               });

            
            ViewBag.URL = currencResource.URL;
            ViewBag.RSSName = ResourceName;
            ViewBag.CollectionName = CollectionName;

            return View(RSSFeedData.ToPagedList(pageNumber, pageSize));
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