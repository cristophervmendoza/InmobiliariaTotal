import { Component, OnInit } from '@angular/core';
type EstadoAgenda = 'Confirmada' | 'Pendiente' | 'Cancelada';
type TipoEvento = 'Cita' | 'Reunión' | 'Seguimiento' | 'Evento';

interface Propiedad {
  id: number;
  nombre: string;
}

interface AgendaItem {
  idAgenda: number;
  titulo: string;
  tipo: TipoEvento;
  telefono: string;
  fechaHora: string;
  descripcionEvento: string;
  estado: EstadoAgenda;
  ubicacion: string;
  idTipoPrioridad: 1 | 2 | 3;
  propiedadId?: number; //JALA DE SUS PROPIEDADES ASIGNADAS
}

@Component({
  selector: 'app-agenda-agent',
  standalone: false,
  templateUrl: './agenda-agent.html',
  styleUrls: ['./agenda-agent.css'],
})
export class AgendaAgent implements OnInit {
  propiedades: Propiedad[] = [];
  agenda: AgendaItem[] = [];
  loading = true;
  showModal = false;
  agendaEditar: AgendaItem | null = null;
  filtroEstado: 'todos' | EstadoAgenda = 'todos';

  form: {
    titulo: string;
    tipo: TipoEvento;
    telefono: string;
    fechaHora: string;
    descripcionEvento: string;
    estado: EstadoAgenda;
    ubicacion: string;
    idTipoPrioridad: 1 | 2 | 3;
    propiedadId?: number;
  } = {
      titulo: '', tipo: 'Cita', telefono: '', fechaHora: '', descripcionEvento: '', estado: 'Pendiente', ubicacion: '', idTipoPrioridad: 2, propiedadId: undefined
    };

  ngOnInit(): void { this.cargarAgenda(); }

  async cargarAgenda() {
    // Propiedades simuladas
    this.propiedades = [
      { id: 1, nombre: 'Propiedad 1 - Miraflores' },
      { id: 2, nombre: 'Propiedad 2 - Barranco' }
    ];
    // Eventos simulados
    const mock: AgendaItem[] = [
      { idAgenda: 1, titulo: 'Cita con nuevo cliente', tipo: 'Cita', telefono: '999888777', fechaHora: '2025-11-10T10:30:00', descripcionEvento: 'Visita a inmueble asignado', estado: 'Confirmada', ubicacion: 'Miraflores, Lima', idTipoPrioridad: 1, propiedadId: 1 },
      { idAgenda: 2, titulo: 'Reunión de equipo', tipo: 'Reunión', telefono: '912000123', fechaHora: '2025-11-11T14:00:00', descripcionEvento: 'Estrategia comercial', estado: 'Pendiente', ubicacion: 'Oficina Agent', idTipoPrioridad: 2, propiedadId: 2 }
    ];
    await new Promise(r => setTimeout(r, 500));
    this.agenda = mock;
    this.loading = false;
  }

  get agendaFiltrada(): AgendaItem[] {
    return this.filtroEstado === 'todos' ? this.agenda : this.agenda.filter(a => a.estado === this.filtroEstado);
  }

  prioridadClase(id: 1 | 2 | 3) {
    return { prioridad1: id === 1, prioridad2: id === 2, prioridad3: id === 3 };
  }
  estadoClase(estado: EstadoAgenda) { return `estado estado${estado}`; }

  abrirModal(evento: AgendaItem | null = null) {
    if (evento) {
      this.form = { ...evento };
      this.agendaEditar = evento;
    } else {
      this.form = { titulo: '', tipo: 'Cita', telefono: '', fechaHora: '', descripcionEvento: '', estado: 'Pendiente', ubicacion: '', idTipoPrioridad: 2, propiedadId: this.propiedades[0]?.id };
      this.agendaEditar = null;
    }
    this.showModal = true;
  }
  cerrarModal() { this.showModal = false; this.agendaEditar = null; }

  handleGuardar() {
    if (!this.form.titulo || !this.form.fechaHora) {
      alert('Completa los campos obligatorios');
      return;
    }
    if (this.agendaEditar) {
      this.agenda = this.agenda.map(a => a.idAgenda === this.agendaEditar!.idAgenda ? { ...this.agendaEditar!, ...this.form } as AgendaItem : a);
      alert('Evento actualizado');
    } else {
      const nuevo: AgendaItem = { idAgenda: Math.max(0, ...this.agenda.map(a => a.idAgenda)) + 1, ...this.form };
      this.agenda = [nuevo, ...this.agenda];
      alert('Evento creado');
    }
    this.cerrarModal();
  }

  handleEliminar(id: number) {
    if (!confirm('¿Eliminar este evento?')) return;
    this.agenda = this.agenda.filter(a => a.idAgenda !== id);
    alert('Evento eliminado');
  }
}
