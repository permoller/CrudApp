using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CrudApp.SuperHeroes;

public sealed class SuperHero : EntityBase
{
    [Required]
    public string CivilName { get; set; }
}
