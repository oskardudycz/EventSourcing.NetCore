namespace ECommerce.Api

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Prometheus
open Serilog

type Startup() =

    member _.ConfigureServices(services : IServiceCollection) : unit =
        services.AddMvc() |> ignore
        services.AddControllers()
            .AddNewtonsoftJson() |> ignore
            // TODO AddSwaggerGen

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member _.Configure(app : IApplicationBuilder, env : IHostEnvironment) : unit =
        if env.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore

        app
            .UseRouting()
            //.UseAuthorization()
            .UseSerilogRequestLogging() // see https://nblumhardt.com/2019/10/serilog-in-aspnetcore-3/
            .UseEndpoints(fun endpoints ->
                endpoints.MapControllers() |> ignore
                endpoints.MapMetrics() |> ignore)
            |> ignore
//        app.UseSwagger();
//        app.UseSwaggerUI
