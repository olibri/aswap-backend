﻿// <auto-generated />
using System;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(P2PDbContext))]
    [Migration("20250503130554_mgr1")]
    partial class mgr1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Models.DB.EscrowOrderEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("amount");

                    b.Property<string>("Buyer")
                        .HasColumnType("text")
                        .HasColumnName("buyer");

                    b.Property<DateTime?>("ClosedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("closed_at_utc");

                    b.Property<DateTime>("CreatedAtUtc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at_utc")
                        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                    b.Property<decimal>("DealId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("deal_id");

                    b.Property<string>("EscrowPda")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("character varying(60)")
                        .HasColumnName("escrow_pda");

                    b.Property<string>("FiatCode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("fiat_code");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("price");

                    b.Property<string>("Seller")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("seller");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<string>("TokenMint")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("token_mint");

                    b.Property<string>("TxInitSig")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("tx_init_sig");

                    b.HasKey("Id");

                    b.ToTable("escrow_orders");
                });
#pragma warning restore 612, 618
        }
    }
}
