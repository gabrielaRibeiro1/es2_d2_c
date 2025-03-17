﻿// <auto-generated />
using System;
using ESOF.WebApp.DBLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ESOF.WebApp.DBLayer.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.Experience", b =>
                {
                    b.Property<int>("experience_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("experience_id"));

                    b.Property<string>("company_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("end_year")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("fk_profile_id")
                        .HasColumnType("integer");

                    b.Property<string>("start_year")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("experience_id");

                    b.HasIndex("fk_profile_id");

                    b.ToTable("Experiences");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.Role", b =>
                {
                    b.Property<int>("role_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("role_id"));

                    b.Property<string>("role")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("role_level")
                        .HasColumnType("integer");

                    b.HasKey("role_id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.Skill", b =>
                {
                    b.Property<int>("skill_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("skill_id"));

                    b.Property<string>("area")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("skill_id");

                    b.ToTable("Skills");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.TalentProfile", b =>
                {
                    b.Property<int>("profile_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("profile_id"));

                    b.Property<string>("country")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("fk_user_id")
                        .HasColumnType("integer");

                    b.Property<float>("price")
                        .HasColumnType("real");

                    b.Property<float>("privacy")
                        .HasColumnType("real");

                    b.Property<string>("profile_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("profile_id");

                    b.HasIndex("fk_user_id");

                    b.HasIndex("user_id");

                    b.ToTable("TalentProfiles");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.User", b =>
                {
                    b.Property<int>("user_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("user_id"));

                    b.Property<int>("fk_role_id")
                        .HasColumnType("integer");

                    b.Property<string>("password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("user_id");

                    b.HasIndex("fk_role_id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.UserSkill", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("SkillId")
                        .HasColumnType("integer");

                    b.HasKey("UserId", "SkillId");

                    b.HasIndex("SkillId");

                    b.ToTable("UserSkills");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.WorkProposal", b =>
                {
                    b.Property<int>("proposal_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("proposal_id"));

                    b.Property<string>("category")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("fk_user_id")
                        .HasColumnType("integer");

                    b.Property<string>("necessary_skills")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("proposal_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("total_hours")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("years_of_experience")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("proposal_id");

                    b.HasIndex("fk_user_id");

                    b.ToTable("WorkProposals");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.Experience", b =>
                {
                    b.HasOne("ESOF.WebApp.DBLayer.Entities.TalentProfile", "Profile")
                        .WithMany("Experiences")
                        .HasForeignKey("fk_profile_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Profile");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.TalentProfile", b =>
                {
                    b.HasOne("ESOF.WebApp.DBLayer.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("fk_user_id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ESOF.WebApp.DBLayer.Entities.User", null)
                        .WithMany("Profiles")
                        .HasForeignKey("user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.User", b =>
                {
                    b.HasOne("ESOF.WebApp.DBLayer.Entities.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("fk_role_id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.UserSkill", b =>
                {
                    b.HasOne("ESOF.WebApp.DBLayer.Entities.Skill", "Skill")
                        .WithMany("UserSkills")
                        .HasForeignKey("SkillId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ESOF.WebApp.DBLayer.Entities.User", "User")
                        .WithMany("UserSkills")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Skill");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.WorkProposal", b =>
                {
                    b.HasOne("ESOF.WebApp.DBLayer.Entities.User", "User")
                        .WithMany("Proposals")
                        .HasForeignKey("fk_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.Role", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.Skill", b =>
                {
                    b.Navigation("UserSkills");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.TalentProfile", b =>
                {
                    b.Navigation("Experiences");
                });

            modelBuilder.Entity("ESOF.WebApp.DBLayer.Entities.User", b =>
                {
                    b.Navigation("Profiles");

                    b.Navigation("Proposals");

                    b.Navigation("UserSkills");
                });
#pragma warning restore 612, 618
        }
    }
}
