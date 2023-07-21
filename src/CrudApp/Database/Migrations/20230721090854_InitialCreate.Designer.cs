﻿// <auto-generated />
using System;
using CrudApp.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CrudApp.Database.Migrations
{
    [DbContext(typeof(CrudAppDbContext))]
    [Migration("20230721090854_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.9");

            modelBuilder.Entity("CrudApp.Authorization.AuthorizationGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSoftDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Version")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("AuthorizationGroup");
                });

            modelBuilder.Entity("CrudApp.Authorization.AuthorizationGroupEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AuthorizationGroupId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("EntityId")
                        .HasColumnType("TEXT");

                    b.Property<string>("EntityType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSoftDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Version")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AuthorizationGroupId");

                    b.ToTable("AuthorizationGroupEntity");
                });

            modelBuilder.Entity("CrudApp.Authorization.AuthorizationGroupMembership", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AuthorizationGroupId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AuthorizationRoleId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSoftDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<long>("Version")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AuthorizationGroupId");

                    b.HasIndex("AuthorizationRoleId");

                    b.HasIndex("UserId", "AuthorizationGroupId", "AuthorizationRoleId")
                        .IsUnique();

                    b.ToTable("AuthorizationGroupMembership");
                });

            modelBuilder.Entity("CrudApp.Authorization.AuthorizationRole", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSoftDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Version")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("AuthorizationRole");
                });

            modelBuilder.Entity("CrudApp.ChangeTracking.EntityChangeEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ActivityId")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("AuthPrincipalId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ChangeType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("EntityId")
                        .HasColumnType("TEXT");

                    b.Property<string>("EntityType")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSoftDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Time")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Version")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("EntityId");

                    b.ToTable("EntityChangeEvent");
                });

            modelBuilder.Entity("CrudApp.ChangeTracking.PropertyChangeEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("EntityChangeEventId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSoftDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("NewPropertyValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("OldPropertyValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("PropertyName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("Version")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("EntityChangeEventId");

                    b.ToTable("PropertyChangeEvent");
                });

            modelBuilder.Entity("CrudApp.SuperHeroes.SuperHero", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("CivilName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSoftDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SuperHeroName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("Version")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("SuperHero");
                });

            modelBuilder.Entity("CrudApp.Users.User", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSoftDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Version")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("CrudApp.Authorization.AuthorizationGroupEntity", b =>
                {
                    b.HasOne("CrudApp.Authorization.AuthorizationGroup", "AuthorizationGroup")
                        .WithMany("AuthorizationGroupEntities")
                        .HasForeignKey("AuthorizationGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AuthorizationGroup");
                });

            modelBuilder.Entity("CrudApp.Authorization.AuthorizationGroupMembership", b =>
                {
                    b.HasOne("CrudApp.Authorization.AuthorizationGroup", "AuthorizationGroup")
                        .WithMany("AuthorizationGroupMemberships")
                        .HasForeignKey("AuthorizationGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CrudApp.Authorization.AuthorizationRole", "AuthorizationRole")
                        .WithMany("AuthorizationGroupMemberships")
                        .HasForeignKey("AuthorizationRoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CrudApp.Users.User", "User")
                        .WithMany("AuthorizationGroupMemberships")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AuthorizationGroup");

                    b.Navigation("AuthorizationRole");

                    b.Navigation("User");
                });

            modelBuilder.Entity("CrudApp.ChangeTracking.EntityChangeEvent", b =>
                {
                    b.HasOne("CrudApp.Authorization.AuthorizationGroup", null)
                        .WithMany("EntityChangeEvents")
                        .HasForeignKey("EntityId");

                    b.HasOne("CrudApp.Authorization.AuthorizationGroupEntity", null)
                        .WithMany("EntityChangeEvents")
                        .HasForeignKey("EntityId");

                    b.HasOne("CrudApp.Authorization.AuthorizationGroupMembership", null)
                        .WithMany("EntityChangeEvents")
                        .HasForeignKey("EntityId");

                    b.HasOne("CrudApp.Authorization.AuthorizationRole", null)
                        .WithMany("EntityChangeEvents")
                        .HasForeignKey("EntityId");

                    b.HasOne("CrudApp.ChangeTracking.EntityChangeEvent", null)
                        .WithMany("EntityChangeEvents")
                        .HasForeignKey("EntityId");

                    b.HasOne("CrudApp.ChangeTracking.PropertyChangeEvent", null)
                        .WithMany("EntityChangeEvents")
                        .HasForeignKey("EntityId");

                    b.HasOne("CrudApp.SuperHeroes.SuperHero", null)
                        .WithMany("EntityChangeEvents")
                        .HasForeignKey("EntityId");

                    b.HasOne("CrudApp.Users.User", null)
                        .WithMany("EntityChangeEvents")
                        .HasForeignKey("EntityId");
                });

            modelBuilder.Entity("CrudApp.ChangeTracking.PropertyChangeEvent", b =>
                {
                    b.HasOne("CrudApp.ChangeTracking.EntityChangeEvent", "EntityChangeEvent")
                        .WithMany("PropertyChangeEvents")
                        .HasForeignKey("EntityChangeEventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EntityChangeEvent");
                });

            modelBuilder.Entity("CrudApp.Authorization.AuthorizationGroup", b =>
                {
                    b.Navigation("AuthorizationGroupEntities");

                    b.Navigation("AuthorizationGroupMemberships");

                    b.Navigation("EntityChangeEvents");
                });

            modelBuilder.Entity("CrudApp.Authorization.AuthorizationGroupEntity", b =>
                {
                    b.Navigation("EntityChangeEvents");
                });

            modelBuilder.Entity("CrudApp.Authorization.AuthorizationGroupMembership", b =>
                {
                    b.Navigation("EntityChangeEvents");
                });

            modelBuilder.Entity("CrudApp.Authorization.AuthorizationRole", b =>
                {
                    b.Navigation("AuthorizationGroupMemberships");

                    b.Navigation("EntityChangeEvents");
                });

            modelBuilder.Entity("CrudApp.ChangeTracking.EntityChangeEvent", b =>
                {
                    b.Navigation("EntityChangeEvents");

                    b.Navigation("PropertyChangeEvents");
                });

            modelBuilder.Entity("CrudApp.ChangeTracking.PropertyChangeEvent", b =>
                {
                    b.Navigation("EntityChangeEvents");
                });

            modelBuilder.Entity("CrudApp.SuperHeroes.SuperHero", b =>
                {
                    b.Navigation("EntityChangeEvents");
                });

            modelBuilder.Entity("CrudApp.Users.User", b =>
                {
                    b.Navigation("AuthorizationGroupMemberships");

                    b.Navigation("EntityChangeEvents");
                });
#pragma warning restore 612, 618
        }
    }
}
