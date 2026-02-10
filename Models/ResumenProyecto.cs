namespace TimeTracker.Models
{
    /// <summary>
    /// Resumen agregado de estadísticas por proyecto
    /// </summary>
    public class ResumenProyecto
    {
        public string Proyecto { get; set; } = string.Empty;
        public double TotalHoras { get; set; }
        public int NumeroDeTareas { get; set; }
        public int NumeroDeRegistros { get; set; }
    }
}
