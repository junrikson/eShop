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
    public class FixedAssetCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Kode Kategori Aktiva harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Kategori Aktiva")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "FixedAssetsCategories", AdditionalFields = "Id", ErrorMessage = "This code has been used.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Kode Kategori Aktiva Tetap harus diisi.")]
        [Display(Name = "Nama Kategori Aktiva")]
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

    public class FixedAssetCategoryDatalist : MvcDatalist<FixedAssetCategory>
    {
        private DbContext Context { get; }

        public FixedAssetCategoryDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.Name;
        }
        public FixedAssetCategoryDatalist()
        {
            Url = "/DatalistFilters/AllFixedAssetCategory";
            Title = "Kategori Aktiva";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<FixedAssetCategory> GetModels()
        {
            return Context.Set<FixedAssetCategory>();
        }
    }


}