﻿using Microsoft.AspNet.Identity;
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
        public virtual MasterCustomer MasterCustomer { get; set; }

        [Display(Name = "Konsumen")]
        public Boolean? IsCustomer { get; set; }

        public virtual ICollection<MasterBusinessUnit> MasterBusinessUnits { get; set; }

        public virtual ICollection<MasterRegion> MasterRegions { get; set; }

        public virtual ICollection<SystemLog> SystemLogs { get; set; }

        public ApplicationUser()
        {
            this.MasterBusinessUnits = new HashSet<MasterBusinessUnit>();
            this.MasterRegions = new HashSet<MasterRegion>();
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
        public System.Data.Entity.DbSet<eShop.Models.MasterBusinessUnitAccount> MasterBusinessUnitsAccounts { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterRegion> MasterRegions { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterRegionAccount> MasterRegionAccounts { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterCurrency> MasterCurrencies{ get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterItem> MasterItems { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterItemUnit> MasterItemUnits { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterCategory> MasterCategories { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterUnit> MasterUnits { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterBrand> MasterBrands { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterCost> MasterCosts { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterCustomer> MasterCustomers { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterSupplier> MasterSuppliers { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterWarehouse> MasterWarehouses { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterSalesPerson> MasterSalesPersons { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.StockAdjustment> StockAdjustments { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.StockAdjustmentDetails> StockAdjustmentsDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.WarehouseTransfer> WarehouseTransfers { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.WarehouseTransferDetails> WarehouseTransfersDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.StockCard> StockCards { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.GiroCheque> GiroCheques { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.Cheque> Cheques { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.MasterCashBank> MasterCashBanks { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.CashBankTransaction> CashBankTransactions { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.CashBankTransactionDetailsHeader> CashBankTransactionsDetailsHeader { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.CashBankTransactionDetails> CashBankTransactionsDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.PurchaseOrder> PurchaseOrders { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.PurchaseOrderDetails> PurchaseOrdersDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.Purchase> Purchases { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.PurchaseDetails> PurchasesDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.PurchaseRequest> PurchaseRequests { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.PurchaseRequestDetails> PurchaseRequestsDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.PurchaseReturn> PurchaseReturns { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.PurchaseReturnDetails> PurchaseReturnsDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.SalesRequest> SalesRequests { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.SalesRequestDetails> SalesRequestsDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.SalesOrder> SalesOrders { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.SalesOrderDetails> SalesOrdersDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.SalesReturn> SalesReturns { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.SalesReturnDetails> SalesReturnsDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.Sale> Sales { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.SaleDetails> SalesDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.CreditNote> CreditNotes { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.CreditNoteDetails> CreditNotesDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.Repayment> Repayments { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.RepaymentDetailsHeader> RepaymentsDetailsHeader { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.RepaymentDetails> RepaymentsDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.ChartOfAccount> ChartOfAccounts { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.AccountBallance> AccountBallances { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.Journal> Journals { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.JournalDetails> JournalsDetails { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.SystemLog> SystemLogs { get; set; }
        public System.Data.Entity.DbSet<eShop.Models.VerificationHistory> VerificationHistories { get; set; }
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
            modelBuilder.Entity<MasterCurrency>().Property(x => x.Rate).HasPrecision(18, 10);
            modelBuilder.Entity<PurchaseRequest>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseRequest>().Property(x => x.Rate).HasPrecision(18, 10);
            modelBuilder.Entity<PurchaseRequestDetails>().Property(x => x.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseRequestDetails>().Property(x => x.Price).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseRequestDetails>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrder>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrder>().Property(x => x.Rate).HasPrecision(18, 10);
            modelBuilder.Entity<PurchaseOrderDetails>().Property(x => x.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrderDetails>().Property(x => x.Price).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrderDetails>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<Purchase>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<Purchase>().Property(x => x.Rate).HasPrecision(18, 10);
            modelBuilder.Entity<PurchaseDetails>().Property(x => x.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseDetails>().Property(x => x.Price).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseDetails>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseReturn>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseReturn>().Property(x => x.Rate).HasPrecision(18, 10);
            modelBuilder.Entity<PurchaseReturnDetails>().Property(x => x.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseReturnDetails>().Property(x => x.Price).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseReturnDetails>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<SalesRequest>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<SalesRequest>().Property(x => x.Rate).HasPrecision(18, 10);
            modelBuilder.Entity<SalesRequestDetails>().Property(x => x.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<SalesRequestDetails>().Property(x => x.Price).HasPrecision(18, 2);
            modelBuilder.Entity<SalesRequestDetails>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<SalesOrder>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<SalesOrder>().Property(x => x.Rate).HasPrecision(18, 10);
            modelBuilder.Entity<SalesOrderDetails>().Property(x => x.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<SalesOrderDetails>().Property(x => x.Price).HasPrecision(18, 2);
            modelBuilder.Entity<SalesOrderDetails>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<Sale>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<Sale>().Property(x => x.Rate).HasPrecision(18, 10);
            modelBuilder.Entity<SaleDetails>().Property(x => x.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<SaleDetails>().Property(x => x.Price).HasPrecision(18, 2);
            modelBuilder.Entity<SaleDetails>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<SalesReturn>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<SalesReturn>().Property(x => x.Rate).HasPrecision(18, 10);
            modelBuilder.Entity<SalesReturnDetails>().Property(x => x.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<SalesReturnDetails>().Property(x => x.Price).HasPrecision(18, 2);
            modelBuilder.Entity<SalesReturnDetails>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<CreditNote>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<CreditNoteDetails>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<Repayment>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<RepaymentDetailsHeader>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<RepaymentDetails>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<Cheque>().Property(x => x.Ammount).HasPrecision(18, 2);
            modelBuilder.Entity<Cheque>().Property(x => x.Allocated).HasPrecision(18, 2);
            modelBuilder.Entity<CashBankTransaction>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<CashBankTransactionDetails>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<CashBankTransactionDetailsHeader>().Property(x => x.Total).HasPrecision(18, 2);
            modelBuilder.Entity<AccountBallance>().Property(x => x.BeginningBallance).HasPrecision(18, 2);
            modelBuilder.Entity<JournalDetails>().Property(x => x.Debit).HasPrecision(18, 2);
            modelBuilder.Entity<JournalDetails>().Property(x => x.Credit).HasPrecision(18, 2);
            modelBuilder.Entity<Journal>().Property(x => x.Debit).HasPrecision(18, 2);
            modelBuilder.Entity<Journal>().Property(x => x.Credit).HasPrecision(18, 2);
        }
    }
}