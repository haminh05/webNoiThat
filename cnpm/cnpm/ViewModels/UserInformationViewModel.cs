namespace cnpm.ViewModels
{
    public class UserInformationViewModel
    {
        public int UserInformationId { get; set; } // Thêm dòng này nếu thiếu
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public int UserId { get; set; }
    }

}
