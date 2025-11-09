import { Component, Input } from '@angular/core';
type DatosDashboard = {
  propiedadesActivas?: number;
  ventasMes?: number;
  ingresosTotales?: number;
  clientesActivos?: number;
};

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {
  @Input() datos: DatosDashboard | null = null;
  @Input() loading = false;

  // Config estática para listas (íconos por nombre, usados en el template)
  readonly graficos = [
    { icon: 'bar-chart-3', titulo: 'Ventas Mensuales', subtitulo: 'Últimos 6 meses' },
    { icon: 'pie-chart', titulo: 'Distribución de Propiedades', subtitulo: 'Por tipo de inmueble' }
  ];

  readonly tablas = [
    { icon: 'building-2', titulo: 'Propiedades Recientes', subtitulo: 'Últimas propiedades agregadas', mensaje: 'No hay propiedades recientes' },
    { icon: 'calendar', titulo: 'Citas Próximas', subtitulo: 'Agenda de esta semana', mensaje: 'No hay citas programadas' }
  ];

  get stats() {
    return [
      { titulo: 'Propiedades Activas', valor: this.datos?.propiedadesActivas ?? 0, icon: 'home' },
      { titulo: 'Ventas del Mes', valor: this.datos?.ventasMes ?? 0, icon: 'trending-up' },
      { titulo: 'Ingresos Totales', valor: this.datos?.ingresosTotales ?? 0, icon: 'dollar-sign' },
      { titulo: 'Clientes Activos', valor: this.datos?.clientesActivos ?? 0, icon: 'users' }
    ];
  }
}
