using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace VisionAPI.Models
{
    public class VisionAPIDbContext : DbContext
    {
        public VisionAPIDbContext() : base() { }

        public DbSet<Image> Images { get; set; }
    }
}