using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinimalApi.Dominio.Entidade;

namespace Test.Domain.Entidades;

[TestClass]
public sealed class AdministradorTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        //Para testes:
        //Arrange
        var adm = new Administrador();

        //Act
        adm.Id = 1;
        adm.Email = "testando@teste.com";
        adm.Senha = "larissamanoela";
        adm.Perfil = "Adm";

        //Assert
        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("testando@teste.com", adm.Email);
        Assert.AreEqual("larissamanoela", adm.Senha);
        Assert.AreEqual("Adm", adm.Perfil);
    }
}
