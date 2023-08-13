using Datalist;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace eShop.Models
{
    public enum EnumRepaymentType
    {
        [Display(Name = "TRANSFER")]
        Bank = 1,
        [Display(Name = "CASH")]
        Cash = 2,
        [Display(Name = "GIRO/CEK")]
        GiroCheque = 3,
        [Display(Name = "NOTA KREDIT")]
        CreditNote = 4,
        [Display(Name = "BIAYA")]
        MasterCost = 5
    }

    public class Repayment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor Pelunasan harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Pelunasan")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "Repayments", AdditionalFields = "Id", ErrorMessage = "Nomor Pelunasan ini sudah dipakai.")]
        public string Code { get; set; }

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

        [DatalistColumn]
        [Display(Name = "Tanggal")]
        [Required(ErrorMessage = "Tanggal harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [DatalistColumn]
        [Display(Name = "Jumlah (Rp)")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
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

    public class RepaymentDetailsHeader
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Pelunasan")]
        [Required(ErrorMessage = "Invoice harus diisi.")]
        public int RepaymentId { get; set; }

        [Display(Name = "Pelunasan")]
        public virtual Repayment Repayment { get; set; }

        [Display(Name = "Jenis")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumRepaymentType Type { get; set; }

        [Display(Name = "Tanggal Transaksi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "Master Bank")]
        public int? MasterCashBankId { get; set; }

        [Display(Name = "Master Bank")]
        public virtual MasterCashBank MasterBank { get; set; }

        [Display(Name = "Master Biaya")]
        public int? MasterCostId { get; set; }

        [Display(Name = "Master Biaya")]
        public virtual MasterCost MasterCost { get; set; }

        [Display(Name = "Nota Kredit")]
        public int? CreditNoteId { get; set; }

        [Display(Name = "Nota Kredit")]
        public virtual CreditNote CreditNote { get; set; }

        [Display(Name = "Giro / Cek")]
        public int? ChequeId { get; set; }

        [Display(Name = "Giro / Cek")]
        public virtual Cheque Cheque { get; set; }

        [Display(Name = "Referensi")]
        [StringLength(256, ErrorMessage = "Maksimal 256 huruf.")]
        public string Refference { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Nilai (Rp)")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Total { get; set; }

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

    public class RepaymentDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Pelunasan")]
        [Required(ErrorMessage = "Pelunasan harus diisi.")]
        public int RepaymentId { get; set; }

        [Display(Name = "Pelunasan")]
        public virtual Repayment Repayment { get; set; }

        [Display(Name = "Jenis")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumRepaymentType Type { get; set; }

        [Display(Name = "Invoice")]
        [Required(ErrorMessage = "Invoice harus diisi.")]
        public int SaleId { get; set; }

        [Display(Name = "Invoice")]
        public virtual Sale Sale { get; set; }

        [Display(Name = "Kas/Bank")]
        public int? MasterCashBankId { get; set; }

        [Display(Name = "Kas/Bank")]
        public virtual MasterCashBank MasterCashBank { get; set; }

        [Display(Name = "Biaya")]
        public int? MasterCostId { get; set; }

        [Display(Name = "Biaya")]
        public virtual MasterCost MasterCost { get; set; }

        [Display(Name = "Nota Kredit")]
        public int? CreditNoteId { get; set; }

        [Display(Name = "Nota Kredit")]
        public virtual CreditNote CreditNote { get; set; }

        [Display(Name = "Giro / Cek")]
        public int? ChequeId { get; set; }

        [Display(Name = "Giro / Cek")]
        public virtual Cheque Cheque { get; set; }

        [Display(Name = "Referensi")]
        [StringLength(256, ErrorMessage = "Maksimal 256 huruf.")]
        public string Refference { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Nilai (Rp)")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Total { get; set; }

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

    public class RepaymentDatalist : MvcDatalist<Repayment>
    {
        private DbContext Context { get; }

        public RepaymentDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code;
        }
        public RepaymentDatalist()
        {
            Url = "/DatalistFilters/AllRepayment";
            Title = "Pelunasan";
            AdditionalFilters.Add("MasterBusinessUnitId");
            AdditionalFilters.Add("MasterRegionId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<Repayment> GetModels()
        {
            return Context.Set<Repayment>();
        }
    }
}