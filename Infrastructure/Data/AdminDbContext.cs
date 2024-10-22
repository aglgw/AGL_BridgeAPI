using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.Domain.Entities;
using AGL.Api.Domain.Entities.ADM.HTT;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;


namespace AGL.Api.Infrastructure.Data
{
    public class AdminContext : DbContext
    {
        
        private readonly ILogger<AdminContext> _logger;
        public AdminContext(
            DbContextOptions<AdminContext> options,
            
            ILogger<AdminContext> logger = null) : base(options)
        {
            
            _logger = logger;
        }

        public DbSet<admin_member> admin_member { get; set; }
        public DbSet<test_Master> test_Master { get; set; }
        public DbSet<test_Detail> test_Detail { get; set; }


        #region HTT_Admin
        public DbSet<ht_user> ht_user { get; set; }
        public DbSet<ht_social_user> ht_social_user { get; set; }
        public DbSet<ht_addr_book> ht_addr_book { get; set; }
        public DbSet<ht_follow> ht_follow { get; set; }
        public DbSet<ht_out_user> ht_out_user { get; set; }
        public DbSet<ht_point> ht_point { get; set; }
        public DbSet<ht_wish> ht_wish { get; set; }
        public DbSet<ht_wish_push> ht_wish_push { get; set; }

        public DbSet<ht_inquery> ht_inquery { get; set; }
        public DbSet<ht_inquery_files> ht_inquery_files { get; set; }
        public DbSet<ht_inquery_option> ht_inquery_option { get; set; }

        public DbSet<ht_estimate> ht_estimate { get; set; }
        public DbSet<ht_estimate_files> ht_estimate_files { get; set; }
        public DbSet<ht_booking> ht_booking { get; set; }
        public DbSet<ht_booking_tee> ht_booking_tee { get; set; }
        public DbSet<ht_booking_tee_option> ht_booking_tee_option { get; set; }
        public DbSet<ht_cart> ht_cart { get; set; }
        public DbSet<ht_cart_tee_option> ht_cart_tee_option { get; set; }
        public DbSet<ht_review> ht_review { get; set; }
        public DbSet<ht_review_like> ht_review_like { get; set; }
        public DbSet<ht_review_files> ht_review_files { get; set; }

        public DbSet<ht_bbs_notice> ht_bbs_notice { get; set; }
        public DbSet<ht_bbs_notice_lang> ht_bbs_notice_lang { get; set; }
        public DbSet<ht_bbs_notice_lang_files> ht_bbs_notice_lang_files { get; set; }
        public DbSet<ht_bbs_faq> ht_bbs_faq { get; set; }
        public DbSet<ht_bbs_faq_lang> ht_bbs_faq_lang { get; set; }
        public DbSet<ht_bbs_faq_lang_files> ht_bbs_faq_lang_files { get; set; }
        public DbSet<ht_bbs_faq_option> ht_bbs_faq_option { get; set; }
        public DbSet<ht_agree> ht_agree { get; set; }
        public DbSet<ht_agree_lang> ht_agree_lang { get; set; }
        public DbSet<ht_agree_kind> ht_agree_kind { get; set; }

        public DbSet<ht_sendemail> ht_sendemail { get; set; }
        public DbSet<ht_visit> ht_visit { get; set; }

        public DbSet<ht_language> ht_language { get; set; }
        public DbSet<ht_language_files> ht_language_files { get; set; }
        public DbSet<ht_currency> ht_currency { get; set; }

        public DbSet<ht_multi_language> ht_multi_language { get; set; }
        public DbSet<ht_multi_language_code> ht_multi_language_code { get; set; }
        public DbSet<ht_multi_language_name> ht_multi_language_name { get; set; }


        #endregion


        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
