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
    public class PurchaseRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nomor Purchase Request harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Purchase Request")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "PurchaseRequests", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
        public string Code { get; set; }

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

        [Display(Name = "Kode Supplier")]
        [Required(ErrorMessage = "Kode Supplier harus diisi.")]
        public int MasterSupplierId { get; set; }

        [Display(Name = "Kode Supplier")]
        public virtual MasterSupplier MasterSupplier { get; set; }

        [Display(Name = "Gudang")]
        [Required(ErrorMessage = "Gudang harus diisi.")]
        public int MasterWarehouseId { get; set; }

        [Display(Name = "Gudang")]
        public virtual MasterWarehouse MasterWarehouse { get; set; }

        [Display(Name = "Referensi")]
        public string Reference { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Diskon Rupiah")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Discount { get; set; }

        [Display(Name = "Diskon Persen")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal DiscountValue { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Total { get; set; }

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

        [Display(Name = "Print")]
        public bool IsPrint { get; set; }
    }
    public class PurchaseRequestViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor Purchase Request harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Purchase Request")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "PurchaseRequests", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
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

        [DatalistColumn]
        [Display(Name = "Supplier")]
        public string MasterSupplierCode { get; set; }

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

    public class OutstandingPurchaseRequestDatalist : MvcDatalist<PurchaseRequestViewModel>
    {
        private DbContext Context { get; }

        public OutstandingPurchaseRequestDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.MasterSupplierCode;
        }
        public OutstandingPurchaseRequestDatalist()
        {
            Url = "/DatalistFilters/AllOutstandingPurchaseRequest";
            Title = "Purchase Request";
            AdditionalFilters.Add("MasterBusinessUnitId");
            AdditionalFilters.Add("MasterRegionId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<PurchaseRequestViewModel> GetModels()
        {
            return Context.Set<PurchaseRequest>()
                .Where(x => !Context.Set<PurchaseOrder>().Where(p => p.Active == true && p.PurchaseRequestId == x.Id).Any())
                .Select(x => new PurchaseRequestViewModel
                {
                    Id = x.Id,
                    MasterBusinessUnitCode = x.MasterBusinessUnit.Code,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    MasterRegionCode = x.MasterRegion.Code,
                    MasterRegionId = x.MasterRegionId,
                    MasterRegion = x.MasterRegion,
                    MasterSupplierCode = x.MasterSupplier.Code,
                    MasterWarehouseCode = x.MasterWarehouse.Code,
                    Code = x.Code,
                    Date = x.Date,
                    Total = x.Total,
                    Active = x.Active,
                });
        }
    }

    public class PurchaseRequestDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Purchase Request")]
        [Required(ErrorMessage = "Invoice harus diisi.")]
        public int PurchaseRequestId { get; set; }

        [Display(Name = "Purchase Request")]
        public virtual PurchaseRequest PurchaseRequest { get; set; }

        [Display(Name = "Master Item")]
        [Required(ErrorMessage = "Master Item harus diisi.")]
        public int MasterItemId { get; set; }

        [Display(Name = "Master Item")]
        public virtual MasterItem MasterItem { get; set; }

        [Display(Name = "Satuan")]
        [Required(ErrorMessage = "Master Satuan harus diisi.")]
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

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Diskon Rupiah")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Discount { get; set; }

        [Display(Name = "Diskon Persen")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal DiscountValue { get; set; }

        [Display(Name = "Nilai")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Total { get; set; }

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