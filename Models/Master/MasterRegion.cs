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
}