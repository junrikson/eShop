using Datalist;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace eShop.Models
{
    public class StockCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Gudang harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "MasterBusinessUnit")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Wilayah")]
        [Required(ErrorMessage = "Wilayah harus diisi.")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [Display(Name = "Gudang")]
        [Required(ErrorMessage = "Gudang harus diisi.")]
        public int MasterWarehouseId { get; set; }

        [Display(Name = "Gudang")]
        public virtual MasterWarehouse MasterWarehouse { get; set; }

        [Display(Name = "Master Item")]
        [Required(ErrorMessage = "Master Item harus diisi.")]
        public int MasterItemId { get; set; }

        [Display(Name = "Master Item")]
        public virtual MasterItem MasterItem { get; set; }

        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity harus diisi.")]
        public int Quantity { get; set; }

        [Display(Name = "Jenis Menu")]
        public EnumMenuType MenuType { get; set; }

        [Display(Name = "Penerimaan Barang")]
        public int? GoodsReceiptDetailsId { get; set; }

        [Display(Name = "Penerimaan Barang")]
        public virtual GoodsReceiptDetails GoodsReceiptDetails { get; set; }

        [Display(Name = "Penjualan")]
        public int? SaleDetailsId { get; set; }

        [Display(Name = "Penjualan")]
        public virtual SaleDetails SaleDetails { get; set; }

        [Display(Name = "Penyesuaian")]
        public int? StockAdjustmentDetailsId { get; set; }

        [Display(Name = "Penyesuaian")]
        public virtual StockAdjustmentDetails StockAdjustmentDetails { get; set; }
    }
}