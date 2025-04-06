namespace cnpm.ViewModels
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
    }
}
