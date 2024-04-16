namespace BIMS.Application.Services.Authors
{
    internal class AuthorService : IAuthorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Author Add(string name, string CreatedById)
        {
            Author author = new()
            {
                Name = name,
                CreatedById = CreatedById
            };

            _unitOfWork.Authors.Add(author);
            _unitOfWork.Commit();
            return author;
        }

        public bool AllowAuthor(int id, string name)
        {
            var authors = _unitOfWork.Authors.Find(c => c.Name == name);
            var isAllowed = (authors is null || authors.Id == id);
            return isAllowed;
        }

        public IEnumerable<Author> GetAll() => _unitOfWork.Authors.GetAll();

        public Author? GetById(int id)
        {
            var author = _unitOfWork.Authors.GetById(id);
            return author;

        }

        public Author? ToggleStatus(int id, string updatedById)
        {
            var author = _unitOfWork.Authors.GetById(id);

            if (author is null)
                return null;

            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedById = updatedById;
            author.LastUpdatedOn = DateTime.Now;

            _unitOfWork.Commit();

            return author;
        }

        public Author? Update(int id, string name, string updatedById)
        {
            var author = _unitOfWork.Authors.GetById(id);

            if (author is null)
                return null;

            author.Name = name;
            author.LastUpdatedById = updatedById;
            author.LastUpdatedOn = DateTime.Now;

            _unitOfWork.Commit();

            return author;
        }
        public IEnumerable<Author> GetActiveAuthors() => _unitOfWork.Authors.FindAll(predicate: a => !a.IsDeleted, orderBy: a => a.Name, OrderBy.Ascending);
    }
}
