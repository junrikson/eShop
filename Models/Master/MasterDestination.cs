using Datalist;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;


namespace eShop.Models
{
    public class MasterDestination
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Kode Destination harus diisi.")]
        [Display(Name = "Kode Kota Tujuan")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nama Kota Tujuan harus diisi.")]
        [Display(Name = "Nama Kota Tujuan")]
        [StringLength(256, ErrorMessage = "Maksimal 128 huruf.")]
        public string Name { get; set; }

        [DatalistColumn]
        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

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

    public class MasterDestinationDatalist : MvcDatalist<MasterDestination>
    {
        private DbContext Context { get; }

        public MasterDestinationDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.Name;
        }
        public MasterDestinationDatalist()
        {
            Url = "/DatalistFilters/AllMasterDestination";
            Title = "Kota Tujuan";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterDestination> GetModels()
        {
            return Context.Set<MasterDestination>();
        }
    }
}