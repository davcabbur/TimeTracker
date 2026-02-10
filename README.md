Una aplicación de escritorio desarrollada en C# diseñada para el seguimiento preciso del tiempo dedicado a tareas y proyectos. Este sistema implementa una arquitectura sólida que permite la creación, gestión y análisis de sesiones de trabajo, enfocándose en la persistencia de datos y la generación de reportes.

Características Clave:

⏱️ Cronómetro de Alta Precisión: Lógica de Stopwatch implementada con manejo de estados (Pausa/Reanudar/Detener).

💾 Persistencia de Datos: Uso de Entity Framework Core (o SQLite/SQL Server) para almacenar históricos de sesiones.

📊 Dashboard Interactivo: Visualización de métricas de productividad (horas por día/semana) usando librerías de gráficos.

📂 Gestión Jerárquica: Organización por Clientes > Proyectos > Tareas.

📄 Exportación: Capacidad de generar reportes en formato Excel (.csv) o PDF para facturación.

Stack Tecnológico:

C# / .NET 7

Entity Framework / LINQ

Windows Forms / WPF (XAML)

Librería de Gráficos (ej: LiveCharts)
