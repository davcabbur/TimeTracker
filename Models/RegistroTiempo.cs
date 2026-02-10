using System;

namespace TimeTracker.Models
{
    /// <summary>
    /// Representa un registro de tiempo dedicado a una tarea de un proyecto
    /// </summary>
    public class RegistroTiempo
    {
        public int Id { get; set; }
        public string Proyecto { get; set; } = string.Empty;
        public string Tarea { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public double Horas { get; set; }
    }
}
