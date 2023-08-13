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
    public enum EnumBankType
    {
        [Display(Name = "BANK")]
        Bank = 1,
        [Display(Name = "KAS")]
        Cash = 2
    }

    public class MasterBank
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kode kas/bank harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Kas/Bank")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "MasterBanks", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
        public string Code { get; set; }

        [Display(Name = "Nama Kas/Bank")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string Name { get; set; }

        [Display(Name = "Jenis")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumBankType Type { get; set; }

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

        [Display(Name = "Atas Nama")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string OnBehalf { get; set; }

        [Display(Name = "No. Rekening")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string AccNumber { get; set; }

        [Display(Name = "Bagan Akun")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int ChartOfAccountId { get; set; }

        [Display(Name = "Bagan Akun")]
        public virtual ChartOfAccount ChartOfAccount { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

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

    public class MasterBankDatalistViewModel
    {
        [Key]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Kode kas/bank harus diisi.")]
        [Display(Name = "Kode Kas/Bank")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Nama Kas/Bank")]
        public string Name { get; set; }

        [DatalistColumn]
        [Display(Name = "Jenis")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumBankType Type { get; set; }

        [DatalistColumn]
        [Display(Name = "Unit Bisnis")]
        public string MasterBusinessUnitName { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [DatalistColumn]
        [Display(Name = "Wilayah")]
        public string MasterRegionCode { get; set; }

        [Display(Name = "Wilayah")]
        [Required(ErrorMessage = "Wilayah harus diisi.")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [DatalistColumn]
        [Display(Name = "Atas Nama")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string OnBehalf { get; set; }

        [DatalistColumn]
        [Display(Name = "No. Rekening")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string AccNumber { get; set; }

        [Display(Name = "Bagan Akun")]
        public int? ChartOfAccountId { get; set; }

        [Display(Name = "Bagan Akun")]
        public virtual ChartOfAccount ChartOfAccount { get; set; }

        [DatalistColumn]
        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }
    }

    public class MasterBankRegionDatalist : MvcDatalist<MasterBankDatalistViewModel>
    {
        private DbContext Context { get; }

        public MasterBankRegionDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.Name;
        }
        public MasterBankRegionDatalist()
        {
            Url = "/DatalistFilters/AllMasterBankRegion";
            Title = "Master Bank";
            AdditionalFilters.Add("MasterBusinessUnitId");
            AdditionalFilters.Add("MasterRegionId");
            AdditionalFilters.Add("Type");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterBankDatalistViewModel> GetModels()
        {
            return Context.Set<MasterBank>()
                .Select(x => new MasterBankDatalistViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Type = x.Type,
                    MasterRegionCode = x.MasterRegion.Notes,
                    MasterRegionId = x.MasterRegionId,
                    MasterRegion = x.MasterRegion,
                    MasterBusinessUnitName = x.MasterBusinessUnit.Name,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    OnBehalf = x.OnBehalf,
                    AccNumber = x.AccNumber,
                    ChartOfAccount = x.ChartOfAccount,
                    Notes = x.Notes,
                    Active = x.Active
                });
        }
    }
}