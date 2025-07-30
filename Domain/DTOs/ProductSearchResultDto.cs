namespace Domain.DTOs
{
    public class ProductSearchResultDto
    {
        public int RequestId { get; set; }
        public List<ProductDto> Products { get; set; }
    }
}
