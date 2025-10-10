using MinimalApi.Infraestrutura.Db;
using MinimalApi.Dominio.Entidade;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Domínio.Serviços;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Test.Domain.Entidades;

[TestClass]
public class AdministradorServicoTest
{
    private DbContexto CriarContextoDeTeste()
    {   //configurar o configurationBuilder
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

        var builder = new ConfigurationBuilder()
        .SetBasePath(path ?? Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

        var configuration = builder.Build();

        var context = new DbContexto(configuration);
        
        context.Database.EnsureCreated(); // Cria o schema no banco de teste

        return context;
        
    }

    [TestMethod]
    public void TestandoSalvarAdministrador()
    {
        //Para testes:
        //Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE administradores");

        var adm = new Administrador();
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";


        var admServico = new AdministradorServico(context);

        //Act
        admServico.Incluir(adm);

        //Assert
        Assert.AreEqual("teste@teste.com", adm.Email);
        Assert.AreEqual("teste", adm.Senha);
        Assert.AreEqual("Adm", adm.Perfil);
    }
    
     [TestMethod]
    public void TestandoBuscaPorId()
    {
        //Para testes:
        //Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE administradores");
        context.Database.ExecuteSqlRaw("ALTER TABLE administradores AUTO_INCREMENT = 1;");

        var adm = new Administrador();
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";
        
        
        var admServico = new AdministradorServico(context);

        //Act
        admServico.Incluir(adm);
        
        var admDoBanco = admServico.BuscaPorId(adm.Id);

        //Assert
        Assert.IsNotNull(admDoBanco);
        Assert.AreEqual(adm.Email,admDoBanco.Email);
    }
}
