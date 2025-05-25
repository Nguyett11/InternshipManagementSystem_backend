using IMS.Models;
using Microsoft.EntityFrameworkCore;

namespace IMS.DataConnection
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options)
        {
        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Companies> Companies { get; set; }
        public DbSet<Lecturers> Lecturers { get; set; }
        public DbSet<Mentors> Mentors { get; set; }
        public DbSet<Reports> Reports { get; set; }
        public DbSet<ReportStudents> ReportStudents { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Students> Students { get; set; }

        public DbSet<Submissions> Submissions { get; set; }
        public DbSet<Grading> Grading { get; set; }
    }

}