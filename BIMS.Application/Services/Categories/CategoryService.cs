
namespace BIMS.Application.Services.Categories
{
    internal class CategoryService: ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Category> GetActiveCategories() => _unitOfWork.Categories.FindAll(predicate: a => !a.IsDeleted, orderBy: a => a.Name, OrderBy.Ascending);
    }
}
