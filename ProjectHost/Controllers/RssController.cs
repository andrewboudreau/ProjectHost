using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using ProjectHost.Models;

namespace ProjectHost.Controllers
{
    public class RssController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public Uri BaseUri
        {
            get
            {
                if (Request == null)
                {
                    throw new NullReferenceException("Request cannot be null");
                }

                if (Request.Url == null)
                {
                    throw new NullReferenceException("Request.Url cannot be null");
                }

                // ReSharper disable once Html.PathError
                return new Uri(Request.Url, Url.Content("~/rss"));
            }
        }

        public async Task<RssActionResult> Index()
        {
            var releases = await db.Releases
                .AsNoTracking()
                .Include(r => r.Project)
                .OrderByDescending(r => r.Id)
                .Take(50)
                .ToListAsync();

            var feed = new SyndicationFeed("ProjectHost", "ProjectHost Releases Feed", BaseUri, "ProjectHostAll", DateTime.Now);

            var syndicationItems = releases.Select(MapReleaseToSyndicationItem).ToList();
            feed.Items = syndicationItems;

            return new RssActionResult() { Feed = feed };
        }
        
        public async Task<ActionResult> Projects(int id)
        {
            var releases = await db.Releases
               .AsNoTracking()
               .Include(r => r.Project)
               .Where(r => r.ProjectId == id)
               .OrderByDescending(r => r.Id)
               .Take(50)
               .ToListAsync();

            if (!releases.Any())
            {
                return HttpNotFound("project not found or contains no releases");
            }

            var project = releases.First().Project;

            var feed = new SyndicationFeed(project.Name, project.Description, BaseUri, $"Project{id}", DateTime.Now);
            feed.Items = releases.Select(MapReleaseToSyndicationItem).ToList();

            return new RssActionResult() { Feed = feed };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private SyndicationItem MapReleaseToSyndicationItem(Release release)
        {
            var url = Url.Action("Download", "Releases", new { release.Id });
            if (url == null)
            {
                throw new NullReferenceException("Release Download route has changed");
            }

            var title = $"{release.Project.Name} {release.Version}";

            var content = $"Project:{release.Project.Name} \r\n Version:{release.Version} \r\n {release.Notes}";

            var uriBuilder = new UriBuilder(BaseUri)
            {
                Path = url
            };

            var item = new SyndicationItem(title, content, uriBuilder.Uri);
            return item;
        }
    }

    public class RssActionResult : ActionResult
    {
        public SyndicationFeed Feed { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/rss+xml";

            Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(Feed);
            using (XmlWriter writer = XmlWriter.Create(context.HttpContext.Response.Output))
            {
                rssFormatter.WriteTo(writer);
            }
        }
    }
}