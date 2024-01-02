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
    public class BalanceSheet
    {
        [Key,ForeignKey("AccountType")]
        [Display(Name = "Jenis Akun")]
        public int Id { get; set; }

        [Display(Name = "Jenis Akun")]
        public virtual AccountType AccountType { get; set; }
    }
}