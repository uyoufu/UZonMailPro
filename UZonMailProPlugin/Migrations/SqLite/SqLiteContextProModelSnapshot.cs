﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UZonMailProPlugin.SQL;

#nullable disable

namespace UZonMailProPlugin.Migrations.SqLite
{
    [DbContext(typeof(SqLiteContextPro))]
    partial class SqLiteContextProModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.13");

            modelBuilder.Entity("EmailAnchorEmailVisitHistory", b =>
                {
                    b.Property<long>("EmailAnchorId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("VisitedHistoriesId")
                        .HasColumnType("INTEGER");

                    b.HasKey("EmailAnchorId", "VisitedHistoriesId");

                    b.HasIndex("VisitedHistoriesId");

                    b.ToTable("EmailAnchorEmailVisitHistory");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.ApiAccess.AccessToken", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Enable")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ExpireDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("JwtId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("AccessTokens");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.EmailCrawler.CrawlerTaskInfo", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Count")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Deadline")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<long>("OutboxGroupId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("ProxyId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TikTokDeviceId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("CrawlerTaskInfos");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.EmailCrawler.CrawlerTaskResult", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("CrawlerTaskInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("ExistExtraInfo")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsAttachingInbox")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<long>("TikTokAuthorId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TikTokAuthorId");

                    b.ToTable("CrawlerTaskResults");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.EmailCrawler.TikTokAuthorDiversification", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<long>("DiversificationId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<long>("TikTokAuthorId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("TikTokAuthorDiversifications");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.EmailCrawler.TikTokDevice", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("DeviceId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsShared")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<string>("OdinId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("OrganizationId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("TikTokDevices");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.EmailCrawler.TiktokAuthor", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AvatarLarger")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AvatarMedium")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AvatarThumb")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("CommentSetting")
                        .HasColumnType("INTEGER");

                    b.Property<long>("CrawledCount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<long>("DiggCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DownloadSetting")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DueSetting")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<long>("FollowingAuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("FollwerCount")
                        .HasColumnType("INTEGER");

                    b.Property<long>("FollwingCount")
                        .HasColumnType("INTEGER");

                    b.Property<long>("FreindCount")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Ftc")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Heart")
                        .HasColumnType("INTEGER");

                    b.Property<long>("HeartCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Instagram")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAdVirtual")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsEmbedBanned")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsParsed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<bool>("OpenFavorite")
                        .HasColumnType("INTEGER");

                    b.Property<long>("OrganizationId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Phone")
                        .HasColumnType("TEXT");

                    b.Property<bool>("PrivateAccount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Relation")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SecUid")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Secret")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Signature")
                        .HasColumnType("TEXT");

                    b.Property<int>("StitchSetting")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Telegram")
                        .HasColumnType("TEXT");

                    b.Property<bool>("TtSeller")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UniqueId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Verified")
                        .HasColumnType("INTEGER");

                    b.Property<long>("VideoCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WhatsApp")
                        .HasColumnType("TEXT");

                    b.Property<string>("Youtube")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TiktokAuthors");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.JsVariable.JsFunctionDefinition", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("FunctionBody")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<long>("OrganizationId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "Name")
                        .IsUnique();

                    b.ToTable("JsFunctionDefinitions");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.JsVariable.JsVariableSource", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<long>("OrganizationId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("JsVariableSources");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.ReadingTracker.EmailAnchor", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("FirstVisitDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("InboxEmails")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastVisitDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<string>("OutboxEmail")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("SendingGroupId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("SendingItemId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VisitedCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("EmailAnchors");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.ReadingTracker.EmailVisitHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("IP")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.HasKey("Id");

                    b.ToTable("EmailVisitHistories");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.ReadingTracker.IPInfo", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("City")
                        .HasColumnType("TEXT");

                    b.Property<string>("Country")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("District")
                        .HasColumnType("TEXT");

                    b.Property<string>("IP")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ISP")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Latitude")
                        .HasColumnType("TEXT");

                    b.Property<string>("Longitude")
                        .HasColumnType("TEXT");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<string>("PostalCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Region")
                        .HasColumnType("TEXT");

                    b.Property<string>("TimeZone")
                        .HasColumnType("TEXT");

                    b.Property<string>("UsageType")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("IP");

                    b.ToTable("IPInfos");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.Unsubscribes.UnsubscribeButton", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ButtonHtml")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<long>("OrganizationId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("UnsubscribeButtons");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.Unsubscribes.UnsubscribeEmail", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Host")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<long>("OrganizationId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId", "Email");

                    b.ToTable("UnsubscribeEmails");
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.Unsubscribes.UnsubscribePage", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("HtmlContent")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("_id");

                    b.Property<long>("OrganizationId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("UnsubscribePages");
                });

            modelBuilder.Entity("EmailAnchorEmailVisitHistory", b =>
                {
                    b.HasOne("UZonMailProPlugin.SQL.ReadingTracker.EmailAnchor", null)
                        .WithMany()
                        .HasForeignKey("EmailAnchorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("UZonMailProPlugin.SQL.ReadingTracker.EmailVisitHistory", null)
                        .WithMany()
                        .HasForeignKey("VisitedHistoriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UZonMailProPlugin.SQL.EmailCrawler.CrawlerTaskResult", b =>
                {
                    b.HasOne("UZonMailProPlugin.SQL.EmailCrawler.TiktokAuthor", "TiktokAuthor")
                        .WithMany()
                        .HasForeignKey("TikTokAuthorId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("TiktokAuthor");
                });
#pragma warning restore 612, 618
        }
    }
}
