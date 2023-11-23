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
    public class PackingBillofMaterial
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor Formula Barang Jadi harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Formula Barang Jadi")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "PackingBillofMaterials", AdditionalFields = "Id", ErrorMessage = "Nomor ini sudah dipakai.")]
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

        [Display(Name = "Mata Uang")]
        [Required(ErrorMessage = "Mata Uang harus diisi.")]
        public int MasterCurrencyId { get; set; }

        [Display(Name = "Mata Uang")]
        public virtual MasterCurrency MasterCurrency { get; set; }

        [Display(Name = "Rate")]
        [Required(ErrorMessage = "Rate harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##########}", ApplyFormatInEditMode = true)]
        public decimal Rate { get; set; }

        [DatalistColumn]
        [Display(Name = "Kode Produk")]
        [Required(ErrorMessage = "Kode Produk harus diisi.")]
        public int HeaderMasterItemId { get; set; }

        [Display(Name = "Kode Produk")]
        public virtual MasterItem HeaderMasterItem { get; set; }

        [Display(Name = "Satuan")]
        [Required(ErrorMessage = "Master Satuan harus diisi.")]
        public int HeaderMasterItemUnitId { get; set; }

        [Display(Name = "Satuan")]
        public virtual MasterItemUnit HeaderMasterItemUnit { get; set; }

        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal HeaderQuantity { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
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

    public class PackingBillofMaterialViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor Formula Barang Jadi harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Formula Barang Jadi")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "PackingBillofMaterials", AdditionalFields = "Id", ErrorMessage = "Nomor ini sudah dipakai.")]
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

        [Display(Name = "Mata Uang")]
        [Required(ErrorMessage = "Mata Uang harus diisi.")]
        public int MasterCurrencyId { get; set; }

        [Display(Name = "Mata Uang")]
        public virtual MasterCurrency MasterCurrency { get; set; }

        [Display(Name = "Rate")]
        [Required(ErrorMessage = "Rate harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##########}", ApplyFormatInEditMode = true)]
        public decimal Rate { get; set; }

        [DatalistColumn]
        [Display(Name = "Kode Produk")]
        [Required(ErrorMessage = "Kode Produk harus diisi.")]
        public int HeaderMasterItemId { get; set; }

        [Display(Name = "Kode Produk")]
        public virtual MasterItem HeaderMasterItem { get; set; }

        [Display(Name = "Satuan")]
        [Required(ErrorMessage = "Master Satuan harus diisi.")]
        public int HeaderMasterItemUnitId { get; set; }

        [Display(Name = "Satuan")]
        public virtual MasterItemUnit HeaderMasterItemUnit { get; set; }

        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal HeaderQuantity { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Total { get; set; }

        [Display(Name = "Print")]
        public bool IsPrint { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }
    }

    public class AllPackingBillofMaterialDatalist : MvcDatalist<PackingBillofMaterialViewModel>
    {
        private DbContext Context { get; }

        public AllPackingBillofMaterialDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code;
        }
        public AllPackingBillofMaterialDatalist()
        {
            Url = "/DatalistFilters/AllPackingBillofMaterial";
            Title = "Formula Barang jadi";
            AdditionalFilters.Add("MasterBusinessUnitId");
            AdditionalFilters.Add("MasterRegionId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<PackingBillofMaterialViewModel> GetModels()
        {
            return Context.Set<PackingBillofMaterial>()
                // .Where(x => !Context.Set<ProductionWorkOrder>().Where(p => p.Active == true && p.ProductionBillofMaterialId == x.Id).Any())
                .Select(x => new PackingBillofMaterialViewModel
                {
                    Id = x.Id,
                    MasterBusinessUnitCode = x.MasterBusinessUnit.Code,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    MasterRegionCode = x.MasterRegion.Code,
                    MasterRegionId = x.MasterRegionId,
                    MasterRegion = x.MasterRegion,
                    Code = x.Code,
                    Date = x.Date,
                    Total = x.Total,
                    Active = x.Active,

                });
        }
    }

    public class PackingBillofMaterialDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Packing Bill of Material")]
        [Required(ErrorMessage = "Nomor BOM harus diisi.")]
        public int PackingBillofMaterialId { get; set; }

        [Display(Name = "Packing Bill of Material")]
        public virtual PackingBillofMaterial PackingBillofMaterial { get; set; }

        [Display(Name = "Master Item")]
        [Required(ErrorMessage = "Master Item harus diisi.")]
        public int MasterItemId { get; set; }

        [Display(Name = "Master Item")]
        public virtual MasterItem MasterItem { get; set; }

        [Display(Name = "Satuan")]
        [Required(ErrorMessage = "Master Item harus diisi.")]
        public int MasterItemUnitId { get; set; }

        [Display(Name = "Satuan")]
        public virtual MasterItemUnit MasterItemUnit { get; set; }

        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Quantity { get; set; }

        [Display(Name = "Harga")]
        [Required(ErrorMessage = "Harga harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Display(Name = "Nilai")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Total { get; set; }

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

