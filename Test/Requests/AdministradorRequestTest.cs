using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using minimal_api.Domínio.ModelViews;
using MinimalApi.Dominio.Entidade;
using MinimalApi.DTOs;
using Test.Helpers;

namespace Test.Requests;

[TestClass]
public sealed class AdministradorRequestTest
{
    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        Setup.ClassInit(testContext);
    }

    [ClassCleanup]
    public static void ClassCleanUp()
    {
        Setup.ClassCleanUp();
    }

    [TestMethod]
    public async Task TestarGetSetPropriedades()
    {
        //Para testes:
        //Arrange
        var loginDTO = new LoginDTO
        {
            Email = "administrador@teste.com",
            Senha = "123456"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginDTO),
            Encoding.UTF8, "Application/json");
        //Act
        var response = await Setup.client.PostAsync("/administradores/login", content);

        //Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.IsNotNull(admLogado);
        Assert.IsNotNull(admLogado.Id);
        Assert.IsNotNull(admLogado.Email ?? "");
        Assert.IsNotNull(admLogado.Token ?? "");

    }
}
