
namespace CrudApp.SuperHeroes;

public sealed class SuperHero : EntityBase
{
    public string SuperHeroName { get; set; }
    public string CivilName { get; set; }
    public override string DisplayName => string.Concat(SuperHeroName, "/", CivilName);
}
