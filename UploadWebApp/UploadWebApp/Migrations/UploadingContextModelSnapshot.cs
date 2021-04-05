﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UploadWebApp.Data;

namespace UploadWebApp.Migrations
{
    [DbContext(typeof(UploadingContext))]
    partial class UploadingContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.4");

            modelBuilder.Entity("UploadWebApp.Models.FileData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ContainsCyrillic")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ContainsLatin")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ContainsNumbers")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ContainsSpChar")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("FileData");
                });
#pragma warning restore 612, 618
        }
    }
}
