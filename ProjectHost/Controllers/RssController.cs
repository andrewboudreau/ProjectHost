using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Mvc;

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

            
            return new 
        }
    }
}