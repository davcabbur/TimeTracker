using System;
using System.Collections.Generic;
using System.Linq;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    /// <summary>
    /// Servicio para gestionar datos de registros de tiempo
    /// Proporciona datos mock para demostración
    /// </summary>
    public class DataService
    {
        private List<RegistroTiempo> _registros;

        public DataService()
        {
            _registros = GenerarDatosMock();
        }

        /// <summary>
        /// Obtiene todos los registros de tiempo
        /// </summary>
        public List<RegistroTiempo> ObtenerTodosLosRegistros()
        {
            return _registros;
        }

        /// <summary>
        /// Filtra registros por proyecto y rango de fechas
        /// </summary>
        public List<RegistroTiempo> FiltrarRegistros(string? proyecto, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var query = _registros.AsEnumerable();

            if (!string.IsNullOrEmpty(proyecto) && proyecto != "Todos")
            {
                query = query.Where(r => r.Proyecto == proyecto);
            }

            if (fechaDesde.HasValue)
            {
                query = query.Where(r => r.Fecha >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(r => r.Fecha <= fechaHasta.Value);
            }

            return query.ToList();
        }

        /// <summary>
        /// Calcula resúmenes por proyecto a partir de registros filtrados
        /// </summary>
        public List<ResumenProyecto> CalcularResumenesPorProyecto(List<RegistroTiempo> registros)
        {
            return registros
                .GroupBy(r => r.Proyecto)
                .Select(g => new ResumenProyecto
                {
                    Proyecto = g.Key,
                    TotalHoras = g.Sum(r => r.Horas),
                    NumeroDeTareas = g.Select(r => r.Tarea).Distinct().Count(),
                    NumeroDeRegistros = g.Count()
                })
                .OrderByDescending(r => r.TotalHoras)
                .ToList();
        }

        /// <summary>
        /// Calcula resúmenes por tarea para un proyecto específico
        /// </summary>
        public List<ResumenTarea> CalcularResumenesPorTarea(List<RegistroTiempo> registros, string proyecto)
        {
            return registros
                .Where(r => r.Proyecto == proyecto)
                .GroupBy(r => r.Tarea)
                .Select(g => new ResumenTarea
                {
                    Tarea = g.Key,
                    TotalHoras = g.Sum(r => r.Horas),
                    NumeroRegistros = g.Count()
                })
                .OrderByDescending(r => r.TotalHoras)
                .ToList();
        }

        /// <summary>
        /// Obtiene la lista de proyectos únicos
        /// </summary>
        public List<string> ObtenerProyectos()
        {
            var proyectos = _registros.Select(r => r.Proyecto).Distinct().OrderBy(p => p).ToList();
            proyectos.Insert(0, "Todos");
            return proyectos;
        }

        /// <summary>
        /// Genera datos mock para demostración
        /// </summary>
        private List<RegistroTiempo> GenerarDatosMock()
        {
            var registros = new List<RegistroTiempo>();
            var random = new Random(42); // Seed fijo para consistencia
            var proyectos = new[] { "Sistema CRM", "Aplicación Móvil", "Portal Web", "API REST", "Dashboard Analytics" };
            var tareasPorProyecto = new Dictionary<string, string[]>
            {
                ["Sistema CRM"] = new[] { "Diseño UI/UX", "Desarrollo Frontend", "Desarrollo Backend", "Testing", "Documentación" },
                ["Aplicación Móvil"] = new[] { "Prototipado", "Desarrollo iOS", "Desarrollo Android", "Integración API", "Testing QA" },
                ["Portal Web"] = new[] { "Análisis Requisitos", "Diseño Responsive", "Desarrollo", "Optimización SEO", "Deploy" },
                ["API REST"] = new[] { "Arquitectura", "Endpoints", "Autenticación", "Documentación Swagger", "Testing Unitario" },
                ["Dashboard Analytics"] = new[] { "Diseño Gráficos", "Integración Datos", "Desarrollo Frontend", "Optimización", "Testing" }
            };

            int id = 1;
            var fechaBase = DateTime.Now.AddMonths(-3);

            // Generar registros para los últimos 3 meses
            for (int dia = 0; dia < 90; dia++)
            {
                var fecha = fechaBase.AddDays(dia);
                
                // Saltar fines de semana
                if (fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                // Generar entre 3-6 registros por día laboral
                int numRegistros = random.Next(3, 7);
                for (int i = 0; i < numRegistros; i++)
                {
                    var proyecto = proyectos[random.Next(proyectos.Length)];
                    var tareas = tareasPorProyecto[proyecto];
                    var tarea = tareas[random.Next(tareas.Length)];
                    var horas = Math.Round(0.5 + random.NextDouble() * 3.5, 1); // Entre 0.5 y 4.0 horas

                    registros.Add(new RegistroTiempo
                    {
                        Id = id++,
                        Proyecto = proyecto,
                        Tarea = tarea,
                        Fecha = fecha,
                        Horas = horas
                    });
                }
            }

            return registros;
        }
    }
}
