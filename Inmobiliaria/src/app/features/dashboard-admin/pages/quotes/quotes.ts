import { Component, OnInit } from '@angular/core';

type EstadoAgenda = 'Confirmada' | 'Pendiente' | 'Cancelada';
type TipoEvento = 'Cita' | 'Reunión' | 'Seguimiento' | 'Evento';

interface AgendaItem {
  idAgenda: number;
  titulo: string;
  tipo: TipoEvento;
  telefono: string;
  fechaHora: string; // ISO
  descripcionEvento: string;
  estado: EstadoAgenda;
  ubicacion: string;
  idTipoPrioridad: 1 | 2 | 3; // 1 Alta, 2 Media, 3 Baja
}


@Component({
  selector: 'app-quotes',
  standalone: false,
  templateUrl: './quotes.html',
  styleUrl: './quotes.css',
})
export class Quotes implements OnInit {
  agenda: AgendaItem[] = [];
  loading = true;

  // UI
  showModal = false;
  agendaEditar: AgendaItem | null = null;
  filtroEstado: 'todos' | EstadoAgenda = 'todos';

  // Form
  form: {
    titulo: string;
    tipo: TipoEvento;
    telefono: string;
    fechaHora: string;
    descripcionEvento: string;
    estado: EstadoAgenda;
    ubicacion: string;
    idTipoPrioridad: 1 | 2 | 3;
  } = {
      titulo: '',
      tipo: 'Cita',
      telefono: '',
      fechaHora: '',
      descripcionEvento: '',
      estado: 'Pendiente',
      ubicacion: '',
      idTipoPrioridad: 2
    };

  ngOnInit(): void {
    this.cargarAgenda();
  }

  async cargarAgenda() {
    try {
      // Mock
      const mock: AgendaItem[] = [
        {
          idAgenda: 1,
          titulo: 'Cita con cliente',
          tipo: 'Cita',
          telefono: '987654321',
          fechaHora: '2025-11-04T10:30:00',
          descripcionEvento: 'Visita a propiedad en Barranco',
          estado: 'Confirmada',
          ubicacion: 'Barranco, Lima',
          idTipoPrioridad: 1
        },
        {
          idAgenda: 2,
          titulo: 'Reunión con equipo',
          tipo: 'Reunión',
          telefono: '912345678',
          fechaHora: '2025-11-05T14:00:00',
          descripcionEvento: 'Planificación de proyectos',
          estado: 'Pendiente',
          ubicacion: 'Oficina Lima',
          idTipoPrioridad: 2
        },
        {
          idAgenda: 3,
          titulo: 'Seguimiento',
          tipo: 'Seguimiento',
          telefono: '945678901',
          fechaHora: '2025-11-10T15:00:00',
          descripcionEvento: 'Seguimiento a cliente anterior',
          estado: 'Confirmada',
          ubicacion: 'Lince, Lima',
          idTipoPrioridad: 3
        },
        {
          idAgenda: 4,
          titulo: 'Evento inmobiliario',
          tipo: 'Evento',
          telefono: '987123456',
          fechaHora: '2025-11-12T18:00:00',
          descripcionEvento: 'Lanzamiento de nuevo proyecto',
          estado: 'Confirmada',
          ubicacion: 'Centro, Lima',
          idTipoPrioridad: 1
        }
      ];
      // Simular delay
      await new Promise(r => setTimeout(r, 500));
      this.agenda = mock;
    } finally {
      this.loading = false;
    }
  }

  // Filtro
  get agendaFiltrada(): AgendaItem[] {
    return this.filtroEstado === 'todos'
      ? this.agenda
      : this.agenda.filter(a => a.estado === this.filtroEstado);
  }

  // UI helpers
  prioridadClase(id: 1 | 2 | 3) {
    return {
      prioridad1: id === 1,
      prioridad2: id === 2,
      prioridad3: id === 3
    };
  }
  estadoClase(estado: EstadoAgenda) {
    return `estado ${'estado' + estado.replace(/\s/g, '')}`;
  }

  // Modal
  abrirModal(evento: AgendaItem | null = null) {
    if (evento) {
      this.form = {
        titulo: evento.titulo,
        tipo: evento.tipo,
        telefono: evento.telefono,
        fechaHora: evento.fechaHora,
        descripcionEvento: evento.descripcionEvento,
        estado: evento.estado,
        ubicacion: evento.ubicacion,
        idTipoPrioridad: evento.idTipoPrioridad
      };
      this.agendaEditar = evento;
    } else {
      this.form = {
        titulo: '',
        tipo: 'Cita',
        telefono: '',
        fechaHora: '',
        descripcionEvento: '',
        estado: 'Pendiente',
        ubicacion: '',
        idTipoPrioridad: 2
      };
      this.agendaEditar = null;
    }
    this.showModal = true;
  }
  cerrarModal() { this.showModal = false; this.agendaEditar = null; }

  // CRUD
  handleGuardar() {
    if (!this.form.titulo || !this.form.fechaHora) {
      alert('Completa los campos obligatorios');
      return;
    }
    if (this.agendaEditar) {
      this.agenda = this.agenda.map(a =>
        a.idAgenda === this.agendaEditar!.idAgenda ? { ...this.agendaEditar!, ...this.form } as AgendaItem : a
      );
      alert('Evento actualizado');
    } else {
      const nuevo: AgendaItem = {
        idAgenda: Math.max(0, ...this.agenda.map(a => a.idAgenda)) + 1,
        ...this.form
      };
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





