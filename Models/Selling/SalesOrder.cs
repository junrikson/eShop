﻿using Datalist;
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
    public class SalesOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Sales Order")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "SalesOrders", AdditionalFields = "Id", ErrorMessage = "Nomor ini sudah dipakai.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Tanggal")]
        [Required(ErrorMessage = "Tanggal harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [DatalistColumn]
        [Display(Name = "Tanggal Pengiriman")]
        [Required(ErrorMessage = "Tanggal Pengiriman harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DeliveryDate { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Relasi Bisnis")]
        public int? MasterBusinessUnitRelationId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnitRelation MasterBusinessUnitRelation { get; set; }

        [Display(Name = "Relasi SO")]
        public int? SalesOrderRelationId { get; set; }

        [Display(Name = "Relasi SO")]
        public virtual SalesOrder SalesOrderRelation { get; set; }

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

        [Display(Name = "Sales Request")]
        public int? SalesRequestId { get; set; }

        [Display(Name = "Sales Request")]
        public virtual SalesRequest SalesRequest { get; set; }

        [DatalistColumn]
        [Display(Name = "Kode Customer")]
        [Required(ErrorMessage = "Kode Customer harus diisi.")]
        public int MasterCustomerId { get; set; }

        [Display(Name = "Kode Customer")]
        public virtual MasterCustomer MasterCustomer { get; set; }

        [DatalistColumn]
        [Display(Name = "Kode Salesman")]
        public int? MasterSalesPersonId { get; set; }

        [Display(Name = "Kode Salesman")]
        public virtual MasterSalesPerson MasterSalesPerson { get; set; }

        [DatalistColumn]
        [Display(Name = "Kota Tujuan")]
        public int? MasterDestinationId { get; set; }

        [Display(Name = "Kota Tujuan")]
        public virtual MasterDestination MasterDestination { get; set; }

        [Display(Name = "Top")]
        public int Top { get; set; }

        [Display(Name = "Topx")]
        public int Topx { get; set; }

        [Display(Name = "Gudang")]
        [Required(ErrorMessage = "Gudang harus diisi.")]
        public int MasterWarehouseId { get; set; }

        [Display(Name = "Gudang")]
        public virtual MasterWarehouse MasterWarehouse { get; set; }

        [DatalistColumn]
        [Display(Name = "Referensi")]
        public string Reference { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [DatalistColumn]
        [Display(Name = "Diskon Rupiah")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Discount { get; set; }

        [DatalistColumn]
        [Display(Name = "Diskon Persen")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal DiscountValue { get; set; }

        [DatalistColumn]
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

    public class SalesOrderViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        [Required(ErrorMessage = "Nomor harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "SalesOrders", AdditionalFields = "Id", ErrorMessage = "Nomor ini sudah dipakai.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Tanggal")]
        [Required(ErrorMessage = "Tanggal harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [DatalistColumn]
        [Display(Name = "Customer")]
        public string MasterCustomerCode { get; set; }

        [DatalistColumn]
        [Display(Name = "Gudang")]
        public string MasterWarehouseCode { get; set; }

        [DatalistColumn]
        [Display(Name = "Diskon Rupiah")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Discount { get; set; }

        [DatalistColumn]
        [Display(Name = "Diskon Persen")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal DiscountValue { get; set; }

        [DatalistColumn]
        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Total { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }
    }

    public class OutstandingSalesOrderDatalist : MvcDatalist<SalesOrderViewModel>
    {
        private DbContext Context { get; }

        public OutstandingSalesOrderDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.MasterCustomerCode;
        }
        public OutstandingSalesOrderDatalist()
        {
            Url = "/DatalistFilters/AllOutstandingSalesOrder";
            Title = "Sales Order";
            AdditionalFilters.Add("MasterBusinessUnitId");
            AdditionalFilters.Add("MasterRegionId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<SalesOrderViewModel> GetModels()
        {
            return Context.Set<SalesOrder>()
                .Where(x => !Context.Set<Sale>().Where(p => p.Active == true && p.SalesOrderId == x.Id).Any())
                .Select(x => new SalesOrderViewModel
                {
                    Id = x.Id,
                    MasterBusinessUnitCode = x.MasterBusinessUnit.Code,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    MasterRegionCode = x.MasterRegion.Code,
                    MasterRegionId = x.MasterRegionId,
                    MasterRegion = x.MasterRegion,
                    MasterCustomerCode = x.MasterCustomer.Code,
                    MasterWarehouseCode = x.MasterWarehouse.Code,
                    Code = x.Code,
                    Date = x.Date,
                    Total = x.Total,
                    Active = x.Active,
                });
        }
    }

    public class OutstandingSalesOrderRelationDatalist : MvcDatalist<SalesOrderViewModel>
    {
        private DbContext Context { get; }

        public OutstandingSalesOrderRelationDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.MasterCustomerCode;
        }
        public OutstandingSalesOrderRelationDatalist()
        {
            Url = "/DatalistFilters/AllOutstandingSalesOrderRelation";
            Title = "Sales Order";
            AdditionalFilters.Add("MasterBusinessUnitRelationId");
            AdditionalFilters.Add("MasterRegionId");

            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<SalesOrderViewModel> GetModels()
        {
            return Context.Set<SalesOrder>()
                .Where(x => !Context.Set<Sale>().Where(p => p.Active == true && p.SalesOrderId == x.Id).Any())
                .Select(x => new SalesOrderViewModel
                {
                    Id = x.Id,
                    MasterBusinessUnitCode = x.MasterBusinessUnit.Code,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    MasterRegionCode = x.MasterRegion.Code,
                    MasterRegionId = x.MasterRegionId,
                    MasterRegion = x.MasterRegion,
                    MasterCustomerCode = x.MasterCustomer.Code,
                    MasterWarehouseCode = x.MasterWarehouse.Code,
                    Code = x.Code,
                    Date = x.Date,
                    Total = x.Total,
                    Active = x.Active,
                });
        }
    }

    public class SalesOrderDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Sales Order")]
        [Required(ErrorMessage = "Invoice harus diisi.")]
        public int SalesOrderId { get; set; }

        [Display(Name = "Sales Order")]
        public virtual SalesOrder SalesOrder { get; set; }

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
        public int Quantity { get; set; }

        [Display(Name = "Harga")]
        [Required(ErrorMessage = "Harga harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Display(Name = "Diskon Rupiah")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Discount { get; set; }

        [Display(Name = "Diskon Persen")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal DiscountValue { get; set; }

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