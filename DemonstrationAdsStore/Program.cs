namespace DemonstrationAdsStore;

public class Program
{
    static WebApplication BuildApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSingleton<AdsStore>();
        builder.Services.AddControllers();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSwaggerGen();
        }

        return builder.Build();
    }

    public static void SetupApp(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "DemonstrationAdsStore API");
                options.RoutePrefix = string.Empty;
            });

            app.Use(async (context, next) =>
            {
                context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                context.Response.Headers.Pragma = "no-cache";
                context.Response.Headers.Expires = "0";

                await next();
            });
        }

        app.UseHttpsRedirection();
        app.MapControllers();
    }

    public static void Main(string[] args)
    {
        var app = BuildApp(args);

        SetupApp(app);

        app.Run();
    }
}
