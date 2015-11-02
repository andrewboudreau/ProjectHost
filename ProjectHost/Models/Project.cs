using System;
using System.Collections.Generic;

namespace ProjectHost.Models
{
    public class Project
    {
        public Project()
        {
            Description = string.Empty;
            Releases = new List<Release>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<Release> Releases { get; set; }
    }

    public class Release
    {
        public Release()
        {
            ReleaseDate = DateTime.UtcNow;
            Version = $"{ReleaseDate.Year}.{ReleaseDate.Month}.{ReleaseDate.Day}";
        }

        public int Id { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public string Version { get; set; }

        public string Notes { get; set; }

        public string DownloadUrl { get; set; }

        public string SourceCodeUrl { get; set; }

        public DateTime ReleaseDate { get; set; }
    }
}