using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace eShop.Models
{
    public enum EnumJournalType
    {
        [Display(Name = "JURNAL UMUM")]
        General = 1,
        [Display(Name = "PENJUALAN")]
        Sales = 2,
        [Display(Name = "RETUR PENJUALAN")]
        SalesReturn = 3,
        [Display(Name = "PEMBELIAN")]
        Purchase = 4,
        [Display(Name = "RETUR PEMBELIAN")]
        PurchaseReturn = 5,
        [Display(Name = "KAS / BANK")]
        BankTransaction = 6,
        [Display(Name = "NOTA KREDIT")]
        SalesAdvance = 7,
        [Display(Name = "PELUNASAN PIUTANG")]
        Repayment = 8,
        [Display(Name = "NOTA DEBIT")]
        DebitNote = 9,
        [Display(Name = "PEMBAYARAN HUTANG")]
        Payment = 10,
        [Display(Name = "PENCAIRAN GIRO")]
        GiroChequeCashOut = 11,
        [Display(Name = "PENYESUAIAN STOK")]
        StockAdjustment = 12
    }

    public enum EnumReportType
    {
        [Display(Name = "PDF")]
        Pdf = 1,
        [Display(Name = "EXCEL")]
        Excel = 2
    }

    public class Journal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kode jurnal harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Jurnal")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "Journals", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
        public string Code { get; set; }

        [Display(Name = "Tanggal Jurnal")]
        [Required(ErrorMessage = "Tanggal jurnal harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Display(Name = "Unit Bisnis")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Jenis Jurnal")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumJournalType Type { get; set; }

        [Display(Name = "Debet")]
        public decimal Debit { get; set; }

        [Display(Name = "Kredit")]
        public decimal Credit { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Pembelian")]
        public int? PurchaseId { get; set; }

        [Display(Name = "Pembelian")]
        public virtual Purchase Purchase { get; set; }

        [Display(Name = "Penjualan")]
        public int? SaleId { get; set; }

        [Display(Name = "Penjualan")]
        public virtual Sale Sale { get; set; }

        [Display(Name = "Uang Muka Penjualan")]
        public int? SalesAdvanceId { get; set; }

        [Display(Name = "Uang Muka Penjualan")]
        public virtual SalesAdvance SalesAdvance { get; set; }

        [Display(Name = "Kas/Bank")]
        public int? BankTransactionId { get; set; }

        [Display(Name = "Kas/Bank")]
        public virtual BankTransaction BankTransaction { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }

        [Display(Name = "Dibuat")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Display(Name = "Diubah")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Updated { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }

    public class JournalDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Jurnal")]
        [Required(ErrorMessage = "Transaksi Bank harus diisi.")]
        public int JournalId { get; set; }

        [Display(Name = "Jurnal")]
        public virtual Journal Journal { get; set; }

        [Display(Name = "Bagan Akun")]
        [Required(ErrorMessage = "Bagan Akun harus diisi.")]
        public int ChartOfAccountId { get; set; }

        [Display(Name = "Bagan Akun")]
        public virtual ChartOfAccount ChartOfAccount { get; set; }

        [Display(Name = "Wilayah")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [Display(Name = "Debet")]
        public decimal Debit { get; set; }

        [Display(Name = "Kredit")]
        public decimal Credit { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Aktif")]
        public bool isMerged { get; set; }

        [Display(Name = "Detail Pembelian")]
        public int? PurchaseDetailsId { get; set; }

        [Display(Name = "Detail Pembelian")]
        public virtual PurchaseDetails PurchaseDetails { get; set; }

        [Display(Name = "Detail Penjualan")]
        public int? SaleDetailsId { get; set; }

        [Display(Name = "Detail Penjualan")]
        public virtual SaleDetails SaleDetails { get; set; }

        [Display(Name = "Detail Uang Muka Penjualan")]
        public int? SalesAdvanceDetailsId { get; set; }

        [Display(Name = "Detail Uang Muka Penjualan")]
        public virtual SalesAdvanceDetails SalesAdvanceDetails { get; set; }

        [Display(Name = "Detail Kas/Bank")]
        public int? BankTransactionDetailsId { get; set; }

        [Display(Name = "Detail Kas/Bank")]
        public virtual BankTransactionDetails BankTransactionDetails { get; set; }

        [Display(Name = "Detail Header as/Bank")]
        public int? BankTransactionDetailsHeaderId { get; set; }

        [Display(Name = "Detail Header Kas/Bank")]
        public virtual BankTransactionDetailsHeader BankTransactionDetailsHeader { get; set; }

        [Display(Name = "Dibuat")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Display(Name = "Diubah")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Updated { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }

    public class ReportJournalsByDatesViewModel
    {
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Jenis Jurnal")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumJournalType? Type { get; set; }

        [Display(Name = "Tanggal Awal")]
        [Required(ErrorMessage = "Tanggal Awal harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Tanggal Akhir")]
        [Required(ErrorMessage = "Tanggal Akhir harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }
    }

    public class ReportLedgersViewModel
    {
        [Display(Name = "Unit Bisnis")]
        public int? MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Wilayah")]
        public int? MasterRegionId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterRegion MasterRegion { get; set; }

        [Display(Name = "Bagan Akun")]
        [Required(ErrorMessage = "Bagan Akun harus diisi.")]
        public int ChartOfAccountId { get; set; }

        [Display(Name = "Bagan Akun")]
        public virtual ChartOfAccount ChartOfAccount { get; set; }

        [Display(Name = "Tanggal Awal")]
        [Required(ErrorMessage = "Tanggal Awal harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Tanggal Akhir")]
        [Required(ErrorMessage = "Tanggal Akhir harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }
    }

    public class ReportTrialBallanceViewModel
    {
        [Display(Name = "Bulan")]
        [Required(ErrorMessage = "Bulan harus diisi.")]
        public int Month { get; set; }

        [Display(Name = "Year")]
        [Required(ErrorMessage = "Year harus diisi.")]
        public int Year { get; set; }

        public int StartYear { get; set; }

        public int EndYear { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }
    }

    public class ReportBallanceSheetViewModel
    {
        [Display(Name = "Bulan")]
        [Required(ErrorMessage = "Bulan harus diisi.")]
        public int Month { get; set; }

        [Display(Name = "Year")]
        [Required(ErrorMessage = "Year harus diisi.")]
        public int Year { get; set; }

        public int StartYear { get; set; }

        public int EndYear { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }
    }

    public class ReportIncomeStatementViewModel
    {
        [Display(Name = "Bulan")]
        [Required(ErrorMessage = "Bulan harus diisi.")]
        public int Month { get; set; }

        [Display(Name = "Year")]
        [Required(ErrorMessage = "Year harus diisi.")]
        public int Year { get; set; }

        public int StartYear { get; set; }

        public int EndYear { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }
    }

    public class ReportCashFlowStatementViewModel
    {
        [Display(Name = "Bulan")]
        [Required(ErrorMessage = "Bulan harus diisi.")]
        public int Month { get; set; }

        [Display(Name = "Year")]
        [Required(ErrorMessage = "Year harus diisi.")]
        public int Year { get; set; }

        public int StartYear { get; set; }

        public int EndYear { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }
    }
}