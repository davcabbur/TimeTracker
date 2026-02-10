using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    /// <summary>
    /// Servicio para generar informes PDF
    /// </summary>
    public class PdfService
    {
        public void GenerarInformePdf(
            string rutaArchivo,
            List<RegistroTiempo> registros,
            List<ResumenProyecto> resumenesProyecto,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            double totalHoras,
            string proyectoConMasHoras,
            string tareaConMasHoras)
        {
            // Configurar licencia de QuestPDF para uso comunitario
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    // Encabezado
                    page.Header()
                        .Text("Informe de Productividad - Time Tracker")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken3);

                    // Pie de página
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });

                    // Contenido
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            // Página 1: Resumen General
                            col.Item().Text("RESUMEN GENERAL").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                            col.Item().PaddingBottom(10);

                            col.Item().Text($"Período analizado: {FormatearPeriodo(fechaDesde, fechaHasta)}").FontSize(12);
                            col.Item().PaddingBottom(5);

                            col.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(c =>
                            {
                                c.Item().Text($"Total de horas trabajadas: {totalHoras:F1} horas").Bold();
                                c.Item().Text($"Proyecto con más horas: {proyectoConMasHoras}");
                                c.Item().Text($"Tarea con más horas: {tareaConMasHoras}");
                            });

                            col.Item().PaddingVertical(20);

                            // Página 2: Resumen por Proyecto
                            col.Item().PageBreak();
                            col.Item().Text("RESUMEN POR PROYECTO").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                            col.Item().PaddingBottom(10);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                // Encabezados
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Proyecto").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Total Horas").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Nº Tareas").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Nº Registros").FontColor(Colors.White).Bold();
                                });

                                // Datos
                                foreach (var resumen in resumenesProyecto)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(resumen.Proyecto);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{resumen.TotalHoras:F1}");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(resumen.NumeroDeTareas.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(resumen.NumeroDeRegistros.ToString());
                                }
                            });

                            // Página 3: Tareas del Proyecto con Más Horas
                            if (resumenesProyecto.Any())
                            {
                                col.Item().PaddingVertical(20);
                                col.Item().PageBreak();

                                var proyectoTop = resumenesProyecto.First();
                                var tareasProyectoTop = registros
                                    .Where(r => r.Proyecto == proyectoTop.Proyecto)
                                    .GroupBy(r => r.Tarea)
                                    .Select(g => new ResumenTarea
                                    {
                                        Tarea = g.Key,
                                        TotalHoras = g.Sum(r => r.Horas),
                                        NumeroRegistros = g.Count()
                                    })
                                    .OrderByDescending(t => t.TotalHoras)
                                    .ToList();

                                col.Item().Text($"TAREAS DEL PROYECTO: {proyectoTop.Proyecto}").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                                col.Item().PaddingBottom(10);

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    // Encabezados
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Tarea").FontColor(Colors.White).Bold();
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Total Horas").FontColor(Colors.White).Bold();
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Nº Registros").FontColor(Colors.White).Bold();
                                    });

                                    // Datos
                                    foreach (var tarea in tareasProyectoTop)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(tarea.Tarea);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{tarea.TotalHoras:F1}");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(tarea.NumeroRegistros.ToString());
                                    }
                                });
                            }
                        });
                });
            })
            .GeneratePdf(rutaArchivo);
        }

        private string FormatearPeriodo(DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue)
                return $"{desde.Value:dd/MM/yyyy} - {hasta.Value:dd/MM/yyyy}";
            if (desde.HasValue)
                return $"Desde {desde.Value:dd/MM/yyyy}";
            if (hasta.HasValue)
                return $"Hasta {hasta.Value:dd/MM/yyyy}";
            return "Todos los registros";
        }
    }
}
