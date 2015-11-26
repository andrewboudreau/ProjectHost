using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.Data.OData.Atom;
using ProjectHost.Models;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ProjectHost.Controllers
{
    public class ReleasesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Releases
        public async Task<ActionResult> Index(int? id)
        {
            if (id.HasValue)
            {
                ViewBag.Id = id;
                return View(await db.Releases.Include(r => r.Project).Where(x => x.ProjectId == id.Value).ToListAsync());
            }

            return View(await db.Releases.Include(r => r.Project).ToListAsync());
        }

        // GET: Releases/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Release release = await db.Releases.FindAsync(id);
            if (release == null)
            {
                return HttpNotFound();
            }
            return View(release);
        }

        // GET: Releases/Create
        public ActionResult Create(int? id)
        {
            ViewBag.Projects = new SelectList(db.Projects.ToList(), "Id", "Name", id.GetValueOrDefault());
            return View(new Release { ProjectId = id.GetValueOrDefault() });
        }

        // POST: Releases/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Version,Notes,DownloadUrl,SourceCodeUrl,ReleaseDate,ProjectId")] Release release, HttpPostedFileBase file)
        {
            var ver = "";
            if (db.Releases.Any(r => r.Version == release.Version && r.ProjectId == release.ProjectId))
            {
                ModelState.AddModelError("Version", $"{release.Version} version is already deployed, suggested '{IncrementVersion(release.Version)}'");
                ver = IncrementVersion(release.Version);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Projects = new SelectList(db.Projects.ToList(), "Id", "Name");
                release.Version = ver;
                return View(release);
            }

            db.Releases.Add(release);
            await db.SaveChangesAsync();

            if (file?.InputStream != null)
            {
                release.DownloadUrl = (await WriteReleaseBinaryAsync(release, file)).ToString();
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GET: Releases/Edit/5
        public async Task<ActionResult> Edit(int? id, int? projectId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Release release = await db.Releases.FindAsync(id);
            if (release == null)
            {
                return HttpNotFound();
            }

            ViewBag.Projects = new SelectList(db.Projects.ToList(), "Id", "Name", projectId ?? 1);
            return View(release);
        }

        // POST: Releases/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Version,Notes,DownloadUrl,SourceCodeUrl,ReleaseDate")] Release release, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                db.Entry(release).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Projects = new SelectList(db.Projects.ToList(), "Id", "Name", release.ProjectId);
            return View(release);
        }

        // GET: Releases/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Release release = await db.Releases.FindAsync(id);
            if (release == null)
            {
                return HttpNotFound();
            }
            return View(release);
        }

        // POST: Releases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Release release = await db.Releases.FindAsync(id);
            db.Releases.Remove(release);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Download(int id)
        {
            var release = await db.Releases.FindAsync(id);
            if (release == null)
            {
                return HttpNotFound();
            }

            var file = await ReadReleaseBinaryAsync(release);

            return File(file.Stream, MimeMapping.GetMimeMapping(file.Name), file.Name);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Make a dumb guess at an un-used version
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private string IncrementVersion(string version)
        {
            if (version.Contains("-"))
            {
                var major = version.Split('-')[0];
                var build = int.Parse(version.Split('-')[1]) + 1;

                return $"{major}-{build}";
            }

            return $"{version}-1";
        }

        private static CloudBlockBlob GetBlob(Release release)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference($"project-{release.ProjectId}");

            container.CreateIfNotExists();
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            var blob = container.GetBlockBlobReference($"release-{release.Id}");
            return blob;
        }

        private static async Task<Uri> WriteReleaseBinaryAsync(Release release, HttpPostedFileBase file)
        {
            var blob = GetBlob(release);
            blob.Metadata.Add("name", file.FileName);
            blob.Metadata.Add("size", file.ContentLength.ToString());
            await blob.UploadFromStreamAsync(file.InputStream);
            await blob.SetMetadataAsync();

            return blob.Uri;
        }

        private static async Task<StreamWithName> ReadReleaseBinaryAsync(Release release)
        {
            var ms = new MemoryStream();

            var blob = GetBlob(release);
            await blob.DownloadToStreamAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);

            return new StreamWithName
            {
                Name = blob.Metadata["name"],
                Stream = ms
            };
        }

        private class StreamWithName
        {
            public Stream Stream { get; set; }

            public string Name { get; set; }
        }
    }
}
