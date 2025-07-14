using System.ComponentModel.DataAnnotations;

namespace Prock.Backend.src.Data.MariaDb;
public class Widget
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
// This class represents a simple widget entity with properties for Id, Name, and Description.