namespace cnpm.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public ProductDetail ProductDetail { get; set; }
        public List<Product> OtherProducts { get; set; }
        public List<Review> Reviews { get; set; }
    }
}
