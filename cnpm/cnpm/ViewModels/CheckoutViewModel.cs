using cnpm.Models;

namespace cnpm.ViewModels
{
    public class CheckoutViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public UserInformationViewModel UserInformation { get; set; }

    }

}
