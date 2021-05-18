// using Core.WebApi.Middlewares.ExceptionHandling;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Warehouse.Storage;
// using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
//
// namespace Warehouse.Api.Tests
// {
//     public static class WarehouseTestWebHostBuilder
//     {
//         public static IWebHostBuilder Configure(IWebHostBuilder webHostBuilder, string schemaName)
//         {
//             webHostBuilder
//                 .ConfigureServices(services =>
//                 {
//                     services.AddRouting()
//                         .AddWarehouseServices()
//                         .AddTransient<DbContextOptions<WarehouseDBContext>>(s =>
//                         {
//                             var connectionString = s.GetRequiredService<IConfiguration>().GetConnectionString("WarehouseDB");
//                             var options = new DbContextOptionsBuilder<WarehouseDBContext>();
//                             options.UseNpgsql(
//                                 $"{connectionString}; searchpath = {schemaName.ToLower()}",
//                                 x => x.MigrationsHistoryTable("__EFMigrationsHistory", schemaName.ToLower()));
//                             return options.Options;
//                         });
//                 })
//                 .Configure(app =>
//                 {
//                     app.UseMiddleware(typeof(ExceptionHandlingMiddleware))
//                         .UseRouting()
//                         .UseEndpoints(endpoints => { endpoints.UseWarehouseEndpoints(); })
//                         .ConfigureWarehouse();
//
//                     // Kids, do not try this at home!
//                     var database = app.ApplicationServices.GetRequiredService<WarehouseDBContext>().Database;
//                     database.ExecuteSqlRaw("TRUNCATE TABLE \"Product\"");
//                 });
//
//             return webHostBuilder;
//         }
//     }
// }
