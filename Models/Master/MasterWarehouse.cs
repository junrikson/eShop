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
    public class MasterWarehouse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Kode gudang harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Gudang")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "MasterWarehouses", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nama gudang harus diisi.")]
        [Display(Name = "Nama Gudang")]
        [StringLength(256, ErrorMessage = "Maksimal 256 huruf.")]
        public string Name { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Lokasi harus diisi.")]
        [Display(Name = "Lokasi")]
        [StringLength(256, ErrorMessage = "Maksimal 256 huruf.")]
        public string Location { get; set; }

        [DatalistColumn]
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

    public class MasterWarehouseRegionDatalist : MvcDatalist<MasterWarehouse>
    {
        private DbContext Context { get; }

        public MasterWarehouseRegionDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Name;
        }

        public MasterWarehouseRegionDatalist()
        {
            Url = "/DatalistFilters/AllMasterWarehouseRegion";
            Title = "Master Gudang";
            AdditionalFilters.Add("MasterRegionId");

            Filter.Sort = "Name";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterWarehouse> GetModels()
        {
            return Context.Set<MasterWarehouse>();
        }
    }



    public class MasterWarehouseDatalist : MvcDatalist<MasterWarehouse>
    {
        private DbContext Context { get; }

        public MasterWarehouseDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Name;
        }

        public MasterWarehouseDatalist()
        {
            Url = "/DatalistFilters/AllMasterWarehouse";
            Title = "Master Gudang";

            Filter.Sort = "Name";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterWarehouse> GetModels()
        {
            return Context.Set<MasterWarehouse>();
        }
    }
}