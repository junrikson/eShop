﻿using Datalist;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace eShop.Models
{
    public class MaterialReturn
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Retur Bahan Baku")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "MaterialReturns", AdditionalFields = "Id", ErrorMessage = "Nomor ini sudah dipakai.")]
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

        //[DatalistColumn]
        //[Display(Name = "Kode Produk")]
        //[Required(ErrorMessage = "Kode Produk harus diisi.")]
        //public int HeaderMasterItemId { get; set; }

        //[Display(Name = "Kode Produk")]
        //public virtual MasterItem HeaderMasterItem { get; set; }

        //[Display(Name = "Satuan")]
        //[Required(ErrorMessage = "Master Satuan harus diisi.")]
        //public int HeaderMasterItemUnitId { get; set; }

        //[Display(Name = "Satuan")]
        //public virtual MasterItemUnit HeaderMasterItemUnit { get; set; }

        //[Display(Name = "Quantity")]
        //[Required(ErrorMessage = "Quantity harus diisi.")]
        //[DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        //public decimal HeaderQuantity { get; set; }

        [Display(Name = "Mata Uang")]
        [Required(ErrorMessage = "Mata Uang harus diisi.")]
        public int MasterCurrencyId { get; set; }

        [Display(Name = "Mata Uang")]
        public virtual MasterCurrency MasterCurrency { get; set; }

        [Display(Name = "Rate")]
        [Required(ErrorMessage = "Rate harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##########}", ApplyFormatInEditMode = true)]
        public decimal Rate { get; set; }

        [Display(Name = "Nomor Pengambilan Bahan Baku")]
        public int? MaterialSlipId { get; set; }

        [Display(Name = "Nomor Pengambilan Bahan Baku")]
        public virtual MaterialSlip MaterialSlip { get; set; }

        [Display(Name = "Gudang")]
        [Required(ErrorMessage = "Gudang harus diisi.")]
        public int MasterWarehouseId { get; set; }

        [Display(Name = "Gudang")]
        public virtual MasterWarehouse MasterWarehouse { get; set; }

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

    public class MaterialReturnModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Retur Bahan Baku")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "MaterialReturns", AdditionalFields = "Id", ErrorMessage = "Nomor ini sudah dipakai.")]
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

        [Display(Name = "Nomor Pengambilan Bahan Baku")]
        public int? MaterialSlipId { get; set; }

        [Display(Name = "Nomor Pengambilan Bahan Baku")]
        public virtual MaterialSlip MaterialSlip { get; set; }

        //[DatalistColumn]
        //[Display(Name = "Kode Produk")]
        //[Required(ErrorMessage = "Kode Produk harus diisi.")]
        //public int HeaderMasterItemId { get; set; }

        //[Display(Name = "Kode Produk")]
        //public virtual MasterItem HeaderMasterItem { get; set; }

        //[Display(Name = "Satuan")]
        //[Required(ErrorMessage = "Master Satuan harus diisi.")]
        //public int HeaderMasterItemUnitId { get; set; }

        //[Display(Name = "Satuan")]
        //public virtual MasterItemUnit HeaderMasterItemUnit { get; set; }

        //[Display(Name = "Quantity")]
        //[Required(ErrorMessage = "Quantity harus diisi.")]
        //[DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        //public decimal HeaderQuantity { get; set; }

        [DatalistColumn]
        [Display(Name = "Gudang")]
        public string MasterWarehouseCode { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Total { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }
    }

    //public class OutstandingMaterialSlipDatalist : MvcDatalist<MaterialSlipViewModel>
    //{
    //    private DbContext Context { get; }

    //    public OutstandingMaterialSlipDatalist(DbContext context)
    //    {
    //        Context = context;

    //        GetLabel = (model) => model.Code;
    //    }
    //    public OutstandingMaterialSlipDatalist()
    //    {
    //        Url = "/DatalistFilters/AllOutstandingMaterialSlip";
    //        Title = "Sales Order";
    //        AdditionalFilters.Add("MasterBusinessUnitId");
    //        AdditionalFilters.Add("MasterRegionId");

    //        Filter.Sort = "Code";
    //        Filter.Order = DatalistSortOrder.Asc;
    //        Filter.Rows = 10;
    //    }

    //    public override IQueryable<MaterialSlipViewModel> GetModels()
    //    {
    //        return Context.Set<MaterialSlip>()
    //            .Where(x => !Context.Set<Sale>().Where(p => p.Active == true && p.SalesOrderId == x.Id).Any())
    //            .Select(x => new SalesOrderViewModel
    //            {
    //                Id = x.Id,
    //                MasterBusinessUnitCode = x.MasterBusinessUnit.Code,
    //                MasterBusinessUnitId = x.MasterBusinessUnitId,
    //                MasterBusinessUnit = x.MasterBusinessUnit,
    //                MasterRegionCode = x.MasterRegion.Code,
    //                MasterRegionId = x.MasterRegionId,
    //                MasterRegion = x.MasterRegion,
    //                //MasterCustomerCode = x.MasterCustomer.Code,
    //                MasterWarehouseCode = x.MasterWarehouse.Code,
    //                Code = x.Code,
    //                Date = x.Date,
    //                Total = x.Total,
    //                Active = x.Active,
    //            });
    //    }
    //}

    //public class SalesOrderPurchaseOrderViewModel
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public int Id { get; set; }

    //    [DatalistColumn]
    //    [Required(ErrorMessage = "Nomor harus diisi.")]
    //    [Index("IX_Code", Order = 1, IsUnique = true)]
    //    [Display(Name = "Nomor")]
    //    [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
    //    [Remote("IsCodeExists", "SalesOrders", AdditionalFields = "Id", ErrorMessage = "Nomor ini sudah dipakai.")]
    //    public string Code { get; set; }

    //    [DatalistColumn]
    //    [Display(Name = "Tanggal")]
    //    [Required(ErrorMessage = "Tanggal harus diisi.")]
    //    [DataType(DataType.Date)]
    //    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    //    public DateTime Date { get; set; }

    //    [Display(Name = "Unit Bisnis")]
    //    [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
    //    public int MasterBusinessUnitId { get; set; }

    //    [Display(Name = "Unit Bisnis")]
    //    public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

    //    [DatalistColumn]
    //    [Display(Name = "Unit Bisnis")]
    //    public string MasterBusinessUnitCode { get; set; }

    //    [Display(Name = "Wilayah")]
    //    [Required(ErrorMessage = "Wilayah harus diisi.")]
    //    public int MasterRegionId { get; set; }

    //    [Display(Name = "Wilayah")]
    //    public virtual MasterRegion MasterRegion { get; set; }

    //    [DatalistColumn]
    //    [Display(Name = "Wilayah")]
    //    public string MasterRegionCode { get; set; }

    //    [DatalistColumn]
    //    [Display(Name = "Customer")]
    //    public string MasterCustomerCode { get; set; }

    //    [DatalistColumn]
    //    [Display(Name = "Gudang")]
    //    public string MasterWarehouseCode { get; set; }

    //    [Display(Name = "Total")]
    //    [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
    //    public decimal Total { get; set; }

    //    [Display(Name = "Aktif")]
    //    public bool Active { get; set; }
    //}

    //public class OutstandingSalesOrderPurchaseOrderDatalist : MvcDatalist<SalesOrderPurchaseOrderViewModel>
    //{
    //    private DbContext Context { get; }

    //    public OutstandingSalesOrderPurchaseOrderDatalist(DbContext context)
    //    {
    //        Context = context;

    //        GetLabel = (model) => model.Code + " - " + model.MasterCustomerCode;
    //    }
    //    public OutstandingSalesOrderPurchaseOrderDatalist()
    //    {
    //        Url = "/DatalistFilters/AllOutstandingSalesOrderPurchaseOrder";
    //        Title = "Sales Order";
    //        AdditionalFilters.Add("MasterBusinessUnitId");
    //        AdditionalFilters.Add("MasterRegionId");

    //        Filter.Sort = "Code";
    //        Filter.Order = DatalistSortOrder.Asc;
    //        Filter.Rows = 10;
    //    }

    //    public override IQueryable<SalesOrderPurchaseOrderViewModel> GetModels()
    //    {
    //        return Context.Set<SalesOrder>()
    //            .Where(x => !Context.Set<PurchaseOrder>().Where(p => p.Active == true && p.SalesOrderId == x.Id).Any())
    //            .Select(x => new SalesOrderPurchaseOrderViewModel
    //            {
    //                Id = x.Id,
    //                MasterBusinessUnitCode = x.MasterBusinessUnit.Code,
    //                MasterBusinessUnitId = x.MasterBusinessUnitId,
    //                MasterBusinessUnit = x.MasterBusinessUnit,
    //                MasterRegionCode = x.MasterRegion.Code,
    //                MasterRegionId = x.MasterRegionId,
    //                MasterRegion = x.MasterRegion,
    //                MasterCustomerCode = x.MasterCustomer.Code,
    //                MasterWarehouseCode = x.MasterWarehouse.Code,
    //                Code = x.Code,
    //                Date = x.Date,
    //                Total = x.Total,
    //                Active = x.Active,
    //            });
    //    }
    //}



    public class MaterialReturnDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Nomor Reture Bahan Baku")]
        [Required(ErrorMessage = "Nomor Reture Bahan Baku harus diisi.")]
        public int MaterialReturnId { get; set; }

        [Display(Name = "Retur Bahan Baku")]
        public virtual MaterialReturn MaterialReturn { get; set; }

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

        [Display(Name = "Quantity Wo")]
        [Required(ErrorMessage = "Quantity Wo harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal QuantityWo { get; set; }

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