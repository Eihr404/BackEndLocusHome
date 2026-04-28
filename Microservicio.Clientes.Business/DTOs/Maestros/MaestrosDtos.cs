namespace Microservicio.Clientes.Business.DTOs.Maestros;

public class CiudadDto
{
    public int CiudadId { get; set; }
    public string? Nombre { get; set; }
    public bool EsCapital { get; set; }
    public int PaisId { get; set; }
}

public class MonedaDto
{
    public int MonedaId { get; set; }
    public string? Nombre { get; set; }
    public string? Simbolo { get; set; }
    public string? Codigo { get; set; }
}

public class TipoAlojamientoDto
{
    public int TipoAlojamientoId { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
}

public class InstalacionDto
{
    public int InstalacionId { get; set; }
    public string? Nombre { get; set; }
    public string? Icono { get; set; }
}
