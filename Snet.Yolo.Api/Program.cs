
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Snet.Yolo.Api.Handler;
using Snet.Yolo.Api.Model;
using Snet.Yolo.Server;
using Snet.Yolo.Server.handler;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;

namespace Snet.Yolo.Api
{
    public class DictionaryTKeyEnumTValueSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema is not OpenApiSchema concrete)
            {
                return;
            }

            // Only run for fields that are a Dictionary<Enum, TValue>
            if (!context.Type.IsGenericType || !context.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
            {
                return;
            }

            var genericArgs = context.Type.GetGenericArguments();
            var keyType = genericArgs[0];
            var valueType = genericArgs[1];

            if (!keyType.IsEnum)
            {
                return;
            }

            concrete.Type = JsonSchemaType.Object;
            concrete.Properties = keyType.GetEnumNames().ToDictionary(
                name => name,
                name => context.SchemaGenerator.GenerateSchema(valueType, context.SchemaRepository));
        }
    }
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            IConfiguration configuration = builder.Configuration.GetSection("ConfigModel");
            ConfigModel config = configuration.Get<ConfigModel>();
            HistoryFileHandler handler = HistoryFileHandler.Instance(config.BasePath);
            handler.SetConfig(config);
            _ = handler.DeleteLogicAsync(CancellationToken.None);

            builder.Services.Configure<ConfigModel>(configuration);

            builder.Services.AddSingleton(new PoseEstimationCustomKeyPointColorHandler());

            builder.Services.AddSingleton(ManageOperate.Instance(PublicHandler.DefaultSN));
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = 1L * 1024 * 1024 * 1024;
            });
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 1L * 1024 * 1024 * 1024;
            });
            builder.Services.Configure<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
            });
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Snet", Version = "v1" });
                opt.SchemaFilter<DictionaryTKeyEnumTValueSchemaFilter>();
                opt.DescribeAllParametersInCamelCase();
                opt.IgnoreObsoleteActions();
                opt.IgnoreObsoleteProperties();
                foreach (var file in Directory.GetFiles(Path.GetDirectoryName(typeof(Program).Assembly.Location)))
                {
                    if (Path.GetExtension(file).Equals(".xml", StringComparison.CurrentCultureIgnoreCase))
                    {
                        opt.IncludeXmlComments(file, true);
                    }
                }
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
            builder.Services.AddControllers();
            var app = builder.Build();

            // 测试环境可以访问
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            //正式环境可以访问
            if (app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
