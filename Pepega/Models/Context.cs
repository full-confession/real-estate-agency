using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pepega.Models;

namespace Pepega.Models
{
    public class Context : DbContext
    {
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyType> PropertyTypes { get; set; }
        public DbSet<BuildingDescription> BuildingDescriptions { get; set; }
        public DbSet<ApartmentDescription> ApartmentDescriptions { get; set; }
        public DbSet<AreaDescription> AreaDescriptions { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<SellOrder> SellOrders { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<SellOrderStatus> SellOrderStatuses { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Street> Streets { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<Ownership> Ownerships { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Sold> Solds { get; set; }

        public DbSet<Manager> Managers { get; set; }

        public Context(DbContextOptions options)
            : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            optionsBuilder.UseSqlite("Data Source = pepega.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<City>()
                .ToTable("City")
                .HasKey(e => e.CityId);

            modelBuilder.Entity<District>()
                .ToTable("District")
                .HasKey(e => e.DistrictId);

            modelBuilder.Entity<District>()
                .HasOne(e => e.City)
                .WithMany(e => e.Districts)
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Street>()
                .ToTable("Street")
                .HasKey(e => e.StreetId);

            modelBuilder.Entity<Street>()
                .HasOne(e => e.District)
                .WithMany(e => e.Streets)
                .HasForeignKey(e => e.DistrictId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Property>()
                .ToTable("Property")
                .HasKey(e => e.PropertyId);

            modelBuilder.Entity<PropertyType>()
                .ToTable("PropertyType")
                .HasKey(e => e.PropertyTypeId);

            modelBuilder.Entity<Property>()
                .HasOne<PropertyType>(e => e.PropertyType)
                .WithMany(e => e.Properties)
                .HasForeignKey(e => e.PropertyTypeId);

            modelBuilder.Entity<Property>()
                .HasOne(e => e.Street)
                .WithMany()
                .HasForeignKey(e => e.StreetId);



            modelBuilder.Entity<BuildingDescription>()
                .ToTable("BuildingDescription")
                .HasKey(e => e.PropertyId);


            modelBuilder.Entity<ApartmentDescription>()
                .ToTable("ApartmentDescription")
                .HasKey(e => e.PropertyId);

            modelBuilder.Entity<AreaDescription>()
                .ToTable("AreaDescription")
                .HasKey(e => e.PropertyId);


            modelBuilder.Entity<BuildingDescription>()
                .HasOne<Property>(e => e.Property)
                .WithOne(e => e.BuildingDescription);

            modelBuilder.Entity<ApartmentDescription>()
                .HasOne<Property>(e => e.Property)
                .WithOne(e => e.ApartmentDescription);

            modelBuilder.Entity<AreaDescription>()
                .HasOne<Property>(e => e.Property)
                .WithOne(e => e.AreaDescription);

            modelBuilder.Entity<Client>()
                .ToTable("Client")
                .HasKey(e => e.ClientId);


            modelBuilder.Entity<Manager>()
                .ToTable("Manager")
                .HasKey(e => e.ManagerId);

            modelBuilder.Entity<Manager>()
                .HasOne(e => e.City)
                .WithMany()
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SellOrder>()
                .ToTable("SellOrder")
                .HasKey(e => e.SellOrderId);


            modelBuilder.Entity<SellOrder>()
                .HasOne(e => e.Property)
                .WithMany(e => e.SellOrders)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SellOrder>()
                .HasOne(e => e.Manager)
                .WithMany(e => e.SellOrders)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Status>()
                .ToTable("Status")
                .HasKey(e => e.StatusId);

            modelBuilder.Entity<SellOrderStatus>()
                .ToTable("SellOrderStatus")
                .HasKey(e => e.SellOrderStatusId);

            modelBuilder.Entity<SellOrderStatus>()
                .HasOne<SellOrder>(e => e.SellOrder)
                .WithMany(e => e.Statuses)
                .HasForeignKey(e => e.SellOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SellOrderStatus>()
                .HasOne<Status>(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Seller>()
                .ToTable("Seller")
                .HasKey(e => e.SellerId);

            modelBuilder.Entity<Seller>()
                .HasOne(e => e.Client)
                .WithMany(e => e.Sellers)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Seller>()
                .HasOne(e => e.SellOrder)
                .WithMany()
                .HasForeignKey(e => e.SellOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Seller>()
                .HasOne(e => e.Ownership)
                .WithOne();

            modelBuilder.Entity<Ownership>()
                .ToTable("Ownership")
                .HasKey(e => e.SellerId);

            modelBuilder.Entity<Sold>()
                .ToTable("Sold")
                .HasKey(e => e.SellOrderId);

            modelBuilder.Entity<Sold>()
                .HasOne(e => e.SellOrder)
                .WithOne(e => e.Sold);

            modelBuilder.Entity<Buyer>()
                .ToTable("Buyer")
                .HasKey(e => e.BuyerId);

            modelBuilder.Entity<Buyer>()
                .HasOne(e => e.Client)
                .WithMany(e => e.Buyers)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Buyer>()
                .HasOne(e => e.SellOrder)
                .WithMany()
                .HasForeignKey(e => e.SellOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Pepega.Models.Manager> Manager { get; set; }
    }
}
