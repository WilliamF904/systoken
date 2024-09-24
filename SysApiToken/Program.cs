using Microsoft.EntityFrameworkCore;
using SysApiToken.Models;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SysApiToken.Auth;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();



//-----------------------------------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------------------------

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SysApiToken", Version = "v1" });
    // * Incluir  JWT Authentication *
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Ingresar tu token de JWT Authentication",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } });
    // **************
});

//---------------------------------------------------

builder.Services.AddDbContext<BdContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("cadenaSQL")));

//----------------------------------------------------

string _key = "ESFE2024SecretKeyForTokenAuthentication";
//builder.Services.AddSingleton(new JwtAuthentication(_key));

//---------------------------------------------------

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
 .AddJwtBearer(x =>
 {
     x.RequireHttpsMetadata = false;
     x.SaveToken = true;
     x.TokenValidationParameters = new TokenValidationParameters
     {
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_key)),
         ValidateAudience = false,
         ValidateIssuerSigningKey = true,
         ValidateIssuer = false
     };
 });

//-----------------------------------------------------

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});


//----------------------------------------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------------------------




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//---------------------------------------------------
app.UseAuthentication(); //---------------------
//---------------------------------------------------
app.UseAuthorization();

app.MapControllers();

app.Run();
