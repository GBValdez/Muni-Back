using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using project.ModelsDto;
using project.roles;
using project.users;
using project.users.Models;
using project.utils.catalogue;

namespace project.Models;

public partial class DBProyContext : IdentityDbContext<userEntity, rolEntity, string>
{
    IConfiguration _configuration;
    public DBProyContext(DbContextOptions<DBProyContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }
    public DbSet<binnacleBody> BinnacleBodies { get; set; }
    public DbSet<binnacleHeader> BinnacleHeaders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));

}
