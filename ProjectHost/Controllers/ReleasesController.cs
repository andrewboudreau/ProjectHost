using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProjectHost.Models;

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
                return View(await db.Releases.Include(r => r.Project).Where(x=> x.ProjectId == id.Value).ToListAsync());
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
            return View(new Release() {ProjectId = id.GetValueOrDefault()});
        }

        // POST: Releases/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Version,Notes,DownloadUrl,SourceCodeUrl,ReleaseDate,ProjectId")] Release release)
        {
            var ver = "";
            if (db.Releases.Any(r => r.Version == release.Version && r.ProjectId == release.ProjectId))
            {
                ModelState.AddModelError("Version", $"{release.Version} version is already deployed, suggested '{BumpVersion(release.Version)}'");
                ver = BumpVersion(release.Version);
            }

            if (ModelState.IsValid)
            {
                db.Releases.Add(release);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Projects = new SelectList(db.Projects.ToList(), "Id", "Name");
            release.Version = ver;
            return View(release);
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
        public async Task<ActionResult> Edit([Bind(Include = "Id,Version,Notes,DownloadUrl,SourceCodeUrl,ReleaseDate")] Release release)
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
        private string BumpVersion(string version)
        {
            if (version.Contains("-"))
            {
                var major =  version.Split('-')[0];
                var build = int.Parse(version.Split('-')[1]) + 1;

                return $"{major}-{build}";
            }

            return $"{version}-1";
        }
    }
}
