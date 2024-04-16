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

            CreateMap<Book, BookRowViewModel>()
                .ForMember(dest => dest.CategoriesList, opt => opt.MapFrom(src => src.Categories.Select(x => x.Category!.Name)))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.Name));


            CreateMap<BookViewModel, Book>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.SelectedCategories.Select(x => new BookCategory { CategoryId = x })));
            ;
            // model.SelectedCategories.ToList().ForEach(x => book.Categories.Add(new BookCategory { CategoryId = x }));

            //Book Copy
            CreateMap<BookCopy, BookCopyViewModel>()
                 .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title))
                 .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.Book!.Id))
                 .ForMember(dest => dest.BookThumbnailUrl, opt => opt.MapFrom(src => src.Book!.ImageThumbnailUrl))
                 .ReverseMap();

            CreateMap<Book, BookSearchResultViewModel>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.Name));

            //Users
            CreateMap<ApplicationUser, UserViewModel>();

            CreateMap<UserViewModel, ApplicationUser>()
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper()));

            CreateMap<UserViewModel, CreateUserDto>();

            //Governorates
            CreateMap<Governorate, SelectListItem>()
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id));

            //Areas
            CreateMap<Area, SelectListItem>()
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id));

            //Subscriber
            CreateMap<Subscriber, SubscriberViewModel>()
                .ReverseMap();

            CreateMap<Subscriber, SubscriberSearchResultViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => string.Concat(src.FirstName, " ", src.LastName)));

            CreateMap<Subscriber, SubscriberDetailViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area!.Name))
                .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Governorate!.Name));

            //Subscription
            CreateMap<Subscription, SubscriptionViewModel>()
                .ReverseMap();

            //Rental
            CreateMap<Rental, RentalViewModel>()
                .ReverseMap();

            //RentalCopy
            CreateMap<RentalCopy, RentalCopyViewModel>()
                .ReverseMap();
            CreateMap<RentalCopy, CopyHistoryViewModel>()
                .ForMember(dest => dest.SubscriberMobile, opt => opt.MapFrom(src => src.Rental!.Subscriber!.MobileNumber))
                .ForMember(dest => dest.SubscriberName, opt => opt.MapFrom(src => string.Concat(src.Rental!.Subscriber!.FirstName, " ", src.Rental!.Subscriber!.LastName)));

            CreateMap<ReturnCopyViewModel, ReturnCopyDto>();

        }
    }
}
