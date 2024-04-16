IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
insert into Categories (Name, IsDeleted, CreatedOn, CreatedById) values ('Drama', 0, GETDATE(), (select Id from dbo.AspNetUsers where UserName = 'archive'))
insert into Categories (Name, IsDeleted, CreatedOn, CreatedById) values ('Comedy', 0, GETDATE(), (select Id from dbo.AspNetUsers where UserName = 'archive'))
insert into Categories (Name, IsDeleted, CreatedOn, CreatedById) values ('Horror', 0, GETDATE(), (select Id from dbo.AspNetUsers where UserName = 'archive'))
insert into Categories (Name, IsDeleted, CreatedOn, CreatedById) values ('Romance', 0, GETDATE(), (select Id from dbo.AspNetUsers where UserName = 'archive'))
insert into Categories (Name, IsDeleted, CreatedOn, CreatedById) values ('Crime', 0, GETDATE(), (select Id from dbo.AspNetUsers where UserName = 'archive'))
insert into Categories (Name, IsDeleted, CreatedOn, CreatedById) values ('War', 0, GETDATE(), (select Id from dbo.AspNetUsers where UserName = 'archive'))
insert into Categories (Name, IsDeleted, CreatedOn, CreatedById) values ('Fantasy', 0, GETDATE(), (select Id from dbo.AspNetUsers where UserName = 'archive'))
insert into Categories (Name, IsDeleted, CreatedOn, CreatedById) values ('Adventure', 0, GETDATE(), (select Id from dbo.AspNetUsers where UserName = 'archive'))
insert into Categories (Name, IsDeleted, CreatedOn, CreatedById) values ('Thriller', 0, GETDATE(), (select Id from dbo.AspNetUsers where UserName = 'archive'))
insert into Categories (Name, IsDeleted, CreatedOn, CreatedById) values ('Action', 0, GETDATE(), (select Id from dbo.AspNetUsers where UserName = 'archive'))
END​
