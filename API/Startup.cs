using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Domínio.Interfaces;
using minimal_api.Domínio.ModelViews;
using minimal_api.Domínio.Serviços;
using MinimalApi;
using MinimalApi.Dominio.Entidade;
using MinimalApi.Dominio.Enuns;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Domínio.ModelViews;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration.GetSection("Jwt").ToString() ?? "";
    }

    private string key;

    public IConfiguration Configuration { get; set; }
    public void ConfigureServices(IServiceCollection services)
    {

        services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateLifetime = true,
        ValidateAudience = false,
        IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key))
    };
});

        services.AddAuthorization();
        services.AddScoped<IAdministradorServico, AdministradorServico>();
        services.AddScoped<IVeiculoServico, VeiculoServico>();

        services.AddDbContext<DbContexto>(options =>
            options.UseMySql(
                Configuration.GetConnectionString("mysql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("mysql"))
                )
        );

        services.AddEndpointsApiExplorer();
        //swagger com suporte a jwt

        services.AddSwaggerGen(option =>
        {   //configurando swagger para a passagem de token JWT
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Digite 'Bearer' [espaço] e então o token JWT.\n\nExemplo: \"Bearer 12345abcdef\""
            });

            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");

            #endregion

            #region Administradores

            string GerarTokenJWT(Administrador administrador)
            {
                if (string.IsNullOrEmpty(key))
                    throw new InvalidOperationException("JWT key não configurada");
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new Claim ("Email", administrador.Email),
                    new Claim ("Perfil", administrador.Perfil),
                    new Claim (ClaimTypes.Role, administrador.Perfil)
                };

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            ;

            endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
            {
                var adm = administradorServico.Login(loginDTO);
                if (adm != null)
                {
                    string token = GerarTokenJWT(adm);
                    return Results.Ok(new AdministradorLogado
                    {
                        Email = adm.Email,
                        Perfil = adm.Perfil,
                        Token = token
                    });

                }
                else
                    return Results.Unauthorized();
            }).AllowAnonymous().WithTags("Administrador");

            endpoints.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
            {
                var adms = new List<AdministradorModelView>();
                var administradores = administradorServico.Todos(pagina);
                foreach (var adm in administradores)
                {
                    adms.Add(new AdministradorModelView
                    {
                        Email = adm.Email,
                        Id = adm.Id,
                        Perfil = adm.Perfil
                    });
                }

                return Results.Ok(adms);

            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administrador");

            endpoints.MapGet("/Administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
            {
                var administrador = administradorServico.BuscaPorId(id);
                if (administrador == null) return Results.NotFound();
                return Results.Ok(new AdministradorModelView
                {
                    Email = administrador.Email,
                    Id = administrador.Id,
                    Perfil = administrador.Perfil
                });
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administrador");
            //add rotas por contexto/tags no swagger

            endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
            {
                var validacao = new ErrorMessage
                {
                    Mensagens = new List<string>()
                };

                if (string.IsNullOrEmpty(administradorDTO.Email))
                    validacao.Mensagens.Add("O campo Email não pode ser vazio");
                if (string.IsNullOrEmpty(administradorDTO.Senha))
                    validacao.Mensagens.Add("O campo Senha não pode ser vazio");
                if (administradorDTO.Perfil == null)
                    validacao.Mensagens.Add("O campo Perfil não pode ser vazio");

                if (validacao.Mensagens.Count > 0)
                    return Results.BadRequest(validacao);

                var adm = new Administrador
                {
                    Email = administradorDTO.Email,
                    Senha = administradorDTO.Senha,
                    Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
                };

                administradorServico.Incluir(adm);

                return Results.Created($"/administrador/{adm.Id}", new AdministradorModelView
                {
                    Email = adm.Email,
                    Id = adm.Id,
                    Perfil = adm.Perfil
                });

            }).RequireAuthorization()
            .WithTags("Administradores")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });

            #endregion

            #region Veiculos
            ErrorMessage validaDTO(VeiculoDTO veiculoDTO)
            {
                var validacao = new ErrorMessage
                {
                    Mensagens = new List<string>()
                };
                if (string.IsNullOrEmpty(veiculoDTO.Nome))
                    validacao.Mensagens.Add("O nome não pode ser vazio.");
                if (string.IsNullOrEmpty(veiculoDTO.Marca))
                    validacao.Mensagens.Add("A marca não pode ser vazio.");
                if (veiculoDTO.Ano < 1900)
                    validacao.Mensagens.Add("Ano de lançamento inválido.");

                return validacao;
            }

            endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
            {
                var validacao = validaDTO(veiculoDTO);
                if (validacao.Mensagens.Count > 0)
                    return Results.BadRequest(validacao);

                var veiculo = new Veiculo
                {
                    Nome = veiculoDTO.Nome,
                    Marca = veiculoDTO.Marca,
                    Ano = veiculoDTO.Ano
                };
                veiculoServico.Incluir(veiculo);

                return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
            }).RequireAuthorization()
            .WithTags("Veículos")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" });

            endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscaPorId(id);
                if (veiculo == null) return Results.NotFound();

                return Results.Ok(veiculo);
            }).RequireAuthorization()
            .WithTags("Veículos")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" });

            endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
            {
                var veiculos = veiculoServico.Todos(pagina);

                return Results.Ok(veiculos);
            }).RequireAuthorization()
            .WithTags("Veículos")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" });

            endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscaPorId(id);
                if (veiculo == null) return Results.NotFound();

                var validacao = validaDTO(veiculoDTO);
                if (validacao.Mensagens.Count > 0)
                    return Results.BadRequest(validacao);
                veiculo.Nome = veiculoDTO.Nome;
                veiculo.Ano = veiculoDTO.Ano;
                veiculo.Marca = veiculoDTO.Marca;

                veiculoServico.Atualizar(veiculo);

                return Results.Ok(veiculo);
            }).RequireAuthorization()
            .WithTags("Veículos")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });

            endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscaPorId(id);

                if (veiculo == null) return Results.NotFound();

                veiculoServico.Apagar(veiculo);

                return Results.NoContent();
            }).RequireAuthorization().WithTags("Veículos");
            #endregion
        });
        
    }
}