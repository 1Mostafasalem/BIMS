namespace BIMS.Application.Services.Authors
{
	public interface IAuthorService
	{
		IEnumerable<Author> GetAll();
		Author? GetById(int id);
		Author Add(string name, string CreatedById);
		Author? Update(int id, string name, string updatedById);
		Author? ToggleStatus(int id, string updatedById);
		bool AllowAuthor(int id, string name);
        IEnumerable<Author> GetActiveAuthors();

    }
}
