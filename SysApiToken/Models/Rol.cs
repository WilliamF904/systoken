using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace SysApiToken.Models;

public partial class Rol
{
    [Key]
    public int Id { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string Nombre { get; set; } = null!;

    [InverseProperty("IdRolv")]
    [JsonIgnore]
    public virtual ICollection<Usuario> Usuario { get; set; } = new List<Usuario>();
}
