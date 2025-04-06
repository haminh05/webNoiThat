using System.ComponentModel.DataAnnotations;

namespace cnpm.ViewModels
{
    public class CreateEmployeeViewModel
    {
        

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải có 10 chữ số và bắt đầu bằng 0")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DateRange150Years(ErrorMessage = "Ngày sinh không hợp lệ, phải trong vòng 150 năm trở lại đây và không quá ngày hiện tại")]
        public DateOnly DateOfBirth { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Chức vụ là bắt buộc")]
        public string Position { get; set; }

        [Required(ErrorMessage = "CMND/CCCD là bắt buộc")]
        [RegularExpression(@"^\d{9}|\d{12}$", ErrorMessage = "CMND/CCCD phải là 9 hoặc 12 chữ số")]
        public string IdentityCard { get; set; }
    }
}