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
    public class AccountType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor Jenis Akun harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Jenis Akun")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "AccountTypes", AdditionalFields = "Id", ErrorMessage = "Nomor Jenis Akun sudah ada.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Nama Jenis Akun")]
        [StringLength(256, ErrorMessage = "Maksimal 128 huruf.")]
        [Required(ErrorMessage = "Nama Jenis Akun harus diisi.")]
        public string Name { get; set; }

        [Display(Name = "Header")]
        public bool IsHeader { get; set; }

        [Display(Name = "Sub Jenis Akun")]
        public int? SubAccountTypeId { get; set; }

        [Display(Name = "Sub Jenis Akun")]
        public virtual AccountType SubAccountType { get; set; }

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

    public class AccountTypeDatalist : MvcDatalist<AccountType>
    {
        private DbContext Context { get; }

        public AccountTypeDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.Name;
        }
        public AccountTypeDatalist()
        {
            Url = "/DatalistFilters/AllAccountType";
            Title = "Jenis Akun";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<AccountType> GetModels()
        {
            return Context.Set<AccountType>();
        }
    }

    public class AccountTypeDetailsDatalist : MvcDatalist<AccountType>
    {
        private DbContext Context { get; }

        public AccountTypeDetailsDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.Name;
        }
        public AccountTypeDetailsDatalist()
        {
            Url = "/DatalistFilters/AllAccountTypeDetails";
            Title = "Jenis Akun";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<AccountType> GetModels()
        {
            return Context.Set<AccountType>();
        }
    }
}