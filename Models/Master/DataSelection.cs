using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace eShop.Models
{
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

        [Display(Name = "Supplier Awal")]
        public virtual MasterSupplier MasterSupplierStart { get; set; }

        [Display(Name = "Supplier Akhir")]
        public int? MasterSupplierEndId { get; set; }

        [Display(Name = "Supplier Akhir")]
        public virtual MasterSupplier MasterSupplierEnd { get; set; }
    }

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

    public class MasterBusinessUnitCustomerSelection
    {
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Konsumen Awal")]
        [Required(ErrorMessage = "Konsumen Awal harus diisi.")]
        public int MasterCustomerStartId { get; set; }

        [Display(Name = "Konsumen Awal")]
        public virtual MasterCustomer MasterCustomerStart { get; set; }

        [Display(Name = "Konsumen Akhir")]
        public int? MasterCustomerEndId { get; set; }

        [Display(Name = "Konsumen Akhir")]
        public virtual MasterCustomer MasterCustomerEnd { get; set; }
    }

    public class MasterBusinessUnitSalesPerson
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

    public class MasterBusinessUnitSalesPersonSelection
    {
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Sales Awal")]
        [Required(ErrorMessage = "Sales Awal harus diisi.")]
        public int MasterSalesPersonStartId { get; set; }

        [Display(Name = "Sales Awal")]
        public virtual MasterSalesPerson MasterSalesPersonStart { get; set; }

        [Display(Name = "Sales Akhir")]
        public int? MasterSalesPersonEndId { get; set; }

        [Display(Name = "Sales Akhir")]
        public virtual MasterSalesPerson MasterSalesPersonEnd { get; set; }
    }
    public class MasterBusinessUnitCategory
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

    public class MasterBusinessUnitCategorySelection
    {
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Kategori Awal")]
        [Required(ErrorMessage = "Kategori Awal harus diisi.")]
        public int MasterCategoryStartId { get; set; }

        [Display(Name = "Kategori Awal")]
        public virtual MasterCategory MasterCategoryStart { get; set; }

        [Display(Name = "Kategori Akhir")]
        public int? MasterCategoryEndId { get; set; }

        [Display(Name = "Kategori Akhir")]
        public virtual MasterCategory MasterCategoryEnd { get; set; }
    }

    public class MasterBusinessUnitBrand
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

    public class MasterBusinessUnitBrandSelection
    {
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Merek Awal")]
        [Required(ErrorMessage = "Merek Awal harus diisi.")]
        public int MasterBrandStartId { get; set; }

        [Display(Name = "Merek Awal")]
        public virtual MasterBrand MasterBrandStart { get; set; }

        [Display(Name = "Merek Akhir")]
        public int? MasterBrandEndId { get; set; }

        [Display(Name = "Merek Akhir")]
        public virtual MasterBrand MasterBrandEnd { get; set; }
    }

    public class MasterBusinessUnitItem
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

    public class MasterBusinessUnitItemSelection
    {
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Item Awal")]
        [Required(ErrorMessage = "Item Awal harus diisi.")]
        public int MasterItemStartId { get; set; }

        [Display(Name = "Item Awal")]
        public virtual MasterItem MasterItemStart { get; set; }

        [Display(Name = "Item Akhir")]
        public int? MasterItemEndId { get; set; }

        [Display(Name = "Item Akhir")]
        public virtual MasterItem MasterItemEnd { get; set; }
    }

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

    //public class MasterRegionAccountViewModel
    //{
    //    public int Id { get; set; }

    //    [Display(Name = "Kode Wilayah")]
    //    public string Code { get; set; }

    //    [Display(Name = "Akun Awal")]
    //    public int ChartOfAccountStartId { get; set; }

    //    [Display(Name = "Akun Awal")]
    //    public virtual ChartOfAccount ChartOfAccountStart { get; set; }

    //    [Display(Name = "Akun Akhir")]
    //    public int ChartOfAccountEndId { get; set; }

    //    [Display(Name = "Akun Akhir")]
    //    public virtual ChartOfAccount ChartOfAccountEnd { get; set; }
    //}

    //public class MasterRegionAccountDatalistViewModel
    //{
    //    [Display(Name = "Bagan Akun")]
    //    public int Id { get; set; }

    //    [Display(Name = "Master Wilayah")]
    //    public int MasterRegionId { get; set; }

    //    [Display(Name = "Master Wilayah")]
    //    public virtual MasterRegion MasterRegion { get; set; }

    //    [DatalistColumn]
    //    public string Code { get; set; }

    //    [DatalistColumn]
    //    [Display(Name = "Nama Akun")]
    //    public string Name { get; set; }

    //    [Display(Name = "Bagan Akun")]
    //    public virtual ChartOfAccount ChartOfAccount { get; set; }

    //    [DatalistColumn]
    //    [Display(Name = "Wilayah")]
    //    public string MasterRegionCode { get; set; }
    //}

    //public class MasterRegionAccountDatalist : MvcDatalist<MasterRegionAccountDatalistViewModel>
    //{
    //    private DbContext Context { get; }

    //    public MasterRegionAccountDatalist(DbContext context)
    //    {
    //        Context = context;

    //        GetLabel = (model) => model.Code + " - " + model.Name;
    //    }
    //    public MasterRegionAccountDatalist()
    //    {
    //        Url = "/DatalistFilters/AllMasterRegionAccount";
    //        Title = "Bagan Akun";
    //        AdditionalFilters.Add("MasterRegionId");

    //        Filter.Sort = "Code";
    //        Filter.Order = DatalistSortOrder.Asc;
    //        Filter.Rows = 10;
    //    }

    //    public override IQueryable<MasterRegionAccountDatalistViewModel> GetModels()
    //    {
    //        return Context.Set<MasterRegionAccount>()
    //            .Select(x => new MasterRegionAccountDatalistViewModel
    //            {
    //                Id = x.ChartOfAccountId,
    //                MasterRegionId = x.MasterRegionId,
    //                MasterRegion = x.MasterRegion,
    //                MasterRegionCode = x.MasterRegion.Code,
    //                Code = x.ChartOfAccount.Code,
    //                Name = x.ChartOfAccount.Name,
    //                ChartOfAccount = x.ChartOfAccount
    //            });
    //    }
    //}
}
