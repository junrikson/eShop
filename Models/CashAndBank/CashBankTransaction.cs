using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace eShop.Models
{
    public enum EnumCashBankTransactionType
    {
        [Display(Name = "MASUK")]
        In = 1,
        [Display(Name = "KELUAR")]
        Out = 2
    }

    public class CashBankTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kode Transaksi harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Transaksi")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "CashBankIns", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
        public string Code { get; set; }

        [Display(Name = "Tanggal Transaksi")]
        [Required(ErrorMessage = "Tanggal Transaksi harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Display(Name = "Jenis Transaksi")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumCashBankTransactionType TransactionType { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Wilayah")]
        [Required(ErrorMessage = "Wilayah harus diisi.")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [Display(Name = "Total (Rp)")]
        public decimal Total { get; set; }

        [Display(Name = "Keterangan")]
        public string Notes { get; set; }

        [Display(Name = "Print")]
        public bool IsPrint { get; set; }

        [Display(Name = "Kode Verifikasi")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string Verification { get; set; }

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

    public class CashBankTransactionDetailsHeader
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Transaksi Bank")]
        [Required(ErrorMessage = "Transaksi Bank harus diisi.")]
        public int CashBankTransactionId { get; set; }

        [Display(Name = "Transaksi Bank")]
        public virtual CashBankTransaction BankTransaction { get; set; }

        [Display(Name = "Jenis Transaksi")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumCashBankType Type { get; set; }

        [Required(ErrorMessage = "Transaksi Bank harus diisi.")]
        [Display(Name = "Kas / Bank")]
        public int MasterCashBankId { get; set; }

        [Display(Name = "Kas / Bank")]
        public virtual MasterCashBank MasterBank { get; set; }

        [Display(Name = "Giro / Cek")]
        public int? ChequeId { get; set; }

        [Display(Name = "Giro / Cek")]
        public virtual Cheque Cheque { get; set; }

        [Display(Name = "Total (Rp)")]
        public decimal Total { get; set; }

        [AllowHtml]
        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

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

    public class CashBankTransactionDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Transaksi Bank")]
        [Required(ErrorMessage = "Transaksi Bank harus diisi.")]
        public int CashBankTransactionId { get; set; }

        [Display(Name = "Transaksi Bank")]
        public virtual CashBankTransaction CashBankTransaction { get; set; }

        [Display(Name = "Jenis Transaksi")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumCashBankType Type { get; set; }

        [Display(Name = "Kas / Bank")]
        public int MasterCashBankId { get; set; }

        [Display(Name = "Kas / Bank")]
        public virtual MasterCashBank MasterCashBank { get; set; }

        [Display(Name = "Giro / Cek")]
        [StringLength(256, ErrorMessage = "Maksimal 256 huruf.")]
        public string GiroCheque { get; set; }

        [Display(Name = "Jatuh Tempo")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Bagan Akun")]
        [Required(ErrorMessage = "Bagan Akun harus diisi.")]
        public int ChartOfAccountId { get; set; }

        [Display(Name = "Bagan Akun")]
        public virtual ChartOfAccount ChartOfAccount { get; set; }

        [Display(Name = "Total (Rp)")]
        public decimal Total { get; set; }

        [AllowHtml]
        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

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
}