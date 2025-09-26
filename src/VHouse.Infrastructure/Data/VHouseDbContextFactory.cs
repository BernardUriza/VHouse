using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VHouse.Infrastructure.Data;

public class VHouseDbContextFactory : IDesignTimeDbContextFactory<VHouseDbContext>
{
    public VHouseDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VHouseDbContext>();
        optionsBuilder.UseSqlite("Data Source=vhouse_clean.db");

        return new VHouseDbContext(optionsBuilder.Options);
    }
}