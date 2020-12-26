﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NclVaultAPIServer.Data;

namespace NclVaultAPIServer.Migrations
{
    [DbContext(typeof(VaultDbContext))]
    [Migration("20201202151202_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("NclVaultAPIServer.Models.Credential", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Credentials");
                });

            modelBuilder.Entity("NclVaultAPIServer.Models.EntryBase", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CredentialFK")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Expired")
                        .HasColumnType("TEXT");

                    b.Property<string>("Group")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CredentialFK");

                    b.ToTable("EntryBase");

                    b.HasDiscriminator<string>("Discriminator").HasValue("EntryBase");
                });

            modelBuilder.Entity("NclVaultAPIServer.Models.PasswordEntry", b =>
                {
                    b.HasBaseType("NclVaultAPIServer.Models.EntryBase");

                    b.Property<string>("Password")
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue("PasswordEntry");
                });

            modelBuilder.Entity("NclVaultAPIServer.Models.EntryBase", b =>
                {
                    b.HasOne("NclVaultAPIServer.Models.Credential", "Credential")
                        .WithMany("Entries")
                        .HasForeignKey("CredentialFK")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Credential");
                });

            modelBuilder.Entity("NclVaultAPIServer.Models.Credential", b =>
                {
                    b.Navigation("Entries");
                });
#pragma warning restore 612, 618
        }
    }
}
