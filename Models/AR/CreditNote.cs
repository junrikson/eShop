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
    public enum EnumCreditNoteType
    {
        [Display(Name = "TRANSFER")]
        Bank = 1,
        [Display(Name = "CASH")]
        Cash = 2,
        [Display(Name = "GIRO/CEK")]
        Cheque = 3,
        [Display(Name = "BIAYA")]
        MasterCost = 4
    }
    public class CreditNote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor Nota Kredit harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Nota Kredit")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "CreditNotes", AdditionalFields = "Id", ErrorMessage = "Nomor Nota Kredit ini sudah dipakai.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Tanggal")]
        [Required(ErrorMessage = "Tanggal harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

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

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Dipakai")]
        public decimal Allocated { get; set; }

        [Display(Name = "Total")]
        public decimal Total { get; set; }

        [Display(Name = "Print")]
        public bool IsPrint { get; set; }

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

    public class CreditNoteDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Nota Kredit")]
        [Required(ErrorMessage = "Invoice harus diisi.")]
        public int CreditNoteId { get; set; }

        [Display(Name = "Nota Kredit")]
        public virtual CreditNote CreditNote { get; set; }

        [Display(Name = "Jenis")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumCreditNoteType Type { get; set; }

        [Display(Name = "Master Kas/Bank")]
        public int? MasterCashBankId { get; set; }

        [Display(Name = "Master Kas/Bank")]
        public virtual MasterCashBank MasterCashBank { get; set; }

        [Display(Name = "Master Biaya")]
        public int? MasterCostId { get; set; }

        [Display(Name = "Master Biaya")]
        public virtual MasterCost MasterCost { get; set; }

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

        [Display(Name = "Nilai")]
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

    public class CreditNoteDatalistViewModel
    {
        [Key]
        public int Id { get; set; }

        [DatalistColumn]
        [Display(Name = "Unit Bisnis")]
        public string MasterBusinessUnitName { get; set; }

        [Display(Name = "Unit Bisnis")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [DatalistColumn]
        [Display(Name = "Wilayah")]
        public string MasterRegionName { get; set; }

        [Display(Name = "Wilayah")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [DatalistColumn]
        [Display(Name = "Nomor Nota Kredit")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Sisa (Rp)")]
        public decimal Remaining { get; set; }

        [DatalistColumn]
        [Display(Name = "Dipakai (Rp)")]
        public decimal Allocated { get; set; }

        [DatalistColumn]
        [Display(Name = "Total (Rp)")]
        public decimal Total { get; set; }

        [DatalistColumn]
        [Display(Name = "Tanggal")]
        [Required(ErrorMessage = "Tanggal harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [DatalistColumn]
        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }
    }

    public class CreditNoteDatalist : MvcDatalist<CreditNoteDatalistViewModel>
    {
        private DbContext Context { get; }

        public CreditNoteDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code;
        }
        public CreditNoteDatalist()
        {
            Url = "/DatalistFilters/AllCreditNote";
            Title = "Nota Kredit";
            AdditionalFilters.Add("MasterBusinessUnitId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<CreditNoteDatalistViewModel> GetModels()
        {
            return Context.Set<CreditNote>()
                .Select(x => new CreditNoteDatalistViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Date = x.Date,
                    Remaining = x.Total - x.Allocated,
                    Total = x.Total,
                    Allocated = x.Allocated,
                    MasterBusinessUnitName = x.MasterBusinessUnit.Name,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    MasterRegionName = x.MasterRegion.Notes,
                    MasterRegionId = x.MasterRegionId,
                    MasterRegion = x.MasterRegion,
                    Notes = x.Notes,
                    Active = x.Active
                });
        }
    }

    public class CreditNoteUnallocatedDatalist : MvcDatalist<CreditNoteDatalistViewModel>
    {
        private DbContext Context { get; }

        public CreditNoteUnallocatedDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code;
        }
        public CreditNoteUnallocatedDatalist()
        {
            Url = "/DatalistFilters/AllCreditNoteUnallocated";
            Title = "Nota Kredit";
            AdditionalFilters.Add("MasterBusinessUnitId");
            AdditionalFilters.Add("MasterRegionId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<CreditNoteDatalistViewModel> GetModels()
        {
            return Context.Set<CreditNote>().Where(x => x.Total > x.Allocated)
                .Select(x => new CreditNoteDatalistViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Date = x.Date,
                    Remaining = x.Total - x.Allocated,
                    Total = x.Total,
                    Allocated = x.Allocated,
                    MasterBusinessUnitName = x.MasterBusinessUnit.Name,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    MasterRegionName = x.MasterRegion.Notes,
                    MasterRegionId = x.MasterRegionId,
                    MasterRegion = x.MasterRegion,
                    Notes = x.Notes,
                    Active = x.Active
                });
        }
    }
}