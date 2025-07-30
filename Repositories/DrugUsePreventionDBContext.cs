using System;
using System.IO;
using System.Text.Json;
using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;

namespace Repositories
{
    public class DrugUsePreventionDBContext : DbContext
    {
        public DrugUsePreventionDBContext() { }

        public DrugUsePreventionDBContext(DbContextOptions<DrugUsePreventionDBContext> options)
            : base(options) { }

        public virtual DbSet<DashboardData> DashboardData { get; set; }
        public virtual DbSet<Consultant> Consultants { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<CourseRegistration> CourseRegistrations { get; set; }
        public virtual DbSet<UserSurveyResponse> UserSurveyResponses { get; set; }
        public virtual DbSet<ProgramParticipation> ProgramParticipations { get; set; }
        public virtual DbSet<Survey> Surveys { get; set; }
        public virtual DbSet<SurveyAnswer> SurveyAnswers { get; set; }
        public virtual DbSet<SurveyQuestion> SurveyQuestions { get; set; }
        public virtual DbSet<CheckCourseContent> CheckCourseContents { get; set; }
        public virtual DbSet<Program> Programs { get; set; }
        public virtual DbSet<UserSurveyAnswer> UserSurveyAnswers { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<CourseContent> CourseContents { get; set; }
        public virtual DbSet<NewsArticle> NewsArticles { get; set; }
        public virtual DbSet<NewsTag> NewsTags { get; set; }
        public virtual DbSet<ConsultantSchedule> ConsultantSchedules { get; set; }
        public virtual DbSet<ConsultantAvailability> ConsultantAvailabilities { get; set; }
        public virtual DbSet<AppointmentSlot> AppointmentSlots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.Restrict); // tránh vòng lặp xóa

            modelBuilder
                .Entity<Appointment>()
                .HasOne(a => a.Consultant)
                .WithMany()
                .HasForeignKey(a => a.ConsultantID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consultant>().Property(a => a.WorkingHours)
                .HasConversion(new WorkingHoursConverter())
                .HasColumnType("nvarchar(max)");

            // Configure ConsultantSchedule relationships
            modelBuilder.Entity<ConsultantSchedule>()
                .HasOne(cs => cs.Consultant)
                .WithMany()
                .HasForeignKey(cs => cs.ConsultantID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ConsultantAvailability relationships
            modelBuilder.Entity<ConsultantAvailability>()
                .HasOne(ca => ca.Consultant)
                .WithMany()
                .HasForeignKey(ca => ca.ConsultantID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure AppointmentSlot relationships
            modelBuilder.Entity<AppointmentSlot>()
                .HasOne(slot => slot.Consultant)
                .WithMany()
                .HasForeignKey(slot => slot.ConsultantID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppointmentSlot>()
                .HasOne(slot => slot.Appointment)
                .WithMany()
                .HasForeignKey(slot => slot.AppointmentID)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }

        // DbSet properties for your entities
        // public DbSet<YourEntity> YourEntities { get; set; }
    }
}
