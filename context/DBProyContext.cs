using back.catalogues;
using back.reports;
using back.votes;
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
    public DbSet<Reports> Reports { get; set; }
    public DbSet<Status> Status { get; set; }
    public DbSet<back.catalogues.Type> Types { get; set; }
    public DbSet<Votes> Votes { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));

}
