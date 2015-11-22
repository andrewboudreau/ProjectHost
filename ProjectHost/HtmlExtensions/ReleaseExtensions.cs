using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Microsoft.Ajax.Utilities;
using ProjectHost.Models;

namespace ProjectHost.HtmlExtensions
{
    public static class ReleaseExtensions
    {
        public static MvcHtmlString DownloadReleaseLink(this HtmlHelper html, Release release, string linkText)
        {
            return html.ActionLink(linkText, "Download", "Releases", new {projectId = release.ProjectId, releaseId = release.Id});
        }
    }
}