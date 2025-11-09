import { Component, OnInit } from '@angular/core';

type EstadoSolicitud = 'Pendiente' | 'Revisada' | 'Aprobada';

interface Solicitud {
  id: string;
  firstName: string;
  lastName: string;
  age: number;
  email: string;
  phone: string;
  address: string;
  realEstateExperience: string;
  availability: string;
  cv: string | null;
  estado: EstadoSolicitud;
  fechaSolicitud: string;
}

@Component({
  selector: 'app-requestadvisors',
  standalone: false,
  templateUrl: './requestadvisors.html',
  styleUrls: ['./requestadvisors.css']
})
export class Requestadvisors implements OnInit {
  // Datos
  solicitudes: Solicitud[] = [];
  loading = true;

  // Filtros
  filtros = { estado: 'Todos' as 'Todos' | EstadoSolicitud };

  // Estado UI
  solicitudSeleccionada: Solicitud | null = null;
  modalAbierto = false;

  // Modal cambiar estado
  modalCambioAbierto = false;
  nuevoEstadoSeleccionado: EstadoSolicitud = 'Pendiente';
  cargandoCambio = false;

  // Export
  exportando = false;

  ngOnInit(): void {
    // Lazy loading simulado (fetch backend)
    setTimeout(() => {
      this.solicitudes = [
        {
          id: '1',
          firstName: 'María',
          lastName: 'Fernández',
          age: 28,
          email: 'maria.fernandez@example.com',
          phone: '987654321',
          address: 'Calle Principal 123, Lima',
          realEstateExperience: 'Medio',
          availability: 'Tiempo Completo',
          cv: 'maria_cv.pdf',
          estado: 'Pendiente',
          fechaSolicitud: '2025-11-01'
        },
        {
          id: '2',
          firstName: 'Carlos',
          lastName: 'López',
          age: 35,
          email: 'carlos.lopez@example.com',
          phone: '999888777',
          address: 'Avenida Central 456, Lima',
          realEstateExperience: 'Experto',
          availability: 'Tiempo Completo',
          cv: 'carlos_cv.pdf',
          estado: 'Revisada',
          fechaSolicitud: '2025-10-30'
        },
        {
          id: '3',
          firstName: 'Andrea',
          lastName: 'Ríos',
          age: 31,
          email: 'andrea.rios@example.com',
          phone: '912345678',
          address: 'Paseo del Bosque 789, Lima',
          realEstateExperience: 'Experto',
          availability: 'Horario Flexible',
          cv: 'andrea_cv.pdf',
          estado: 'Aprobada',
          fechaSolicitud: '2025-10-28'
        },
        {
          id: '4',
          firstName: 'Luis',
          lastName: 'Torres',
          age: 42,
          email: 'luis.torres@example.com',
          phone: '981234567',
          address: 'Calle del Comercio 321, Lima',
          realEstateExperience: 'Básico',
          availability: 'Medio Tiempo',
          cv: null,
          estado: 'Pendiente',
          fechaSolicitud: '2025-11-02'
        },
        {
          id: '5',
          firstName: 'Gabriela',
          lastName: 'Silva',
          age: 26,
          email: 'gabriela.silva@example.com',
          phone: '975321654',
          address: 'Avenida del Mar 654, Lima',
          realEstateExperience: 'Medio',
          availability: 'Tiempo Completo',
          cv: 'gabriela_cv.pdf',
          estado: 'Pendiente',
          fechaSolicitud: '2025-11-02'
        }
      ];
      this.loading = false;
    }, 800);
  }

  // Estadísticas
  get stats() {
    return {
      total: this.solicitudes.length,
      pendientes: this.solicitudes.filter(s => s.estado === 'Pendiente').length,
      revisadas: this.solicitudes.filter(s => s.estado === 'Revisada').length,
      aprobadas: this.solicitudes.filter(s => s.estado === 'Aprobada').length
    };
  }

  // Filtrado
  get solicitudesFiltradas(): Solicitud[] {
    return this.filtros.estado === 'Todos'
      ? this.solicitudes
      : this.solicitudes.filter(s => s.estado === this.filtros.estado);
  }

  onFiltroEstadoChange(v: string) {
    if (v === 'Todos' || v === 'Pendiente' || v === 'Revisada' || v === 'Aprobada') {
      this.filtros.estado = v;
    }
  }

  // Acciones fila
  verDetalles(s: Solicitud) {
    this.solicitudSeleccionada = s;
    this.nuevoEstadoSeleccionado = s.estado;
    this.modalAbierto = true;
  }

  // Descarga simulada
  handleDescargarCV(cvNombre: string | null) {
    if (!cvNombre) { alert('No hay CV disponible para descargar'); return; }
    console.log('Descargando CV:', cvNombre);
  }

  // Badge de estado
  getEstadoClase(estado: EstadoSolicitud) {
    switch (estado) {
      case 'Pendiente': return 'badge pendiente';
      case 'Revisada': return 'badge aprobado';
      case 'Aprobada': return 'badge aprobado';
    }
  }

  // Modal detalle
  cerrarModal() { this.modalAbierto = false; this.solicitudSeleccionada = null; }

  // Cambiar estado
  abrirModalCambio() { this.modalCambioAbierto = true; }
  cerrarModalCambio() { this.modalCambioAbierto = false; }

  async confirmarCambioEstado() {
    if (!this.solicitudSeleccionada) return;
    if (this.nuevoEstadoSeleccionado === this.solicitudSeleccionada.estado) { this.cerrarModalCambio(); return; }
    this.cargandoCambio = true;
    try {
      await new Promise(r => setTimeout(r, 800));
      this.solicitudes = this.solicitudes.map(s =>
        s.id === this.solicitudSeleccionada!.id ? { ...s, estado: this.nuevoEstadoSeleccionado } : s
      );
      this.solicitudSeleccionada = { ...this.solicitudSeleccionada, estado: this.nuevoEstadoSeleccionado };
      this.cerrarModalCambio();
    } catch {
      alert('Error al cambiar estado');
    } finally {
      this.cargandoCambio = false;
    }
  }

  // Export
  async exportarAExcel(datos: Solicitud[]): Promise<void> {
    await new Promise(r => setTimeout(r, 600));
  }
  async handleExportar(): Promise<void> {
    if (this.loading) return;
    const datos = this.solicitudesFiltradas;
    if (!datos.length) { alert('No hay datos para exportar'); return; }
    this.exportando = true;
    try { await this.exportarAExcel(datos); }
    catch { alert('Error al exportar'); }
    finally { this.exportando = false; }
  }
}
