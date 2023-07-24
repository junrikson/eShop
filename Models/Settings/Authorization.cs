using Datalist;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace eShop.Models
{
    public class AssignedRoles
    {
        public virtual int Order { get; set; }

        public virtual int Id { get; set; }

        public virtual string Code { get; set; }

        public virtual string Notes { get; set; }

        public virtual bool Active { get; set; }

        public virtual bool Assigned { get; set; }
    }

    public class Authorization
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Kode otorisasi harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Otorisasi")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "Authorizations", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }

        public virtual ICollection<CustomRole> Roles { get; set; }

        public Authorization()
        {
            this.Roles = new HashSet<CustomRole>();
        }
    }

    public class AuthorizationDatalist : MvcDatalist<Authorization>
    {
        private DbContext Context { get; }

        public AuthorizationDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.Notes;
        }
        public AuthorizationDatalist()
        {
            Url = "AllAuthorization";
            Title = "Otorisasi";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<Authorization> GetModels()
        {
            return Context.Set<Authorization>();
        }
    }
}