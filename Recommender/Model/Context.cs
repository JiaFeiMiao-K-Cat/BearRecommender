using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Model;

public class Context : DbContext
{
    private readonly string _connectionString;

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Rating> Ratings { get; set; }

    public Context(string connectionString)
    {
        _connectionString = connectionString;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
    }
}
