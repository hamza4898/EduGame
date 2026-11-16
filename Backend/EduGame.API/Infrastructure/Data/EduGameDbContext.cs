using Microsoft.EntityFrameworkCore;
using EduGame.Domain.Entities;
using EduGame.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EduGame.Infrastructure.Data;

public class EduGameDbContext : IdentityDbContext<ApplicationUser>
{
    public EduGameDbContext() { }

    public EduGameDbContext(DbContextOptions<EduGameDbContext> options) : base(options) { }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetConnectionString("DefaultConnection");
            
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(EduGameDbContext).Assembly);

        builder.ApplyConfiguration(new BaseUserConfiguration<BaseUser>());

        builder.Entity<LessonBlock>()
            .Property(block => block.Content)
            .HasMaxLength(100_000)
            .HasColumnType("text")
            .IsRequired();

        builder.Entity<Message>()
            .Property(message => message.Text)
            .HasMaxLength(1000)
            .IsRequired();
        builder.Entity<Notification>()
            .Property(notification => notification.Message)
            .HasMaxLength(2000)
            .IsRequired();
        builder.Entity<DeveloperUpdate>(entity =>
        {
            entity.Property(update => update.Title).HasMaxLength(120).IsRequired();
            entity.Property(update => update.Category).HasMaxLength(32).IsRequired();
            entity.Property(update => update.Content)
                .HasMaxLength(10_000)
                .HasColumnType("text")
                .IsRequired();
            entity.Property(update => update.AuthorSignature).HasMaxLength(128).IsRequired();
            entity.Property(update => update.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();
        });

        builder.Entity<CourseNews>().Property(news => news.Text).HasMaxLength(2000).IsRequired();
        builder.Entity<CourseReview>().Property(review => review.Text).HasMaxLength(1000).IsRequired();
        builder.Entity<CoursePromoCode>().Property(promo => promo.Code).HasMaxLength(32).IsRequired();
        builder.Entity<CourseTeacherAccess>().Property(access => access.Role).HasMaxLength(32).IsRequired();
        builder.Entity<Notification>().HasIndex(notification => notification.ReceiverId);
        builder.Entity<CourseTeacherAccess>()
            .HasIndex(access => new { access.CourseId, access.TeacherId })
            .IsUnique();
        builder.Entity<AiSupportMessage>(entity =>
        {
            entity.ToTable("AiSupportMessages");
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Role).HasMaxLength(16).IsRequired();
            entity.Property(message => message.Content)
                .HasMaxLength(100_000)
                .HasColumnType("text")
                .IsRequired();
            entity.Property(message => message.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            entity.HasIndex(message => new { message.UserId, message.CreatedAt });
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(message => message.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<CoursePromoCode>()
            .HasIndex(promo => new { promo.CourseId, promo.Code })
            .IsUnique();
        builder.Entity<CourseReview>()
            .HasIndex(review => new { review.CourseId, review.StudentId })
            .IsUnique();
        builder.Entity<CoinTransaction>()
            .Property(transaction => transaction.Type)
            .HasMaxLength(32)
            .IsRequired();
        builder.Entity<CoinTransaction>()
            .Property(transaction => transaction.CounterpartyId)
            .IsRequired();
        builder.Entity<CoinTransaction>()
            .HasIndex(transaction => new { transaction.UserId, transaction.CreatedAt });
        builder.Entity<CoinTransaction>()
            .HasIndex(transaction => new { transaction.CourseId, transaction.Type });
        builder.Entity<CoinTransaction>()
            .HasIndex(transaction => transaction.TransactionGroupId);
        builder.Entity<TeacherXpAward>()
            .HasIndex(award => award.AwardKey)
            .IsUnique();
        builder.Entity<TeacherXpAward>()
            .HasIndex(award => new { award.TeacherId, award.CourseId });
        builder.Entity<TeacherXpAward>()
            .Property(award => award.Reason)
            .HasMaxLength(32)
            .IsRequired();
        builder.Entity<TeacherXpAward>()
            .Property(award => award.AwardKey)
            .HasMaxLength(300)
            .IsRequired();

        builder.Entity<ApplicationUser>()
            .Property(x => x.RegisteredAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Entity<Student>().ToTable("Students");
        builder.Entity<Teacher>().ToTable("Teachers");

        builder.Entity<CourseLike>()
            .HasKey(cl => new { cl.UserId, cl.CourseId });

        builder.Entity<CourseArchive>()
            .HasKey(cl => new { cl.UserId, cl.CourseId });
        
        builder.Entity<BaseUser>()
            .HasOne(b => b.ApplicationUser)
            .WithOne(a => a.BaseUser)
            .HasForeignKey<BaseUser>(b => b.Id);

        builder.Entity<LessonBlock>()
            .Property(e => e.Type)
            .HasConversion<string>();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<string>()
            .HaveMaxLength(255)
            .AreUnicode(true);
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

    public DbSet<BaseUser> BaseUsers { get; set; }

    public DbSet<Student> Students { get; set; }

    public DbSet<Teacher> Teachers { get; set; }

    public DbSet<Message> Messages { get; set; }

    public DbSet<Community> Communities { get; set; }

    public DbSet<CommunityMember> CommunityMembers { get; set; }

    public DbSet<Friendship> Friendships { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<Course> Courses { get; set; }

    public DbSet<CourseNews> CourseNews { get; set; }

    public DbSet<CourseReview> CourseReviews { get; set; }

    public DbSet<CoinTransaction> CoinTransactions { get; set; }

    public DbSet<TeacherXpAward> TeacherXpAwards { get; set; }

    public DbSet<AiSupportMessage> AiSupportMessages { get; set; }

    public DbSet<DeveloperUpdate> DeveloperUpdates { get; set; }

    public DbSet<CoursePromoCode> CoursePromoCodes { get; set; }

    public DbSet<CourseTeacherAccess> CourseTeacherAccesses { get; set; }

    public DbSet<Section> Sections { get; set; }

    public DbSet<Lesson> Lessons { get; set; }

    public DbSet<LessonBlock> LessonBlocks { get; set; }

    public DbSet<SkillPath> SkillPaths { get; set; }

    public DbSet<SkillSubSection> SkillSubSections { get; set; }

    public DbSet<StudentCourse> StudentCourses { get; set; }

    public DbSet<StudentLesson> StudentLessons { get; set; }

    public DbSet<CourseLike> CourseLikes { get; set; }

    public DbSet<CourseArchive> CourseArchives { get; set; }

    public DbSet<UserSubSectionXp> UserSubSectionXps { get; set; }

    public DbSet<FocusNode> FocusNodes { get; set; }
}