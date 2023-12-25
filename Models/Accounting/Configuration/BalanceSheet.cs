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
        [Key]
        [Display(Name = "User")]
        public int AccountTypeId { get; set; }

        [Display(Name = "User")]
        public virtual AccountType AccountType { get; set; }
    }
}