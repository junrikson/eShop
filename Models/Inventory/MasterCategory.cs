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
    public class MasterCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Kode Kategori harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Kategori")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "MasterCategories", AdditionalFields = "Id", ErrorMessage = "This code has been used.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nama Kategori harus diisi.")]
        [Display(Name = "Nama Kategori")]
        [StringLength(256, ErrorMessage = "Maksimal 128 huruf.")]
        public string Name { get; set; }

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

    public class MasterCategoryDatalist : MvcDatalist<MasterCategory>
    {
        private DbContext Context { get; }

        public MasterCategoryDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code;
        }
        public MasterCategoryDatalist()
        {
            Url = "/DatalistFilters/AllMasterCategory";
            Title = "Kategori Barang";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterCategory> GetModels()
        {
            return Context.Set<MasterCategory>();
        }
    }
}