using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eShop.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager, string authenticationType)
        {
            if (string.IsNullOrEmpty(authenticationType))
            {
                authenticationType = DefaultAuthenticationTypes.ApplicationCookie;
            }

            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);

            userIdentity.AddClaim(new Claim("ToggleSidebar", this.ToggleSidebar.ToString()));

            userIdentity.AddClaim(new Claim("Skin", Skin == null ? "blue" : this.Skin));

            // Add custom user claims here
            return userIdentity;
        }

        [Display(Name = "Nama Lengkap")]
        [StringLength(256, ErrorMessage = "Maksimal 128 huruf.")]
        public string FullName { get; set; }

        [Display(Name = "Tanggal Lahir")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDay { get; set; }

        [Display(Name = "Pendidikan")]
        [DataType(DataType.MultilineText)]
        public string Education { get; set; }

        [Display(Name = "Alamat")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Toggle Sidebar")]
        public bool? ToggleSidebar { get; set; }

        [Display(Name = "Skin")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string Skin { get; set; }

        [Display(Name = "Otorisasi")]
        public int? AuthorizationId { get; set; }

        [Display(Name = "Otorisasi")]
        public virtual Authorization Authorization { get; set; }

        [Display(Name = "Konsumen")]
        public int? MasterCustomerId { get; set; }

        [Display(Name = "Konsumen")]
        public Boolean? IsCustomer { get; set; }

        public virtual ICollection<MasterBusinessUnit> MasterBusinessUnits { get; set; }

        public virtual ICollection<SystemLog> SystemLogs { get; set; }

        public ApplicationUser()
        {
            this.MasterBusinessUnits = new HashSet<MasterBusinessUnit>();
        }
    }

    public class CustomUserRole : IdentityUserRole<int> { }

    public class CustomUserClaim : IdentityUserClaim<int> { }

    public class CustomUserLogin : IdentityUserLogin<int> { }

    public class CustomRole : IdentityRole<int, CustomUserRole>
    {
        public virtual ICollection<Authorization> Authorizations { get; set; }

        [Required(ErrorMessage = "Nomor urut roles harus diisi.")]
        [Display(Name = "Nomor Urut")]
        public int Order { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }
    }

    public class CustomUserStore : UserStore<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public CustomUserStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public IEnumerable ApplicationUsers { get; internal set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterBusinessUnit> MasterBusinessUnits { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterRegion> MasterRegions { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterItem> MasterItems { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterItemUnits> MasterItemsUnits { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterCategory> MasterCategories { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterUnit> MasterUnits { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterRegionAccount> MasterRegionAccounts { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterCustomer> MasterCustomers { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterSupplier> MasterSuppliers { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterWarehouse> MasterWarehouses { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.PurchaseOrder> PurchaseOrders { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.PurchaseOrderDetails> PurchaseOrdersDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.ChartOfAccount> ChartOfAccounts { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.SystemLog> SystemLogs { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.Authorization> Authorizations { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.Chat> Chats { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.ChatUserOnline> ChatUserOnlines { get; set; }
        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Entity<MasterBusinessUnit>().Property(x => x.PPNRate).HasPrecision(18, 2);
            modelBuilder.Entity<MasterBusinessUnit>().Property(x => x.FCLLoadFee).HasPrecision(18, 2);
            modelBuilder.Entity<MasterBusinessUnit>().Property(x => x.LCLLoadFee).HasPrecision(18, 2);
            modelBuilder.Entity<MasterBusinessUnit>().Property(x => x.EmptyLoadFee).HasPrecision(18, 2);
        }
    }
}