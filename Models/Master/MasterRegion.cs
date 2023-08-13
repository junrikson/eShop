using Datalist;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;


namespace eShop.Models
{
    public class AssignedRegion
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public bool Assigned { get; set; }
    }

    public class MasterRegion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Kode wilayah harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Wilayah")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "MasterRegions", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Ekspor")]
        public bool IsExport { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }

        public MasterRegion()
        {
            this.ApplicationUsers = new HashSet<ApplicationUser>();
        }
    }

    public class MasterRegionAccount
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Master Wilayah")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Master Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "Bagan Akun")]
        public int ChartOfAccountId { get; set; }

        [Display(Name = "Bagan Akun")]
        public virtual ChartOfAccount ChartOfAccount { get; set; }

        [Display(Name = "Dibuat")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }

    public class MasterRegionDatalist : MvcDatalist<MasterRegion>
    {
        private DbContext Context { get; }

        public MasterRegionDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code;
        }
        public MasterRegionDatalist()
        {
            Url = "AllMasterRegion";
            Title = "Master Wilayah";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterRegion> GetModels()
        {
            return Context.Set<MasterRegion>();
        }
    }

    public class MasterRegionAccountViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Kode Wilayah")]
        public string Code { get; set; }

        [Display(Name = "Akun Awal")]
        public int ChartOfAccountStartId { get; set; }

        [Display(Name = "Akun Awal")]
        public virtual ChartOfAccount ChartOfAccountStart { get; set; }

        [Display(Name = "Akun Akhir")]
        public int ChartOfAccountEndId { get; set; }

        [Display(Name = "Akun Akhir")]
        public virtual ChartOfAccount ChartOfAccountEnd { get; set; }
    }

    public class MasterRegionAccountDatalistViewModel
    {
        [Display(Name = "Bagan Akun")]
        public int Id { get; set; }

        [Display(Name = "Master Wilayah")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Master Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [DatalistColumn]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Nama Akun")]
        public string Name { get; set; }

        [Display(Name = "Bagan Akun")]
        public virtual ChartOfAccount ChartOfAccount { get; set; }

        [DatalistColumn]
        [Display(Name = "Unit Bisnis")]
        public string MasterBusinessUnitName { get; set; }

        [DatalistColumn]
        [Display(Name = "Wilayah")]
        public string MasterRegionCode { get; set; }
    }

    public class MasterRegionAccountDatalist : MvcDatalist<MasterRegionAccountDatalistViewModel>
    {
        private DbContext Context { get; }

        public MasterRegionAccountDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.Name;
        }
        public MasterRegionAccountDatalist()
        {
            Url = "/DatalistFilters/AllMasterRegionAccount";
            Title = "Bagan Akun";
            AdditionalFilters.Add(nameof(ChartOfAccount.MasterBusinessUnitId));
            AdditionalFilters.Add("MasterRegionId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterRegionAccountDatalistViewModel> GetModels()
        {
            return Context.Set<MasterRegionAccount>()
                .Select(x => new MasterRegionAccountDatalistViewModel
                {
                    Id = x.ChartOfAccountId,
                    MasterRegionId = x.MasterRegionId,
                    MasterRegion = x.MasterRegion,
                    MasterBusinessUnitName = x.ChartOfAccount.MasterBusinessUnit.Name,
                    MasterRegionCode = x.MasterRegion.Code,
                    Code = x.ChartOfAccount.Code,
                    Name = x.ChartOfAccount.Name,
                    ChartOfAccount = x.ChartOfAccount
                });
        }
    }
}