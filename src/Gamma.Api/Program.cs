using Gamma.Ai;
using Gamma.Core.Abstractions;
using Gamma.Core.Models;
using Gamma.Rendering.Commercial;
using Gamma.Rendering.OpenSource;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Accept/emit enum values as their names ("TitleAndBullets") rather
        // than integers, both in request bodies and query strings — much
        // friendlier for anyone calling this API by hand.
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.Configure<RenderingOptions>(builder.Configuration.GetSection(RenderingOptions.SectionName));
builder.Services.AddSingleton<RendererResolver>();

builder.Services.AddGammaAi(builder.Configuration);
builder.Services.AddOpenSourceRendering();
builder.Services.AddCommercialRendering();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
