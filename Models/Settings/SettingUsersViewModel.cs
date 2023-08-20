using System.ComponentModel.DataAnnotations;

namespace eShop.Models
{
    public class AssignedMasterBusinessUnitRegion
    {
        public int MasterBusinessUnitId { get; set; }

        public int MasterRegionId { get; set; }

        public string Title { get; set; }

        public bool Assigned { get; set; }
    }

    public class SettingUsersChangePassword
    {
        [Required(ErrorMessage = "ID harus diisi.")]
        public int Id { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password harus diisi.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}