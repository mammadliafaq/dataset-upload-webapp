using System;
using Microsoft.EntityFrameworkCore;
using UploadWebApp.Models;

namespace UploadWebApp.Data
{
    public class UploadingContext : DbContext
    {
        public UploadingContext(DbContextOptions<UploadingContext> options) : base(options)
        {
        }
    public DbSet<FileData> FileData { get; set; }
}
}
