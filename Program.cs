using EducaMente.AccessData;
using EducaMente.Interface;
using EducaMente.Models;
using EducaMente.Repositories;
using EducaMente.Services;
using EducaMente.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiRest", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Autrorizacion header usando Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below. \r\n\r\nExample: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    c.EnableAnnotations(); // Habilita soporte para anotaciones en Swagger

});

// Registro del AccesData
builder.Services.AddScoped<AccesoData>(provider =>
{
    // Configuracin de la cadena de conexin
    string connectionString = builder.Configuration.GetConnectionString("SQL");
    return new AccesoData(connectionString);
});

//Aquí inyectaré los servicios
builder.Services.AddScoped<I_WebService, WebServiceRepos>();
builder.Services.AddScoped<I_CertiEnvios, CertiEnviosRepos>();
builder.Services.AddScoped<I_Parametro, ParametroRepos>();
builder.Services.AddScoped<I_Promt, PromtRepos>();
builder.Services.AddScoped<I_TipoUsuario, TipoUsuarioRepos>();
builder.Services.AddScoped<I_Usuario, UsuarioRepos>();
builder.Services.AddScoped<I_PsicoPreguntaBank, PsicoPreguntaBankRepos>();
builder.Services.AddScoped<I_PsicoTest, PsicoTestRepos>();
builder.Services.AddScoped<I_CampainRisk, CampaingRiskRepos>();
builder.Services.AddScoped<I_OTP, OTPRepos>();

builder.Services.AddScoped<I_GeminiService, GeminiService>();
builder.Services.AddScoped<I_FinalizeTestOrquestador, FinalizeTestOrquestadorService>();
builder.Services.AddScoped<I_ExcelExporterService, ExcelExporterService>();

//Mensajería
builder.Services.AddScoped<NotificationManager>();

// Habilitar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

//Desabilitando el manejo global de validaciones en los endpoint para manejarlo mas personalizado y estandar
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

//Seguridad
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:ClaveSecreta"]))
    };
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// Configuracin de encriptacin
Encriptacion.CargarConfiguracion(builder.Configuration);
// Registrar la configuracin de encriptacin en la inyeccin de dependencias
builder.Services.Configure<CrytographySettings>(builder.Configuration.GetSection("CrytographySettings"));

builder.Services.AddHttpContextAccessor();

var certiEnviosUrl = builder.Configuration["Services:CertiEnviosBaseUrl"];

builder.Services.AddHttpClient("CertiEnviosClient", client =>
{
    client.BaseAddress = new Uri(certiEnviosUrl);
});
builder.Services.AddHttpClient("GeminiClient");

var app = builder.Build();

// Usar CORS
app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Agrega UseRouting antes de aplicar CORS
app.UseRouting();

//Para manejar las excepciones de los Endpoint para estandarizarlas
app.UseMiddleware<ApiExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
