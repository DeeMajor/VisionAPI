using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Linq;
using System.Web;

namespace VisionAPI.Models
{
    public class VisionAPIDbContext : DbContext
    {
        public VisionAPIDbContext() : base("DefaultConnection") 
        {
        }

        public DbSet<Image> Images { get; set; }
        public static VisionAPIDbContext Create()
        {
            return new VisionAPIDbContext();
        }
    }
}