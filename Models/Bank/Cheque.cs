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
    public enum EnumChequeType
    {
        [Display(Name = "Giro & Cek Masuk")]
        ChequeIn = 1,
        [Display(Name = "Giro & Cek Keluar")]
        ChequeOut = 2
    }

    public class Cheque
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nomor Giro harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Giro")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "ChequeIns", AdditionalFields = "Id", ErrorMessage = "Nomor ini sudah ada.")]
        public string Code { get; set; }

        [Display(Name = "Tanggal")]
        [Required(ErrorMessage = "Tanggal Giro harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Display(Name = "Jatuh Tempo")]
        [Required(ErrorMessage = "Jatuh Tempo harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Bank Penerbit")]
        [Required(ErrorMessage = "Bank Penerbit harus diisi.")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string Issued { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Jenis")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumChequeType Type { get; set; }

        [Display(Name = "Atas Nama")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string OnBehalf { get; set; }

        [Display(Name = "No. Rekening")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string AccNumber { get; set; }

        [Display(Name = "Jumlah (Rp)")]
        public decimal Ammount { get; set; }

        [Display(Name = "Dialokasikan (Rp)")]
        public decimal Allocated { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }

        [Display(Name = "Cair")]
        public bool Cashed { get; set; }

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

    public class ChequeDatalistViewModel
    {
        [Key]
        public int Id { get; set; }

        [DatalistColumn]
        [Display(Name = "Nomor Giro")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "ChequeIns", AdditionalFields = "Id", ErrorMessage = "Nomor ini sudah ada.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Tanggal")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [DatalistColumn]
        [Display(Name = "Jatuh Tempo")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Bank Penerbit")]
        [Required(ErrorMessage = "Bank Penerbit harus diisi.")]
        public string Issued { get; set; }

        [DatalistColumn]
        [Display(Name = "Unit Bisnis")]
        public string MasterBusinessUnitName { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [DatalistColumn]
        [Display(Name = "Jenis")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumChequeType Type { get; set; }

        [DatalistColumn]
        [Display(Name = "Atas Nama")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string OnBehalf { get; set; }

        [Display(Name = "No. Rekening")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string AccNumber { get; set; }

        [DatalistColumn]
        [Display(Name = "Sisa (Rp)")]
        public decimal Remaining { get; set; }

        [DatalistColumn]
        [Display(Name = "Jumlah (Rp)")]
        public decimal Ammount { get; set; }

        [DatalistColumn]
        [Display(Name = "Dialokasikan (Rp)")]
        public decimal Allocated { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }

        [Display(Name = "Cair")]
        public bool Cashed { get; set; }
    }

    public class ReportListChequesViewModel
    {
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Jenis Laporan")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumReportType ReportType { get; set; }

        [Display(Name = "Jenis Giro")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumChequeType GiroType { get; set; }

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

    public class ChequeDatalist : MvcDatalist<ChequeDatalistViewModel>
    {
        private DbContext Context { get; }

        public ChequeDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code;
        }
        public ChequeDatalist()
        {
            Url = "/DatalistFilters/AllCheque";
            Title = "Giro & Cek";
            AdditionalFilters.Add("MasterBusinessUnitId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<ChequeDatalistViewModel> GetModels()
        {
            return Context.Set<Cheque>()
                .Select(x => new ChequeDatalistViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Date = x.Date,
                    DueDate = x.DueDate,
                    Remaining = x.Ammount - x.Allocated,
                    Ammount = x.Ammount,
                    Allocated = x.Allocated,
                    Issued = x.Issued,
                    Type = x.Type,
                    MasterBusinessUnitName = x.MasterBusinessUnit.Name,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    OnBehalf = x.OnBehalf,
                    AccNumber = x.AccNumber,
                    Notes = x.Notes,
                    Active = x.Active
                });
        }
    }

    public class ChequeUnallocatedDatalist : MvcDatalist<ChequeDatalistViewModel>
    {
        private DbContext Context { get; }

        public ChequeUnallocatedDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - Sisa = Rp " + (model.Ammount - model.Allocated).ToString("N0");
        }
        public ChequeUnallocatedDatalist()
        {
            Url = "/DatalistFilters/AllChequeUnallocated";
            Title = "Giro & Cek";
            AdditionalFilters.Add("MasterBusinessUnitId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<ChequeDatalistViewModel> GetModels()
        {
            return Context.Set<Cheque>().Where(x => x.Ammount > x.Allocated)
                .Select(x => new ChequeDatalistViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Date = x.Date,
                    DueDate = x.DueDate,
                    Remaining = x.Ammount - x.Allocated,
                    Ammount = x.Ammount,
                    Allocated = x.Allocated,
                    Issued = x.Issued,
                    Type = x.Type,
                    MasterBusinessUnitName = x.MasterBusinessUnit.Name,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    OnBehalf = x.OnBehalf,
                    AccNumber = x.AccNumber,
                    Notes = x.Notes,
                    Active = x.Active
                });
        }
    }

    public class ChequeCashedDatalist : MvcDatalist<ChequeDatalistViewModel>
    {
        private DbContext Context { get; }

        public ChequeCashedDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code;
        }
        public ChequeCashedDatalist()
        {
            Url = "/DatalistFilters/AllChequeCashed";
            Title = "Giro & Cek";
            AdditionalFilters.Add("MasterBusinessUnitId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<ChequeDatalistViewModel> GetModels()
        {
            return Context.Set<Cheque>().Where(x => x.Ammount <= x.Allocated && x.Cashed != true)
                .Select(x => new ChequeDatalistViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Date = x.Date,
                    DueDate = x.DueDate,
                    Remaining = x.Ammount - x.Allocated,
                    Ammount = x.Ammount,
                    Allocated = x.Allocated,
                    Issued = x.Issued,
                    Type = x.Type,
                    MasterBusinessUnitName = x.MasterBusinessUnit.Name,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    OnBehalf = x.OnBehalf,
                    AccNumber = x.AccNumber,
                    Notes = x.Notes,
                    Active = x.Active
                });
        }
    }
}