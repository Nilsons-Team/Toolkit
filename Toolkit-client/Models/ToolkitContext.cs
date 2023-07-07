using System.Collections.Generic;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace Toolkit_Client.Models;

public partial class ToolkitContext : DbContext
{
    public ToolkitContext()
    {
    }

    public ToolkitContext(DbContextOptions<ToolkitContext> options)
        : base(options)
    {
    }

    public virtual DbSet<App> Apps { get; set; }

    public virtual DbSet<AppKey> AppKeys { get; set; }

    public virtual DbSet<AppPurchaseMethod> AppPurchaseMethods { get; set; }

    public virtual DbSet<AppReleaseState> AppReleaseStates { get; set; }

    public virtual DbSet<AppStorePage> AppStorePages { get; set; }

    public virtual DbSet<AppType> AppTypes { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPurchasedApp> UserPurchasedApps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"datasource={ToolkitApp.DbFilePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<App>(entity =>
        {
            entity.ToTable("App");

            entity.HasIndex(e => e.Name, "IX_App_Name").IsUnique();

            entity.Property(e => e.DiscountPercent).HasColumnType("INT");
            entity.Property(e => e.UploadDatetime).HasDefaultValueSql("DATETIME('now')");

            entity.HasOne(d => d.AppReleaseState).WithMany(p => p.Apps)
                .HasForeignKey(d => d.AppReleaseStateId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.AppType).WithMany(p => p.Apps)
                .HasForeignKey(d => d.AppTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.DeveloperCompany).WithMany(p => p.AppDeveloperCompanies).HasForeignKey(d => d.DeveloperCompanyId);

            entity.HasOne(d => d.PublisherCompany).WithMany(p => p.AppPublisherCompanies).HasForeignKey(d => d.PublisherCompanyId);

            entity.HasMany(d => d.Categories).WithMany(p => p.Apps)
                .UsingEntity<Dictionary<string, object>>(
                    "AppCategory",
                    r => r.HasOne<Category>().WithMany().HasForeignKey("CategoryId"),
                    l => l.HasOne<App>().WithMany().HasForeignKey("AppId"),
                    j =>
                    {
                        j.HasKey("AppId", "CategoryId");
                        j.ToTable("AppCategory");
                    });

            entity.HasMany(d => d.DependantApps).WithMany(p => p.RequiredApps)
                .UsingEntity<Dictionary<string, object>>(
                    "AppDependency",
                    r => r.HasOne<App>().WithMany().HasForeignKey("DependantAppId"),
                    l => l.HasOne<App>().WithMany().HasForeignKey("RequiredAppId"),
                    j =>
                    {
                        j.HasKey("DependantAppId", "RequiredAppId");
                        j.ToTable("AppDependency");
                    });

            entity.HasMany(d => d.RequiredApps).WithMany(p => p.DependantApps)
                .UsingEntity<Dictionary<string, object>>(
                    "AppDependency",
                    r => r.HasOne<App>().WithMany().HasForeignKey("RequiredAppId"),
                    l => l.HasOne<App>().WithMany().HasForeignKey("DependantAppId"),
                    j =>
                    {
                        j.HasKey("DependantAppId", "RequiredAppId");
                        j.ToTable("AppDependency");
                    });

            entity.HasMany(d => d.Tags).WithMany(p => p.Apps)
                .UsingEntity<Dictionary<string, object>>(
                    "AppTag",
                    r => r.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                    l => l.HasOne<App>().WithMany().HasForeignKey("AppId"),
                    j =>
                    {
                        j.HasKey("AppId", "TagId");
                        j.ToTable("AppTag");
                    });
        });

        modelBuilder.Entity<AppKey>(entity =>
        {
            entity.HasKey(e => e.Key);

            entity.ToTable("AppKey");

            entity.Property(e => e.IsDisabled).HasColumnType("INT");

            entity.HasOne(d => d.ActivatedByUser).WithMany(p => p.AppKeyActivatedByUsers)
                .HasForeignKey(d => d.ActivatedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.App).WithMany(p => p.AppKeys).HasForeignKey(d => d.AppId);

            entity.HasOne(d => d.AppPurchaseMethod).WithMany(p => p.AppKeys).HasForeignKey(d => d.AppPurchaseMethodId);

            entity.HasOne(d => d.IssuerUser).WithMany(p => p.AppKeyIssuerUsers).HasForeignKey(d => d.IssuerUserId);
        });

        modelBuilder.Entity<AppPurchaseMethod>(entity =>
        {
            entity.ToTable("AppPurchaseMethod");

            entity.Property(e => e.Hours).HasColumnType("INT");
            entity.Property(e => e.IsPerpetual).HasColumnType("INT");

            entity.HasOne(d => d.App).WithMany(p => p.AppPurchaseMethods).HasForeignKey(d => d.AppId);
        });

        modelBuilder.Entity<AppReleaseState>(entity =>
        {
            entity.ToTable("AppReleaseState");

            entity.HasIndex(e => e.Name, "IX_AppReleaseState_Name").IsUnique();
        });

        modelBuilder.Entity<AppStorePage>(entity =>
        {
            entity.ToTable("AppStorePage");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.AppStorePage).HasForeignKey<AppStorePage>(d => d.Id);
        });

        modelBuilder.Entity<AppType>(entity =>
        {
            entity.ToTable("AppType");

            entity.HasIndex(e => e.Name, "IX_AppType_Name").IsUnique();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.HasIndex(e => e.Name, "IX_Category_Name").IsUnique();
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Company");

            entity.HasIndex(e => e.LegalName, "IX_Company_LegalName").IsUnique();

            entity.Property(e => e.RegistrationDatetime).HasDefaultValueSql("DATETIME('now')");

            entity.HasOne(d => d.Country).WithMany(p => p.Companies).HasForeignKey(d => d.CountryId);

            entity.HasOne(d => d.OwnerUser).WithMany(p => p.Companies).HasForeignKey(d => d.OwnerUserId);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.TwoLetterIsoCode);

            entity.ToTable("Country");

            entity.HasIndex(e => e.CountryName, "IX_Country_CountryName").IsUnique();

            entity.HasIndex(e => e.StateName, "IX_Country_StateName").IsUnique();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tag");

            entity.HasIndex(e => e.Name, "IX_Tag_Name").IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.HasIndex(e => e.Login, "IX_User_Login").IsUnique();

            entity.Property(e => e.RegistrationDatetime).HasDefaultValueSql("DATETIME('now')");

            entity.HasOne(d => d.Country).WithMany(p => p.Users).HasForeignKey(d => d.CountryId);
        });

        modelBuilder.Entity<UserPurchasedApp>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.AppId });

            entity.ToTable("UserPurchasedApp");

            entity.Property(e => e.PurchaseDatetime).HasDefaultValueSql("DATETIME('now')");

            entity.HasOne(d => d.App).WithMany(p => p.UserPurchasedApps).HasForeignKey(d => d.AppId);

            entity.HasOne(d => d.AppPurchaseMethod).WithMany(p => p.UserPurchasedApps).HasForeignKey(d => d.AppPurchaseMethodId);

            entity.HasOne(d => d.User).WithMany(p => p.UserPurchasedApps).HasForeignKey(d => d.UserId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
