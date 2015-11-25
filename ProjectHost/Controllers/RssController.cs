using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace ProjectHost.Controllers
{
    public class RssController : Controller
    {
        public ActionResult Index()
        {
            if (Request == null)
            {
                throw new NullReferenceException("Request cannot be null");
            }

            if (Request.Url == null)
            {
                throw new NullReferenceException("Request.Url cannot be null");
            }

            if (Request.ApplicationPath == null)
            {
                throw new NullReferenceException("Request.ApplicationPath cannot be null");
            }

            var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            SyndicationFeed feed = new SyndicationFeed("ProjectHost", "ProjectHost Releases Feed", new Uri(baseUrl + "/RSS/"), "ProjectHostAll", DateTime.Now);


            return new RssActionResult(feed);
        }

        public class RssActionResult : ActionResult
        {
            public RssActionResult(SyndicationFeed feed)
            {
                Feed = feed;
            }

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