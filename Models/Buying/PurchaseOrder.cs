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
    public class PurchaseOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor Purchase Order harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Purchase Order")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "PurchaseOrders", AdditionalFields = "Id", ErrorMessage = "Nomor ini sudah dipakai.")]
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

        [Display(Name = "Purchase Request")]
        public int? PurchaseRequestId { get; set; }

        [Display(Name = "Purchase Request")]
        public virtual PurchaseRequest PurchaseRequest { get; set; }

        [DatalistColumn]
        [Display(Name = "Kode Supplier")]
        [Required(ErrorMessage = "Kode Supplier harus diisi.")]
        public int MasterSupplierId { get; set; }

        [Display(Name = "Kode Supplier")]
        public virtual MasterSupplier MasterSupplier { get; set; }

        [Display(Name = "Sales Order")]
        public int? SalesOrderId { get; set; }

        [Display(Name = "Sales Order")]
        public virtual SalesOrder SalesOrder { get; set; }

        [DatalistColumn]
        [Display(Name = "Kode Konsumen")]
        public int? MasterCustomerId { get; set; }

        [Display(Name = "Kode Konsumen")]
        public virtual MasterCustomer MasterCustomer { get; set; }

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

    public class PurchaseOrderSalesOrderViewModel
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
        [Required(ErrorMessage = "Nomor Purchase Order harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Purchase Order")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "PurchaseOrders", AdditionalFields = "Id", ErrorMessage = "Nomor ini sudah dipakai.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Tanggal")]
        [Required(ErrorMessage = "Tanggal harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

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
        [Display(Name = "Kode Supplier")]
        [Required(ErrorMessage = "Kode Supplier harus diisi.")]
        public int MasterSupplierId { get; set; }

        [Display(Name = "Kode Supplier")]
        public virtual MasterSupplier MasterSupplier { get; set; }

        [DatalistColumn]
        [Display(Name = "Customer")]
        public string MasterCustomerCode { get; set; }

        [DatalistColumn]
        [Display(Name = "Nama Konsumen")]
        public string MasterCustomerName { get; set; }

        [DatalistColumn]
        [Display(Name = "Gudang")]
        public string MasterWarehouseCode { get; set; }

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

    public class OutstandingPurchaseOrderSalesOrderDatalist : MvcDatalist<PurchaseOrderSalesOrderViewModel>
    {
        private DbContext Context { get; }

        public OutstandingPurchaseOrderSalesOrderDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code;
        }
        public OutstandingPurchaseOrderSalesOrderDatalist()
        {
            Url = "/DatalistFilters/AllOutstandingPurchaseOrderSalesOrder";
            Title = "Purchase Order";
            AdditionalFilters.Add("MasterBusinessUnitId");
            AdditionalFilters.Add("MasterRegionId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<PurchaseOrderSalesOrderViewModel> GetModels()
        {
            return Context.Set<PurchaseOrder>()
                .Where(x => !Context.Set<SalesOrder>().Where(p => p.Active == true && p.PurchaseOrderId == x.Id).Any())
                .Select(x => new PurchaseOrderSalesOrderViewModel
                {
                    Id = x.Id,
                    MasterBusinessUnitCode = x.MasterBusinessUnit.Code,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    MasterRegionCode = x.MasterRegion.Code,
                    MasterRegionId = x.MasterRegionId,
                    MasterRegion = x.MasterRegion,
                    MasterCustomerCode = x.MasterCustomer.Code,
                    MasterCustomerName = x.MasterCustomer.Name,
                    MasterWarehouseCode = x.MasterWarehouse.Code,
                    Created = x.Created,
                    Updated = x.Updated,
                    Code = x.Code,
                    Date = x.Date,
                    Total = x.Total,
                    Active = x.Active
                });
        }
    }
    public class PurchaseOrderViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor Purchase Order harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Purchase Order")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "PurchaseOrders", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
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
        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Total { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }
    }

    public class OutstandingPurchaseOrderDatalist : MvcDatalist<PurchaseOrderViewModel>
    {
        private DbContext Context { get; }

        public OutstandingPurchaseOrderDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.MasterSupplierCode;
        }
        public OutstandingPurchaseOrderDatalist()
        {
            Url = "/DatalistFilters/AllOutstandingPurchaseOrder";
            Title = "Purchase Order";
            AdditionalFilters.Add("MasterBusinessUnitId");
            AdditionalFilters.Add("MasterRegionId");

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<PurchaseOrderViewModel> GetModels()
        {
            return Context.Set<PurchaseOrder>()
                .Where(x => !Context.Set<Purchase>().Where(p => p.Active == true && p.PurchaseOrderId == x.Id).Any())
                .Select(x => new PurchaseOrderViewModel
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

    public class PurchaseOrderDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Purchase Order")]
        [Required(ErrorMessage = "Purchase Order harus diisi.")]
        public int PurchaseOrderId { get; set; }

        [Display(Name = "Purchase Order")]
        public virtual PurchaseOrder PurchaseOrder { get; set; }

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