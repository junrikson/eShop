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
    public class MasterItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Item code must be filled.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Item Code")]
        [StringLength(128, ErrorMessage = "Maximum 128 Characters.")]
        [Remote("IsCodeExists", "MasterItems", AdditionalFields = "Id", ErrorMessage = "This code has been used.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Item Name")]
        [StringLength(256, ErrorMessage = "Maximum 128 Characters.")]
        public string Name { get; set; }

        [Display(Name = "Kategori")]
        [Required(ErrorMessage = "Kode Kategori harus diisi.")]
        public int MasterCategoryId { get; set; }

        [Display(Name = "Kategori")]
        public virtual MasterCategory MasterCategory { get; set; }

        [Display(Name = "Merek")]
        [Required(ErrorMessage = "Kode Merek harus diisi.")]
        public int MasterBrandId { get; set; }

        [Display(Name = "Merek")]
        public virtual MasterBrand MasterBrand { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        //[Display(Name = "Default Discount (%)")]
        //[DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        //public decimal DefaultDiscountPercentage { get; set; }

        //[Display(Name = "Harga Beli")]
        //[DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        //public decimal PurchasePrice { get; set; }

        //[Display(Name = "Pajak Penjualan dan Pembelian")]
        //[DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        //public decimal Vat { get; set; }

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



    public class MasterItemSupplierDatalist : MvcDatalist<MasterItem>
    {
        private DbContext Context { get; }

        public MasterItemSupplierDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " " + model.Name;
        }
        public MasterItemSupplierDatalist()
        {
            Url = "/DatalistFilters/AllMasterItemSupplier";
            Title = "Master Item";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterItem> GetModels()
        {
            return Context.Set<MasterItem>();
        }
    }

    public class AllMasterItemDatalist : MvcDatalist<MasterItem>
    {
        private DbContext Context { get; }

        public AllMasterItemDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " " + model.Name;
        }
        public AllMasterItemDatalist()
        {
            Url = "/DatalistFilters/AllMasterItem";
            Title = "Master Item";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterItem> GetModels()
        {
            return Context.Set<MasterItem>();
        }
    }

    public class MasterItemUnit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Master Item")]
        [Required(ErrorMessage = "Master Item harus diisi.")]
        public int MasterItemId { get; set; }

        [Display(Name = "Master Item")]
        public virtual MasterItem MasterItem { get; set; }

        [Display(Name = "Satuan")]
        [Required(ErrorMessage = "Master Item harus diisi.")]
        public int MasterUnitId { get; set; }

        [Display(Name = "Satuan")]
        public virtual MasterUnit MasterUnit { get; set; }

        [Display(Name = "Default")]
        public bool Default { get; set; }

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
    public class MasterItemUnitViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Master Item")]
        [Required(ErrorMessage = "Master Item harus diisi.")]
        public int MasterItemId { get; set; }

        [Display(Name = "Master Item")]
        public virtual MasterItem MasterItem { get; set; }

        [Display(Name = "Satuan")]
        [Required(ErrorMessage = "Master Item harus diisi.")]
        public int MasterUnitId { get; set; }

        [Display(Name = "Satuan")]
        public virtual MasterUnit MasterUnit { get; set; }

        [DatalistColumn]
        [Display(Name = "Satuan")]
        public string MasterUnitCode { get; set; }

        [DatalistColumn]
        [Display(Name = "Rasio")]
        [DisplayFormat(DataFormatString = "{0:0.##########}", ApplyFormatInEditMode = true)]
        public decimal MasterUnitRatio { get; set; }

        [DatalistColumn]
        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string MasterUnitNotes { get; set; }

        [DatalistColumn]
        [Display(Name = "Default")]
        public bool Default { get; set; }

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



    public class MasterItemUnitDatalist : MvcDatalist<MasterItemUnitViewModel>
    {
        private DbContext Context { get; }

        public MasterItemUnitDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.MasterUnitCode;
        }
        public MasterItemUnitDatalist()
        {
            Url = "/DatalistFilters/AllMasterItemUnit";
            Title = "Master Satuan";
            AdditionalFilters.Add("MasterItemId");

            Filter.Sort = "MasterUnitCode";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterItemUnitViewModel> GetModels()
        {
            return Context.Set<MasterItemUnit>()
                .Select(x => new MasterItemUnitViewModel
                {
                    Id = x.Id,
                    MasterItemId = x.MasterItemId,
                    MasterItem = x.MasterItem,
                    MasterUnitId = x.MasterUnitId,
                    MasterUnit = x.MasterUnit,
                    MasterUnitCode = x.MasterUnit.Code,
                    MasterUnitRatio = x.MasterUnit.Ratio,
                    MasterUnitNotes = x.MasterUnit.Notes,
                    Default = x.Default,
                    Active = x.Active,
                    Created = x.Created,
                    Updated = x.Updated,
                    UserId = x.UserId
                });
        }
    }

    public class HeaderMasterItemUnitDatalist : MvcDatalist<MasterItemUnitViewModel>
    {
        private DbContext Context { get; }

        public HeaderMasterItemUnitDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.MasterUnitCode;
        }
        public HeaderMasterItemUnitDatalist()
        {
            Url = "/DatalistFilters/AllHeaderMasterItemUnit";
            Title = "Master Satuan";
            AdditionalFilters.Add("HeaderMasterItemId");

            Filter.Sort = "MasterUnitCode";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterItemUnitViewModel> GetModels()
        {
            return Context.Set<MasterItemUnit>()
                .Select(x => new MasterItemUnitViewModel
                {
                    Id = x.Id,
                    MasterItemId = x.MasterItemId,
                    MasterItem = x.MasterItem,
                    MasterUnitId = x.MasterUnitId,
                    MasterUnit = x.MasterUnit,
                    MasterUnitCode = x.MasterUnit.Code,
                    MasterUnitRatio = x.MasterUnit.Ratio,
                    MasterUnitNotes = x.MasterUnit.Notes,
                    Default = x.Default,
                    Active = x.Active,
                    Created = x.Created,
                    Updated = x.Updated,
                    UserId = x.UserId
                });
        }
    }
}