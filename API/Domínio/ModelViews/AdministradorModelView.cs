using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalApi.Domínio.ModelViews;

public record AdministradorModelView
{
    //model para ocultar senha ao obter as informações dos administradores 
    public string Email { get; set; } = default!;
    public string Perfil { get; set; } = default!;
    public int Id { get; set; }
}
