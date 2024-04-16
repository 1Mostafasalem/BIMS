namespace BIMS.Application.Services.Categories
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetActiveCategories();
    }

}
