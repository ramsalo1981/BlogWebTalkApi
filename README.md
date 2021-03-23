# BlogWebTalkApi
This is ASP.NET Core in .NET 5.0 
1- you need to download nuGet package:
- Microsoft.AspNetCore.Mvc.NewtonsoftJson
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.Extensions.Configuration.NewtonsoftJson
- Microsoft.VisualStudio.Web.CodeGeneration.Design
- Newtonsoft.Json
- Swashbuckle.AspNetCore

2- Open file appsettings.json 
3- Add and change database servername ((localdb)\\MSSQLLocalDB) to yours
"ConnectionStrings": {
    "ConnStr": "server=(localdb)\\MSSQLLocalDB;Initial Catalog=BlogWebTalkApiDb;Integrated Security=True"
  }
4- open startup.cs file:
      Add to ConfigureServices this:  services.AddDbContext<BlogWebTalkApiDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ConnStr")));

Write in Package manage Console this command (add-migration) then write ( update-database)
