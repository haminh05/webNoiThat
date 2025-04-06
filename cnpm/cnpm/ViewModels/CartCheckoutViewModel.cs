namespace cnpm.ViewModels
{
    public class CartCheckoutViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new();
        public UserInformationViewModel UserInformation { get; set; } = new();
    }

}
