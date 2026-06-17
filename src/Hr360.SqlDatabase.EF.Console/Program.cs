// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


//dotnet.exe ef dbcontext scaffold "Server=xxx,1433;Initial Catalog=hr360;Persist Security Info=False;User ID=xxx;Password=xxxxx;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" Microsoft.EntityFrameworkCore.SqlServer -c Hr360DbContext --force --startup-project .  --verbose --no-onconfiguring