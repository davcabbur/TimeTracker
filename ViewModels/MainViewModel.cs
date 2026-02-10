using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TimeTracker.Models;
using TimeTracker.Services;
using Microsoft.Win32;
using System.Windows;

namespace TimeTracker.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DataService _dataService;
        private readonly PdfService _pdfService;

        // Aqui guardamos las variables para filtrar las cosas
        private string? _proyectoSeleccionado;
        public string? ProyectoSeleccionado
        {
            get => _proyectoSeleccionado;
            set { _proyectoSeleccionado = value; OnPropertyChanged(); }
        }

        private DateTime? _fechaDesde;
        public DateTime? FechaDesde
        {
            get => _fechaDesde;
            set { _fechaDesde = value; OnPropertyChanged(); }
        }

        private DateTime? _fechaHasta;
        public DateTime? FechaHasta
        {
            get => _fechaHasta;
            set { _fechaHasta = value; OnPropertyChanged(); }
        }

        // Listas donde guardamos los datos para mostrarlos
        public ObservableCollection<RegistroTiempo> RegistrosTiempo { get; } = new();
        public ObservableCollection<ResumenProyecto> ResumenesProyecto { get; } = new();
        public ObservableCollection<ResumenTarea> ResumenesTarea { get; } = new();
        public ObservableCollection<string> Proyectos { get; } = new();

        // Cosas para pintar los graficos bonitos
        public ObservableCollection<ResumenProyecto> DatosGraficoProyectos { get; } = new();
        public ObservableCollection<ResumenTarea> DatosGraficoTareas { get; } = new();

        // Variables para los numeritos de resumen
        private double _totalHoras;
        public double TotalHoras
        {
            get => _totalHoras;
            set { _totalHoras = value; OnPropertyChanged(); }
        }

        private string _proyectoConMasHoras = "-";
        public string ProyectoConMasHoras
        {
            get => _proyectoConMasHoras;
            set { _proyectoConMasHoras = value; OnPropertyChanged(); }
        }

        private string _tareaConMasHoras = "-";
        public string TareaConMasHoras
        {
            get => _tareaConMasHoras;
            set { _tareaConMasHoras = value; OnPropertyChanged(); }
        }

        // Esto es para cuando pinchas en un proyecto
        private ResumenProyecto? _proyectoSeleccionadoParaTareas;
        public ResumenProyecto? ProyectoSeleccionadoParaTareas
        {
            get => _proyectoSeleccionadoParaTareas;
            set
            {
                _proyectoSeleccionadoParaTareas = value;
                OnPropertyChanged();
                if (value != null)
                {
                    ActualizarTareasPorProyecto(value.Proyecto);
                }
            }
        }

        // Para que las barras del grafico no se salgan
        private double _maxHorasProyecto;
        public double MaxHorasProyecto
        {
            get => _maxHorasProyecto;
            set { _maxHorasProyecto = value; OnPropertyChanged(); }
        }

        private double _maxHorasTarea;
        public double MaxHorasTarea
        {
            get => _maxHorasTarea;
            set { _maxHorasTarea = value; OnPropertyChanged(); }
        }

        // Acciones de los botones
        public RelayCommand AplicarFiltrosCommand { get; }
        public RelayCommand ExportarPdfCommand { get; }

        public MainViewModel()
        {
            _dataService = new DataService();
            _pdfService = new PdfService();

            AplicarFiltrosCommand = new RelayCommand(_ => AplicarFiltros());
            ExportarPdfCommand = new RelayCommand(_ => ExportarPdf());

            InicializarDatos();
        }

        private void InicializarDatos()
        {
            var proyectos = _dataService.ObtenerProyectos();
            Proyectos.Clear();
            foreach (var p in proyectos) Proyectos.Add(p);
            
            ProyectoSeleccionado = "Todos";
            FechaHasta = DateTime.Now;
            FechaDesde = DateTime.Now.AddMonths(-1);

            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            var registros = _dataService.FiltrarRegistros(ProyectoSeleccionado, FechaDesde, FechaHasta);

            // Pongo los datos en la tabla
            RegistrosTiempo.Clear();
            foreach (var r in registros.OrderByDescending(x => x.Fecha))
                RegistrosTiempo.Add(r);

            // Hago las cuentas para los resumenes
            var resumenes = _dataService.CalcularResumenesPorProyecto(registros);
            ResumenesProyecto.Clear();
            foreach (var r in resumenes)
                ResumenesProyecto.Add(r);

            // Calculo los totales y maximos
            TotalHoras = registros.Sum(r => r.Horas);
            ProyectoConMasHoras = resumenes.FirstOrDefault()?.Proyecto ?? "-";
            
            var tareaTop = registros
                .GroupBy(t => t.Tarea)
                .Select(g => new { Tarea = g.Key, Horas = g.Sum(x => x.Horas) })
                .OrderByDescending(x => x.Horas)
                .FirstOrDefault();
            TareaConMasHoras = tareaTop?.Tarea ?? "-";

            // Preparo los datos para el grafico de tartas o barras
            if (resumenes.Any())
                MaxHorasProyecto = resumenes.Max(r => r.TotalHoras);

            DatosGraficoProyectos.Clear();
            foreach (var item in resumenes.Take(10))
                DatosGraficoProyectos.Add(item);

            // Borro lo de abajo si cambio el filtro
            ProyectoSeleccionadoParaTareas = null;
            ResumenesTarea.Clear();
            DatosGraficoTareas.Clear();
        }

        private void ActualizarTareasPorProyecto(string proyecto)
        {
            // Vuelvo a filtrar por si acaso
            var registrosGlobales = _dataService.FiltrarRegistros(ProyectoSeleccionado, FechaDesde, FechaHasta);
            var tareas = _dataService.CalcularResumenesPorTarea(registrosGlobales, proyecto);

            if (tareas.Any())
                MaxHorasTarea = tareas.Max(t => t.TotalHoras);

            ResumenesTarea.Clear();
            DatosGraficoTareas.Clear();

            foreach (var t in tareas)
                ResumenesTarea.Add(t);
            
            foreach (var t in tareas.Take(10))
                DatosGraficoTareas.Add(t);
        }

        private void ExportarPdf()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Informe_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var registros = _dataService.FiltrarRegistros(ProyectoSeleccionado, FechaDesde, FechaHasta);
                    var resumenes = _dataService.CalcularResumenesPorProyecto(registros);

                    _pdfService.GenerarInformePdf(
                        saveFileDialog.FileName,
                        registros,
                        resumenes,
                        FechaDesde,
                        FechaHasta,
                        TotalHoras,
                        ProyectoConMasHoras,
                        TareaConMasHoras
                    );

                    MessageBox.Show("PDF generado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
