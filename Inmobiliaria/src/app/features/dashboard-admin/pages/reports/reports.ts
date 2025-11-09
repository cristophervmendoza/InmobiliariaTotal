import { Component } from '@angular/core';

type TipoReporte = 'empleados' | 'propiedades' | 'testimonios' | 'citas' | 'mantenimiento';

@Component({
  selector: 'app-reports',
  standalone: false,
  templateUrl: './reports.html',
  styleUrls: ['./reports.css']
})
export class Reports {
  tipoActivo: TipoReporte = 'empleados';
  exportando = false;

  readonly tipos: TipoReporte[] = ['empleados', 'propiedades', 'testimonios', 'citas', 'mantenimiento'];

  empleadosEjemplo = [
    { id: 1, nombre: 'Ana Torres', rol: 'Agente', email: 'ana@idealhome.pe', telefono: '999-111-222', propiedadesAsignadas: 8 },
    { id: 2, nombre: 'Luis Ramos', rol: 'Gerente', email: 'luis@idealhome.pe', telefono: '999-333-444', propiedadesAsignadas: 3 }
  ];
  propiedadesEjemplo = [
    { id: 101, titulo: 'Dpto. Miraflores', tipo: 'Departamento', estado: 'Disponible', precio: 420000, ubicacion: 'Miraflores', habitaciones: 3 },
    { id: 102, titulo: 'Casa La Molina', tipo: 'Casa', estado: 'Vendido', precio: 780000, ubicacion: 'La Molina', habitaciones: 4 }
  ];
  testimoniosEjemplo = [
    { id: 1, cliente: 'Carlos M.', contenido: 'Excelente servicio, muy recomendados.', valoracion: 5, fecha: '2025-10-01' },
    { id: 2, cliente: 'Rosa P.', contenido: 'Atención rápida y confiable.', valoracion: 4, fecha: '2025-10-12' }
  ];
  citasEjemplo = [
    { id: 1, cliente: 'María L.', agente: 'Ana Torres', fecha: '2025-11-10', hora: '15:00', estado: 'Pendiente' },
    { id: 2, cliente: 'Jorge K.', agente: 'Luis Ramos', fecha: '2025-11-12', hora: '11:00', estado: 'Confirmada' }
  ];
  mantenimientoEjemplo = [
    { id: 1, propiedad: 'Dpto. Miraflores', tipo: 'Plomería', descripcion: 'Cambio de grifería', costo: 250, estado: 'Completado', fechaProgramada: '2025-10-20' },
    { id: 2, propiedad: 'Casa La Molina', tipo: 'Pintura', descripcion: 'Fachada', costo: 1200, estado: 'Programado', fechaProgramada: '2025-11-20' }
  ];

  get estadisticas() {
    return [
      { titulo: 'Total Empleados', valor: this.empleadosEjemplo.length, icon: 'users', color: '#06b6d4' },
      { titulo: 'Total Propiedades', valor: this.propiedadesEjemplo.length, icon: 'building-2', color: '#8b5cf6' },
      { titulo: 'Total Testimonios', valor: this.testimoniosEjemplo.length, icon: 'message-square', color: '#ec4899' },
      { titulo: 'Total Citas', valor: this.citasEjemplo.length, icon: 'calendar', color: '#f59e0b' }
    ];
  }

  setTipoActivo(tipo: TipoReporte) { this.tipoActivo = tipo; }

  obtenerDatos(): any[] {
    switch (this.tipoActivo) {
      case 'empleados': return this.empleadosEjemplo;
      case 'propiedades': return this.propiedadesEjemplo;
      case 'testimonios': return this.testimoniosEjemplo;
      case 'citas': return this.citasEjemplo;
      case 'mantenimiento': return this.mantenimientoEjemplo;
      default: return [];
    }
  }

  obtenerColumnas(): string[] {
    switch (this.tipoActivo) {
      case 'empleados': return ['ID', 'Nombre', 'Rol', 'Email', 'Teléfono', 'Propiedades Asignadas'];
      case 'propiedades': return ['ID', 'Título', 'Tipo', 'Estado', 'Precio', 'Ubicación', 'Habitaciones'];
      case 'testimonios': return ['ID', 'Cliente', 'Contenido', 'Valoración', 'Fecha'];
      case 'citas': return ['ID', 'Cliente', 'Agente', 'Fecha', 'Hora', 'Estado'];
      case 'mantenimiento': return ['ID', 'Propiedad', 'Tipo', 'Descripción', 'Costo', 'Estado', 'Fecha Programada'];
      default: return [];
    }
  }

  // Genera 5 posiciones: 1 encendida, 0 apagada
  getEstrellas(valor: number): number[] {
    const v = Math.max(0, Math.min(5, Math.floor(Number(valor) || 0)));
    return Array.from({ length: 5 }, (_, i) => i < v ? 1 : 0);
  }

  renderizarCelda(dato: any, columna: string): string {
    const c = columna.toLowerCase();

    if (this.tipoActivo === 'empleados') {
      if (c === 'id') return String(dato.id);
      if (c === 'nombre') return dato.nombre;
      if (c === 'rol') return dato.rol;
      if (c === 'email') return dato.email;
      if (c === 'teléfono') return dato.telefono;
      if (c === 'propiedades asignadas') return String(dato.propiedadesAsignadas);
    }

    if (this.tipoActivo === 'propiedades') {
      if (c === 'id') return String(dato.id);
      if (c === 'título') return dato.titulo;
      if (c === 'tipo') return dato.tipo;
      if (c === 'estado') return dato.estado;
      if (c === 'precio') return `S/ ${Number(dato.precio).toLocaleString()}`;
      if (c === 'ubicación') return dato.ubicacion;
      if (c === 'habitaciones') return String(dato.habitaciones);
    }

    if (this.tipoActivo === 'testimonios') {
      if (c === 'id') return String(dato.id);
      if (c === 'cliente') return dato.cliente;
      if (c === 'contenido') return dato.contenido;
      // 'valoración' se renderiza en el template con íconos
      if (c === 'fecha') return dato.fecha;
    }

    if (this.tipoActivo === 'citas') {
      if (c === 'id') return String(dato.id);
      if (c === 'cliente') return dato.cliente;
      if (c === 'agente') return dato.agente;
      if (c === 'fecha') return dato.fecha;
      if (c === 'hora') return dato.hora;
      if (c === 'estado') return dato.estado;
    }

    if (this.tipoActivo === 'mantenimiento') {
      if (c === 'id') return String(dato.id);
      if (c === 'propiedad') return dato.propiedad;
      if (c === 'tipo') return dato.tipo;
      if (c === 'descripción') return dato.descripcion;
      if (c === 'costo') return `S/ ${Number(dato.costo).toLocaleString()}`;
      if (c === 'estado') return dato.estado;
      if (c === 'fecha programada') return dato.fechaProgramada;
    }

    return '-';
  }

  async exportarAExcel(datos: any[], tipo: TipoReporte): Promise<void> {
    await new Promise(r => setTimeout(r, 500));
  }

  async handleExportar(): Promise<void> {
    const datos = this.obtenerDatos();
    if (!datos.length) { alert('No hay datos para exportar'); return; }
    this.exportando = true;
    try { await this.exportarAExcel(datos, this.tipoActivo); }
    catch (e) { console.error('Error al exportar:', e); alert('Error al exportar el archivo'); }
    finally { setTimeout(() => this.exportando = false, 500); }
  }
}
