using PagedList;
using RSSref.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;

namespace RSSref.Controllers
{
    public class HomeController : Controller
    {
        IRepository repository;

        MainCollection currentCollection = new MainCollection();

        MainResource currencResource = new MainResource();

        IEnumerable<RSSFeed> RSSFeedData = null;

        public HomeController(IRepository repository)
        {
            this.repository = repository;
        }

        [Authorize]
        public async Task<ActionResult> Index()
        {
            List<MainCollection> collections = await repository.GetCollectionsJoinResourcesAsync();
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
                repository.SaveCollection(collection);
                repository.SaveChanges();
            }


            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult> Resource(string id, string ResourceName, int? page)
        {
            int pageSize = 4;
            int pageNumber = (page ?? 1);

            if (currencResource.ResourceName == ResourceName)
            {

                ViewBag.URL = currencResource.URL;
                ViewBag.RSSName = ResourceName;
                ViewBag.CollectionName = id;

                return View(RSSFeedData.ToPagedList(pageNumber, pageSize));
            }


            // find collection by name (made for user-friendly url despite the speed)
            currentCollection = await repository.FindCollectionByNameAsync(ViewBag.CollectionName);

            // find resource
            currencResource = await repository.FindResourceByNameAsync(ResourceName);

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
            ViewBag.CollectionName = ViewBag.CollectionName;

            return View(RSSFeedData.ToPagedList(pageNumber, pageSize));
        }


        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult> AddResource()
        {
            ViewBag.collections = await repository.GetCollectionsJoinResourcesAsync();

            return View();
        }


        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult AddResource(MainResource mainResource)
        {
            repository.SaveResource(mainResource);
            repository.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}