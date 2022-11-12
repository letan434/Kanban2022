using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KanbanApp.BackendServer.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace KanbanApp.BackendServer.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
            builder.Entity<User>().Property(x => x.Id).HasMaxLength(50).IsUnicode(false);

            builder.Entity<CommandInFunction>()
                        .HasKey(c => new { c.CommandId, c.FunctionId });

            builder.Entity<Permission>()
                       .HasKey(c => new { c.RoleId, c.FunctionId, c.CommandId });

            builder.Entity<UserInIssue>()
                        .HasKey(c => new { c.UserId, c.IssueId });

            builder.Entity<UserInProject>()
                       .HasKey(c => new { c.UserId, c.ProjectId });
            builder.Entity<LabelInIssue>()
                       .HasKey(c => new { c.LabelId, c.IssueId });

            builder.HasSequence("KanbanAppSequence");
        }
        public DbSet<Command> Commands { set; get; }
        public DbSet<CommandInFunction> CommandInFunctions { set; get; }

        public DbSet<ActivityLog> ActivityLogs { set; get; }
        public DbSet<Category> Categories { set; get; }
        public DbSet<Comment> Comments { set; get; }

        public DbSet<Function> Functions { set; get; }
        public DbSet<Project> Projects { set; get; }
        public DbSet<Issue> Issues { set; get; }
        public DbSet<Label> Labels { set; get; }
        public DbSet<LabelInIssue> LabelInIssues { set; get; }

        public DbSet<Permission> Permissions { set; get; }
        public DbSet<UserInProject> UserInProjects { set; get; }
        public DbSet<UserInIssue> UserInIssues { set; get; }

        public DbSet<Status> Statuses { set; get; }

        public DbSet<Attachment> Attachments { get; set; }
    }

}
