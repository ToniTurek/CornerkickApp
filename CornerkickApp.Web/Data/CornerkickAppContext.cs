using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CornerkickApp.Web.Data
{
  public class CornerkickAppContext(DbContextOptions<CornerkickAppContext> options) : IdentityDbContext<CornerkickAppUser>(options)
  {
    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);
      // Customize the ASP.NET Identity model and override the defaults if needed.
      // For example, you can rename the ASP.NET Identity table names and more.
      // Add your customizations after calling base.OnModelCreating(builder);

      // Ignore phone number entries
      builder.Entity<CornerkickAppUser>().Ignore(x => x.PhoneNumber);
      builder.Entity<CornerkickAppUser>().Ignore(x => x.PhoneNumberConfirmed);
      builder.Entity<CornerkickAppUser>().Ignore(x => x.TwoFactorEnabled);

      // Explicitely map type of tinyint to bool
      builder.Entity<CornerkickAppUser>().Property(p => p.EmailConfirmed).HasColumnType("tinyint");
      builder.Entity<CornerkickAppUser>().Property(p => p.LockoutEnabled).HasColumnType("tinyint");
      builder.Entity<CornerkickAppUser>().Property(p => p.bOffline).HasColumnType("tinyint");

      /*
      // Map all types of tinyint to bool
      builder.Properties()
             .Where(x => x.PropertyType == typeof(bool))
             .Configure(x => x.HasColumnType("tinyint"));
      */
    }
  }
}
