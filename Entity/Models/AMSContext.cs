using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Entity.Models;

public partial class AMSContext : DbContext
{
    public AMSContext()
    {
    }

    public AMSContext(DbContextOptions<AMSContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<Leave> Leaves { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Attendanceid).HasName("attendances_pkey");

            entity.ToTable("attendances");

            entity.Property(e => e.Attendanceid).HasColumnName("attendanceid");
            entity.Property(e => e.Approvedby).HasColumnName("approvedby");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Intime).HasColumnName("intime");
            entity.Property(e => e.Isapproved).HasColumnName("isapproved");
            entity.Property(e => e.Outtime).HasColumnName("outtime");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Updatedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Updatedby).HasColumnName("updatedby");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.ApprovedbyNavigation).WithMany(p => p.AttendanceApprovedbyNavigations)
                .HasForeignKey(d => d.Approvedby)
                .HasConstraintName("attendances_approvedby_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.AttendanceUsers)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("attendances_userid_fkey");
        });

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.Holidayid).HasName("holiday_pkey");

            entity.ToTable("holiday");

            entity.Property(e => e.Holidayid).HasColumnName("holidayid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Holidaydate).HasColumnName("holidaydate");
            entity.Property(e => e.Holidayname)
                .HasMaxLength(100)
                .HasColumnName("holidayname");
            entity.Property(e => e.Isdeleted).HasColumnName("isdeleted");
            entity.Property(e => e.Updatedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Updatedby).HasColumnName("updatedby");
        });

        modelBuilder.Entity<Leave>(entity =>
        {
            entity.HasKey(e => e.Leaveid).HasName("leaves_pkey");

            entity.ToTable("leaves");

            entity.Property(e => e.Leaveid).HasColumnName("leaveid");
            entity.Property(e => e.Approvedby).HasColumnName("approvedby");
            entity.Property(e => e.Comment)
                .HasMaxLength(500)
                .HasColumnName("comment");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Fromdate).HasColumnName("fromdate");
            entity.Property(e => e.Isapproved).HasColumnName("isapproved");
            entity.Property(e => e.Isdeleted).HasColumnName("isdeleted");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .HasColumnName("reason");
            entity.Property(e => e.Todate).HasColumnName("todate");
            entity.Property(e => e.Updatedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Updatedby).HasColumnName("updatedby");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.ApprovedbyNavigation).WithMany(p => p.LeaveApprovedbyNavigations)
                .HasForeignKey(d => d.Approvedby)
                .HasConstraintName("leaves_approvedby_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.LeaveUsers)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("leaves_userid_fkey");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Logid).HasName("logs_pkey");

            entity.ToTable("logs");

            entity.Property(e => e.Logid).HasColumnName("logid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Logmessage)
                .HasMaxLength(1000)
                .HasColumnName("logmessage");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Rolename, "roles_rolename_key").IsUnique();

            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Rolename).HasColumnName("rolename");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Department)
                .HasMaxLength(50)
                .HasColumnName("department");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.Isdeleted).HasColumnName("isdeleted");
            entity.Property(e => e.Managerid).HasColumnName("managerid");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(500)
                .HasColumnName("password");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Updatedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Updatedby).HasColumnName("updatedby");

            entity.HasOne(d => d.Manager).WithMany(p => p.InverseManager)
                .HasForeignKey(d => d.Managerid)
                .HasConstraintName("users_managerid_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_roleid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
