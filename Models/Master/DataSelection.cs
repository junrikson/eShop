using Datalist;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace eShop.Models
{

    // Begin of MasterBusinessUnitAccount
    public class MasterBusinessUnitAccount
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "Wilayah")]
        [Required(ErrorMessage = "Wilayah harus diisi.")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [Key, Column(Order = 2)]
        [Display(Name = "Jenis")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumBusinessUnitAccountType Type { get; set; }

        [Display(Name = "Bagan Akun")]
        [Required(ErrorMessage = "Bagan Akun harus diisi.")]
        public int ChartOfAccountId { get; set; }

        [Display(Name = "Bagan Akun")]
        public virtual ChartOfAccount ChartOfAccount { get; set; }
    }

    // End of MasterBusinessUnitAccount

    // Begin of MasterBusinessUnitRegion

    public class MasterBusinessUnitRegion
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "Master Wilayah")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Master Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [Display(Name = "Dibuat")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }

    public class MasterBusinessUnitRegionSelection
    {
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Wilayah Awal")]
        [Required(ErrorMessage = "Wilayah Awal harus diisi.")]
        public int MasterRegionStartId { get; set; }

        [Display(Name = "Wilayah Awal")]
        public virtual MasterRegion MasterRegionStart { get; set; }

        [Display(Name = "Wilayah Akhir")]
        public int? MasterRegionEndId { get; set; }

        [Display(Name = "Wilayah Akhir")]
        public virtual MasterRegion MasterRegionEnd { get; set; }
    }

    public class MasterBusinessUnitRegionViewModel
    {
        public int Id { get; set; }

        [DatalistColumn]
        [Display(Name = "Kode Wilayah")]
        public string MasterRegionCode { get; set; }

        [DatalistColumn]
        [Display(Name = "Keterangan")]
        public string MasterRegionNotes { get; set; }

        [Display(Name = "Aktif")]
        public bool MasterRegionActive { get; set; }

        [Display(Name = "Unit Bisnis")]
        public int MasterBusinessUnitId { get; set; }

        [DatalistColumn]
        [Display(Name = "Unit Bisnis")]
        public string MasterBusinessUnitCode { get; set; }
    }

    public class MasterBusinessUnitRegionDatalist : MvcDatalist<MasterBusinessUnitRegionViewModel>
    {
        private DbContext Context { get; }

        public MasterBusinessUnitRegionDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.MasterRegionCode + " - " + model.MasterRegionNotes;
        }
        public MasterBusinessUnitRegionDatalist()
        {
            Url = "/DatalistFilters/AllMasterBusinessUnitRegion";
            Title = "Wilayah";
            AdditionalFilters.Add("MasterBusinessUnitId");

            Filter.Sort = "MasterRegionCode";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterBusinessUnitRegionViewModel> GetModels()
        {
            return Context.Set<MasterBusinessUnitRegion>()
                .Select(x => new MasterBusinessUnitRegionViewModel
                {
                    Id = x.MasterRegionId,
                    MasterRegionCode = x.MasterRegion.Code,
                    MasterRegionNotes = x.MasterRegion.Notes,
                    MasterRegionActive = x.MasterRegion.Active,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnitCode = x.MasterBusinessUnit.Code
                });
        }
    }

    // End of MasterBusinessUnitRegion

    // Begin of MasterBusinessUnitSupplier

    public class MasterBusinessUnitSupplier
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "Master Supplier")]
        public int MasterSupplierId { get; set; }

        [Display(Name = "Master Supplier")]
        public virtual MasterSupplier MasterSupplier { get; set; }

        [Display(Name = "Dibuat")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }

    public class MasterBusinessUnitSupplierSelection
    {
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Supplier Awal")]
        [Required(ErrorMessage = "Supplier Awal harus diisi.")]
        public int MasterSupplierStartId { get; set; }

        [Display(Name = "Supplier Akhir")]
        public virtual MasterSupplier MasterSupplierStart { get; set; }

        [Display(Name = "Supplier Akhir")]
        public int? MasterSupplierEndId { get; set; }

        [Display(Name = "Supplier Akhir")]
        public virtual MasterSupplier MasterSupplierEnd { get; set; }
    }

    // End of MasterBusinessUnitSupplier

    // Begin of MasterBusinessUnitCustomer

    public class MasterBusinessUnitCustomer
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "Master Konsumen")]
        public int MasterCustomerId { get; set; }

        [Display(Name = "Master Konsumen")]
        public virtual MasterCustomer MasterCustomer { get; set; }

        [Display(Name = "Dibuat")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }

    // End of MasterBusinessUnitCustomer

    // Begin of MasterBusinessSalesPerson

    public class MasterBusinessSalesPerson
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "Master Sales")]
        public int MasterSalesPersonId { get; set; }

        [Display(Name = "Master Sales")]
        public virtual MasterSalesPerson MasterSalesPerson { get; set; }

        [Display(Name = "Dibuat")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }

    // End of MasterBusinessSalesPerson

    // Begin of MasterBusinessCategory

    public class MasterBusinessCategory
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "Master Kategori")]
        public int MasterCategoryId { get; set; }

        [Display(Name = "Master Kategori")]
        public virtual MasterCategory MasterCategory { get; set; }

        [Display(Name = "Dibuat")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }

    // End of MasterBusinessCategory

    // Begin of MasterBusinessBrand

    public class MasterBusinessBrand
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "Master Merek")]
        public int MasterBrandId { get; set; }

        [Display(Name = "Master Merek")]
        public virtual MasterBrand MasterBrand { get; set; }

        [Display(Name = "Dibuat")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }

    // End of MasterBusinessBrand

    // Begin of MasterBusinessItem

    public class MasterBusinessItem
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "Master Barang")]
        public int MasterItemId { get; set; }

        [Display(Name = "Master Barang")]
        public virtual MasterItem MasterItem { get; set; }

        [Display(Name = "Dibuat")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }

    // End of MasterBusinessItem

    // Begin of MasterBusinessRegionWarehouse

    public class MasterBusinessRegionWarehouse
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "Master Wilayah")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Master Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [Key, Column(Order = 2)]
        [Display(Name = "Master Gudang")]
        public int MasterWarehouseId { get; set; }

        [Display(Name = "Master Gudang")]
        public virtual MasterWarehouse MasterWarehouse { get; set; }

        [Display(Name = "Dibuat")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }

    // End of MasterBusinessRegionWarehouse

    // Begin of MasterBusinessRegionAccount

    public class MasterBusinessRegionAccount
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "Master Wilayah")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Master Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [Key, Column(Order = 2)]
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

    public class MasterBusinessRegionAccountSelection
    {
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

        [Display(Name = "Akun Awal")]
        [Required(ErrorMessage = "Akun Awal harus diisi.")]
        public int ChartOfAccountStartId { get; set; }

        [Display(Name = "Akun Awal")]
        public virtual ChartOfAccount ChartOfAccountStart { get; set; }

        [Display(Name = "Akun Akhir")]
        public int? ChartOfAccountEndId { get; set; }

        [Display(Name = "Akun Akhir")]
        public virtual ChartOfAccount ChartOfAccountEnd { get; set; }
    }

    // End of MasterBusinessRegionAccount
}
