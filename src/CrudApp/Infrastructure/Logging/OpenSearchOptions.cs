using System.ComponentModel.DataAnnotations;

namespace CrudApp.Infrastructure.Logging;

public class OpenSearchOptions
{
    [Required]
    public Uri BaseAddress { get; set; } = null!;
}
