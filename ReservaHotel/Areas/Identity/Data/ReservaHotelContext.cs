using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReservaHotel.Areas.Identity.Data;
using ReservaHotel.Models;

namespace ReservaHotel.Data;

public class ReservaHotelContext : IdentityDbContext<ReservaHotelUser>
{
    public ReservaHotelContext(DbContextOptions<ReservaHotelContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

public DbSet<ReservaHotel.Models.CheckIn> CheckIn { get; set; } = default!;

public DbSet<ReservaHotel.Models.CheckOut> CheckOut { get; set; } = default!;

public DbSet<ReservaHotel.Models.Funcionario> Funcionario { get; set; } = default!;

public DbSet<ReservaHotel.Models.Hospede> Hospede { get; set; } = default!;

public DbSet<ReservaHotel.Models.Pacote> Pacote { get; set; } = default!;

public DbSet<ReservaHotel.Models.Quarto> Quarto { get; set; } = default!;

public DbSet<ReservaHotel.Models.Reserva> Reserva { get; set; } = default!;
}
