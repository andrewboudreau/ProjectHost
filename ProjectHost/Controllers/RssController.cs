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
            var syndicationItems = new List<SyndicationItem>(50);

            foreach (var release in releases)
            {
                var url = Url.Action("Download", "Releases", new { projectId = release.ProjectId, releaseId = release.Id });
                if (url == null)
                {
                    throw new NullReferenceException("Release Download route has changed");
                }

                var title = $"{release.Project.Name} {release.Version}";
                var content = $"Project:{release.Project.Name} {Environment.NewLine} Version:{release.Version} {Environment.NewLine} {release.Notes}";
                var uriBuilder = new UriBuilder(BaseUri) { Path = url };
                var item = new SyndicationItem(title, content, uriBuilder.Uri);

                syndicationItems.Add(item);
            }

            feed.Items = syndicationItems;
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
}