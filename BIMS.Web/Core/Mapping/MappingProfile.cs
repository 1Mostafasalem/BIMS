namespace BIMS.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //CreateMap<Source,Destination>();
            //Categories
            CreateMap<Category, CategoryViewModel>().ReverseMap();
            CreateMap<Category, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            //Authors
            CreateMap<Author, AuthorViewModel>().ReverseMap();
            CreateMap<Author, SelectListItem>()
                 .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));



            //Books
            CreateMap<Book, BookViewModel>()
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.CategoriesList, opt => opt.MapFrom(src => src.Categories.Select(x => x.Category!.Name)))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.Name));


            CreateMap<BookViewModel, Book>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.SelectedCategories.Select(x => new BookCategory { CategoryId = x })));
            ;
            // model.SelectedCategories.ToList().ForEach(x => book.Categories.Add(new BookCategory { CategoryId = x }));

            //Book Copy
            CreateMap<BookCopy, BookCopyViewModel>()
                 .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title))
                 .ReverseMap();


            //Users
            CreateMap<ApplicationUser, UserViewModel>();

            CreateMap<UserViewModel,ApplicationUser>()
                .ForMember(dest => dest.NormalizedEmail , opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedUserName , opt => opt.MapFrom(src => src.UserName.ToUpper()));

        }
    }
}
