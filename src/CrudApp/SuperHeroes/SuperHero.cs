namespace CrudApp.SuperHeroes;

public sealed class SuperHero : EntityBase
{
    public string? HeroName { get; set; }
    public string? CivilianName { get; set; }
    public override string DisplayName => string.Concat(HeroName, "/", CivilianName);
}
