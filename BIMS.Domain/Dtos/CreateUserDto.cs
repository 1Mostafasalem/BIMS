namespace BIMS.Domain.Dtos;
public class CreateUserDto
{
    public string FullName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public IList<string> RolseList = new List<string>();
}