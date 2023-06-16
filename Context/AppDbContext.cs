﻿using IS_ZJZ_B.Models;
using Microsoft.EntityFrameworkCore;

namespace IS_ZJZ_B.Context
{
    public class AppDbContext: DbContext
    {
        //konstruktor
        public AppDbContext(DbContextOptions <AppDbContext> options ): base (options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<AdministrativeWorker> Administrativeworkers { get; set; }
        public DbSet<Admin> Admin { get; set; }

        //ADMINISTRATOR
        public DbSet<HealthCenterEmployee> HealthCenterEmployees { get; set; }

        //modelBuilder je klasa koja pomaže za konekciju sa entitijem u .net
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<AdministrativeWorker>().ToTable("administrativeworkers");
            modelBuilder.Entity<Admin>().ToTable("admins");

            //ADMINISTRATOR
            modelBuilder.Entity<HealthCenterEmployee>().ToTable("healthcenteremployee");
        }
    }
}
