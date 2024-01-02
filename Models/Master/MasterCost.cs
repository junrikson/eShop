using Datalist;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace eShop.Models
{
    public class MasterCost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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

        [Required(ErrorMessage = "Kode Biaya harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Biaya")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "MasterCosts", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
        public string Code { get; set; }

        [Display(Name = "Nama Biaya")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string Name { get; set; }

        [Display(Name = "Nomor Akun")]
        [Required(ErrorMessage = "Nomor Akun harus diisi.")]
        public int ChartOfAccountId { get; set; }

        [Display(Name = "Nomor Akun")]
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

    public class MasterCostDatalistViewModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [DatalistColumn]
        [Display(Name = "Unit Bisnis")]
        public string MasterBusinessUnitCode { get; set; }

        [Display(Name = "Wilayah")]
        [Required(ErrorMessage = "Wilayah harus diisi.")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [DatalistColumn]
        [Display(Name = "Wilayah")]
        public string MasterRegionCode { get; set; }

        [DatalistColumn]
        [Display(Name = "Kode Biaya")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Nama Biaya")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string Name { get; set; }

        [DatalistColumn]
        [Display(Name = "Nomor Akun")]
        public string ChartOfAccountCode { get; set; }

        [DatalistColumn]
        [Display(Name = "Nama Akun")]
        public string ChartOfAccountName { get; set; }

        [Display(Name = "Nomor Akun")]
        [Required(ErrorMessage = "Nomor Akun harus diisi.")]
        public int ChartOfAccountId { get; set; }

        [Display(Name = "Nomor Akun")]
        public virtual ChartOfAccount ChartOfAccount { get; set; }

        [DatalistColumn]
        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }
    }

    public class MasterCostDatalist : MvcDatalist<MasterCostDatalistViewModel>
    {
        private DbContext Context { get; }

        public MasterCostDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.Name;
        }
        public MasterCostDatalist()
        {
            Url = "/DatalistFilters/AllMasterCost";
            Title = "Master Biaya";
            AdditionalFilters.Add("MasterBusinessUnitId");
            AdditionalFilters.Add("MasterRegionId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterCostDatalistViewModel> GetModels()
        {
            return Context.Set<MasterCost>()
                .Select(x => new MasterCostDatalistViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    MasterBusinessUnitCode = x.MasterBusinessUnit.Code,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterRegionCode = x.MasterRegion.Code,
                    MasterRegionId = x.MasterRegionId,
                    MasterRegion = x.MasterRegion,
                    Name = x.Name,
                    ChartOfAccount = x.ChartOfAccount,
                    ChartOfAccountId = x.ChartOfAccountId,
                    ChartOfAccountName = x.ChartOfAccount.Name,
                    ChartOfAccountCode = x.ChartOfAccount.Code,
                    Notes = x.Notes,
                    Active = x.Active
                });
        }
    }

    public class MasterCostUnitBusinessDatalist : MvcDatalist<MasterCostDatalistViewModel>
    {
        private DbContext Context { get; }

        public MasterCostUnitBusinessDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.Name;
        }
        public MasterCostUnitBusinessDatalist()
        {
            Url = "/DatalistFilters/AllMasterCostUnitBusiness";
            Title = "Master Biaya";
            AdditionalFilters.Add("MasterBusinessUnitId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterCostDatalistViewModel> GetModels()
        {
            return Context.Set<MasterCost>()
                .Select(x => new MasterCostDatalistViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    MasterRegionCode = x.MasterRegion.Code,
                    MasterRegionId = x.MasterRegionId,
                    MasterRegion = x.MasterRegion,
                    Name = x.Name,
                    ChartOfAccount = x.ChartOfAccount,
                    ChartOfAccountId = x.ChartOfAccountId,
                    ChartOfAccountName = x.ChartOfAccount.Name,
                    ChartOfAccountCode = x.ChartOfAccount.Code,
                    Notes = x.Notes,
                    Active = x.Active
                });
        }
    }
}