using System.ComponentModel.DataAnnotations;

namespace cnpm.ViewModels
{
    public class UserInformationViewModel
    {
        public int UserInformationId { get; set; } 
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Họ và tên không được chứa số hoặc ký tự đặc biệt.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải có 10 chữ số và bắt đầu bằng 0.")]
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public int UserId { get; set; }
    }

}
