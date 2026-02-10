namespace TimeTracker.Models
{
    /// <summary>
    /// Resumen agregado de estadísticas por tarea
    /// </summary>
    public class ResumenTarea
    {
        public string Tarea { get; set; } = string.Empty;
        public double TotalHoras { get; set; }
        public int NumeroRegistros { get; set; }
    }
}
