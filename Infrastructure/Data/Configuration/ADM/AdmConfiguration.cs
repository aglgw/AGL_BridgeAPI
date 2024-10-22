using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AGL.Api.Domain.Entities;
using AGL.Api.Domain.Entities.ADM.HTT;
using System.Reflection.Emit;

namespace AGL.Api.Infrastructure.Data.Configuration
{
    #region HTT_Admin
    class adminmemberConfiguration : IEntityTypeConfiguration<admin_member>
    {
        public void Configure(EntityTypeBuilder<admin_member> builder)
        {
            builder.ToTable("admin_member");
            builder.HasKey(e => new { e.adm_seq }).HasName("admin_member_pk");

        }
    }

    class testMasterConfiguration : IEntityTypeConfiguration<test_Master>
    {
        public void Configure(EntityTypeBuilder<test_Master> builder)
        {
            builder.ToTable("test_Master");
            builder.HasKey(e => new { e.idx }).HasName("test_Master_pk");

        }
    }


    class testDetailConfiguration : IEntityTypeConfiguration<test_Detail>
    {
        public void Configure(EntityTypeBuilder<test_Detail> builder)
        {
            builder.ToTable("test_Detail");
            builder.HasKey(e => new { e.idx }).HasName("test_Detail_pk");

        }
    }
    
    class HtUserConfiguration : IEntityTypeConfiguration<ht_user>
    {
        public void Configure(EntityTypeBuilder<ht_user> builder)
        {
            builder.ToTable("ht_user");
            builder.HasKey(e => new { e.idx }).HasName("ht_user_pk");

        }
    }


    class HtSocialUserConfiguration : IEntityTypeConfiguration<ht_social_user>
    {
        public void Configure(EntityTypeBuilder<ht_social_user> builder)
        {
            builder.ToTable("ht_social_user");
            builder.HasKey(e => new { e.idx }).HasName("ht_social_user_pk");

        }
    }

    class HtAddrBookConfiguration : IEntityTypeConfiguration<ht_addr_book>
    {
        public void Configure(EntityTypeBuilder<ht_addr_book> builder)
        {
            builder.ToTable("ht_addr_book");
            builder.HasKey(e => new { e.idx }).HasName("ht_addr_book_pk");

        }
    }

    class HtFollowConfiguration : IEntityTypeConfiguration<ht_follow>
    {
        public void Configure(EntityTypeBuilder<ht_follow> builder)
        {
            builder.ToTable("ht_follow");
            builder.HasKey(e => new { e.idx }).HasName("ht_follow_pk");

        }
    }

    class HtOutUserConfiguration : IEntityTypeConfiguration<ht_out_user>
    {
        public void Configure(EntityTypeBuilder<ht_out_user> builder)
        {
            builder.ToTable("ht_out_user");
            builder.HasKey(e => new { e.idx }).HasName("ht_out_user_pk");

        }
    }

    class HtPointConfiguration : IEntityTypeConfiguration<ht_point>
    {
        public void Configure(EntityTypeBuilder<ht_point> builder)
        {
            builder.ToTable("ht_point");
            builder.HasKey(e => new { e.idx }).HasName("ht_point_pk");

        }
    }

    class HtWishConfiguration : IEntityTypeConfiguration<ht_wish>
    {
        public void Configure(EntityTypeBuilder<ht_wish> builder)
        {
            builder.ToTable("ht_wish");
            builder.HasKey(e => new { e.idx }).HasName("ht_wish_pk");

        }
    }

    class HtWishPushConfiguration : IEntityTypeConfiguration<ht_wish_push>
    {
        public void Configure(EntityTypeBuilder<ht_wish_push> builder)
        {
            builder.ToTable("ht_wish_push");
            builder.HasKey(e => new { e.idx }).HasName("ht_wish_push_pk");

        }
    }


    class HtInqueryConfiguration : IEntityTypeConfiguration<ht_inquery>
    {
        public void Configure(EntityTypeBuilder<ht_inquery> builder)
        {
            builder.ToTable("ht_inquery");
            builder.HasKey(e => new { e.idx }).HasName("ht_inquery_pk");

        }
    }

    class HtInqueryFilesConfiguration : IEntityTypeConfiguration<ht_inquery_files>
    {
        public void Configure(EntityTypeBuilder<ht_inquery_files> builder)
        {
            builder.ToTable("ht_inquery_files");
            builder.HasKey(e => new { e.idx }).HasName("ht_inquery_files_pk");

        }
    }

    class HtInqueryOptionConfiguration : IEntityTypeConfiguration<ht_inquery_option>
    {
        public void Configure(EntityTypeBuilder<ht_inquery_option> builder)
        {
            builder.ToTable("ht_inquery_option");
            builder.HasKey(e => new { e.idx }).HasName("ht_inquery_option_pk");

        }
    }

    class HtEstimateConfiguration : IEntityTypeConfiguration<ht_estimate>
    {
        public void Configure(EntityTypeBuilder<ht_estimate> builder)
        {
            builder.ToTable("ht_estimate");
            builder.HasKey(e => new { e.idx }).HasName("ht_estimate_pk");

        }
    }

    class HtEstimateFilesConfiguration : IEntityTypeConfiguration<ht_estimate_files>
    {
        public void Configure(EntityTypeBuilder<ht_estimate_files> builder)
        {
            builder.ToTable("ht_estimate_files");
            builder.HasKey(e => new { e.idx }).HasName("ht_estimate_files_pk");

        }
    }


    class htbbsnoticeConfiguration : IEntityTypeConfiguration<ht_bbs_notice>
    {
        public void Configure(EntityTypeBuilder<ht_bbs_notice> builder)
        {
            builder.ToTable("ht_bbs_notice");
            builder.HasKey(e => new { e.idx }).HasName("ht_bbs_notice_pk");

        }
    }

    class htbbsnoticelangConfiguration : IEntityTypeConfiguration<ht_bbs_notice_lang>
    {
        public void Configure(EntityTypeBuilder<ht_bbs_notice_lang> builder)
        {
            builder.ToTable("ht_bbs_notice_lang");
            builder.HasKey(e => new { e.idx }).HasName("ht_bbs_notice_lang_pk");

        }
    }

    class htbbsnoticelangfilesConfiguration : IEntityTypeConfiguration<ht_bbs_notice_lang_files>
    {
        public void Configure(EntityTypeBuilder<ht_bbs_notice_lang_files> builder)
        {
            builder.ToTable("ht_bbs_notice_lang_files");
            builder.HasKey(e => new { e.idx }).HasName("ht_bbs_notice_lang_files_pk");

        }
    }

    class htbbsfaqConfiguration : IEntityTypeConfiguration<ht_bbs_faq>
    {
        public void Configure(EntityTypeBuilder<ht_bbs_faq> builder)
        {
            builder.ToTable("ht_bbs_faq");
            builder.HasKey(e => new { e.idx }).HasName("ht_bbs_faq_pk");

        }
    }

    class htbbsfaqlangConfiguration : IEntityTypeConfiguration<ht_bbs_faq_lang>
    {
        public void Configure(EntityTypeBuilder<ht_bbs_faq_lang> builder)
        {
            builder.ToTable("ht_bbs_faq_lang");
            builder.HasKey(e => new { e.idx }).HasName("ht_bbs_faq_lang_pk");

        }
    }

    class htbbsfaqlangfilesConfiguration : IEntityTypeConfiguration<ht_bbs_faq_lang_files>
    {
        public void Configure(EntityTypeBuilder<ht_bbs_faq_lang_files> builder)
        {
            builder.ToTable("ht_bbs_faq_lang_files");
            builder.HasKey(e => new { e.idx }).HasName("ht_bbs_faq_lang_files_pk");

        }
    }

    class htbbsfaqoptionConfiguration : IEntityTypeConfiguration<ht_bbs_faq_option>
    {
        public void Configure(EntityTypeBuilder<ht_bbs_faq_option> builder)
        {
            builder.ToTable("ht_bbs_faq_option");
            builder.HasKey(e => new { e.idx }).HasName("ht_bbs_faq_option_pk");

        }
    }

    class HtaAreeConfiguration : IEntityTypeConfiguration<ht_agree>
    {
        public void Configure(EntityTypeBuilder<ht_agree> builder)
        {
            builder.ToTable("ht_agree");
            builder.HasKey(e => new { e.idx }).HasName("ht_agree_pk");

        }
    }

    class HtAgreeLangConfiguration : IEntityTypeConfiguration<ht_agree_lang>
    {
        public void Configure(EntityTypeBuilder<ht_agree_lang> builder)
        {
            builder.ToTable("ht_agree_lang");
            builder.HasKey(e => new { e.idx }).HasName("ht_agree_lang_pk");

        }
    }
    class HtAgreeKindConfiguration : IEntityTypeConfiguration<ht_agree_kind>
    {
        public void Configure(EntityTypeBuilder<ht_agree_kind> builder)
        {
            builder.ToTable("ht_agree_kind");
            builder.HasKey(e => new { e.idx }).HasName("ht_agree_kind_pk");

        }
    }

    class HtBookingConfiguration : IEntityTypeConfiguration<ht_booking>
    {
        public void Configure(EntityTypeBuilder<ht_booking> builder)
        {
            builder.ToTable("ht_booking");
            builder.HasKey(e => new { e.idx }).HasName("ht_booking_pk");
            builder.HasAlternateKey(e => new { e.booking_number }).HasName("ht_booking_unique");

        }
    }

    class HtBookingTeeConfiguration : IEntityTypeConfiguration<ht_booking_tee>
    {
        public void Configure(EntityTypeBuilder<ht_booking_tee> builder)
        {
            builder.ToTable("ht_booking_tee");
            builder.HasKey(e => new { e.idx }).HasName("ht_booking_tee_pk");
            builder.HasOne(o => o.Booking)
                .WithMany(c => c.BookingTees)
                .HasPrincipalKey(x => x.booking_number)
                .HasForeignKey(o => o.booking_number); 
        }
    }

    class HtBookingTeeOptionConfiguration : IEntityTypeConfiguration<ht_booking_tee_option>
    {
        public void Configure(EntityTypeBuilder<ht_booking_tee_option> builder)
        {
            builder.ToTable("ht_booking_tee_option");
            builder.HasKey(e => new { e.idx }).HasName("ht_booking_tee_option_pk");
        }
    }

    class HtCartConfiguration : IEntityTypeConfiguration<ht_cart>
    {
        public void Configure(EntityTypeBuilder<ht_cart> builder)
        {
            builder.ToTable("ht_cart");
            builder.HasKey(e => new { e.idx }).HasName("ht_cart_pk");

        }
    }

    class HtCartTeeOptionConfiguration : IEntityTypeConfiguration<ht_cart_tee_option>
    {
        public void Configure(EntityTypeBuilder<ht_cart_tee_option> builder)
        {
            builder.ToTable("ht_cart_tee_option");
            builder.HasKey(e => new { e.idx }).HasName("ht_cart_tee_option_pk");

        }
    }

    class HtReviewConfiguration : IEntityTypeConfiguration<ht_review>
    {
        public void Configure(EntityTypeBuilder<ht_review> builder)
        {
            builder.ToTable("ht_review");
            builder.HasKey(e => new { e.idx }).HasName("ht_review_pk");

        }
    }

    class HtReviewLikeConfiguration : IEntityTypeConfiguration<ht_review_like>
    {
        public void Configure(EntityTypeBuilder<ht_review_like> builder)
        {
            builder.ToTable("ht_review_like");
            builder.HasKey(e => new { e.idx }).HasName("ht_review_like_pk");

        }
    }


    class HtReviewFilesConfiguration : IEntityTypeConfiguration<ht_review_files>
    {
        public void Configure(EntityTypeBuilder<ht_review_files> builder)
        {
            builder.ToTable("ht_review_files");
            builder.HasKey(e => new { e.idx }).HasName("ht_review_files_pk");

        }
    }

    class HtLanguageConfiguration : IEntityTypeConfiguration<ht_language>
    {
        public void Configure(EntityTypeBuilder<ht_language> builder)
        {
            builder.ToTable("ht_language");
            builder.HasKey(e => new { e.idx }).HasName("ht_language_pk");

        }
    }

    class HtLanguageFilesConfiguration : IEntityTypeConfiguration<ht_language_files>
    {
        public void Configure(EntityTypeBuilder<ht_language_files> builder)
        {
            builder.ToTable("ht_language_files");
            builder.HasKey(e => new { e.idx }).HasName("ht_language_files_pk");

        }
    }

    class HtCurrencyConfiguration : IEntityTypeConfiguration<ht_currency>
    {
        public void Configure(EntityTypeBuilder<ht_currency> builder)
        {
            builder.ToTable("ht_currency");
            builder.HasKey(e => new { e.idx }).HasName("ht_currency_pk");

        }
    }


    class HtMultiLanguageConfiguration : IEntityTypeConfiguration<ht_multi_language>
    {
        public void Configure(EntityTypeBuilder<ht_multi_language> builder)
        {
            builder.ToTable("ht_multi_language");
            builder.HasKey(e => new { e.idx }).HasName("ht_multi_language_pk");

        }
    }


    class HtMultiLanguageCodeConfiguration : IEntityTypeConfiguration<ht_multi_language_code>
    {
        public void Configure(EntityTypeBuilder<ht_multi_language_code> builder)
        {
            builder.ToTable("ht_multi_language_code");
            builder.HasKey(e => new { e.code_name }).HasName("ht_multi_language_code_pk");


            //builder.
            //builder.HasAlternateKey(e => e.code_name2);
            //builder.HasAlternateKey(e => e.code_name3);


        }
    }

    class HtMultiLanguageNameConfiguration : IEntityTypeConfiguration<ht_multi_language_name>
    {
        public void Configure(EntityTypeBuilder<ht_multi_language_name> builder)
        {
            builder.ToTable("ht_multi_language_name");
            builder.HasKey(e => new { e.code_name, e.language_idx }).HasName("ht_multi_language_name_pk");

        }
    }



    #endregion


}
