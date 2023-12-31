﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nomis.DataAccess.PostgreSql.Persistence;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Nomis.DataAccess.PostgreSql.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20221026215743_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Application")
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Nomis.Domain.Entities.EventLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AggregateId")
                        .HasColumnType("uuid");

                    b.Property<int?>("AggregateVersion")
                        .HasColumnType("integer");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EventDescription")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte>("EventType")
                        .HasColumnType("smallint");

                    b.Property<string>("MessageType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NewValues")
                        .HasColumnType("text");

                    b.Property<string>("OldValues")
                        .HasColumnType("text");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("EventLogs", "Application");
                });
#pragma warning restore 612, 618
        }
    }
}
