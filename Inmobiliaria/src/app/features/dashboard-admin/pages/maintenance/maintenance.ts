import { Component, OnInit } from '@angular/core';



type EstadoMant = 'EN REVISIÓN' | 'PENDIENTE' | 'PROGRAMADO' | 'COMPLETADO';
type TipoMant = 'Plomería' | 'Electricidad' | 'Correctivo' | 'Preventivo' | '';

interface Mantenimiento {
  id: number;
  propiedad: string;
  tipo: string;
  estado: EstadoMant;
  costo: number;
  fechaProgramada: string;
  responsable: string;
  descripcion?: string;
}

interface Filtros {
  nombre: string;
  estado: '' | EstadoMant;
  tipo: TipoMant | '';
  responsable: string;
}

@Component({
  selector: 'app-maintenance',
  standalone: false,
  templateUrl: './maintenance.html',
  styleUrls: ['./maintenance.css']
})
export class Maintenance implements OnInit {
  // Datos
  mantenimientos: Mantenimiento[] = [];
  loading = true;

  // Filtros
  filtros: Filtros = { nombre: '', estado: '', tipo: '', responsable: '' };

  // Form modal
  mostrarForm = false;
  editando: Mantenimiento | null = null;

  // Catálogos
  propiedadesDisponibles = [
    'Casa Moderna en San Isidro',
    'Departamento en Miraflores',
    'Terreno Comercial Ate',
    'Oficina San Borja',
    'Casa Familiar en Surco'
  ];
  responsables = ['Juan Pérez', 'Luis Pedro', 'Carla Gómez', 'María Torres', 'Sin asignar'];

  // Form state
  form: {
    propiedad: string;
    tipo: string;
    estado: EstadoMant;
    costo: string;
    fechaProgramada: string;
    responsable: string;
    descripcion: string;
  } = {
      propiedad: '',
      tipo: '',
      estado: 'EN REVISIÓN',
      costo: '',
      fechaProgramada: '',
      responsable: '',
      descripcion: ''
    };

  ngOnInit(): void {
    // Lazy loading simulado
    setTimeout(() => {
      this.mantenimientos = [
        {
          id: 1,
          propiedad: 'Casa Moderna en San Isidro',
          tipo: 'Plomería',
          estado: 'PROGRAMADO',
          costo: 1350,
          fechaProgramada: '2025-07-15',
          responsable: 'Juan Pérez'
        },
        {
          id: 2,
          propiedad: 'Terreno Comercial Ate',
          tipo: 'Correctivo',
          estado: 'COMPLETADO',
          costo: 1500,
          fechaProgramada: '2025-07-18',
          responsable: 'Luis Pedro'
        },
        {
          id: 3,
          propiedad: 'Departamento Miraflores',
          tipo: 'Electricidad',
          estado: 'PENDIENTE',
          costo: 900,
          fechaProgramada: '2025-08-10',
          responsable: 'Carla Gómez'
        }
      ];
      this.loading = false;
    }, 800);
  }

  // Métricas
  get totalPendiente() { return this.mantenimientos.filter(m => m.estado === 'PENDIENTE').length; }
  get totalProgramado() { return this.mantenimientos.filter(m => m.estado === 'PROGRAMADO').length; }
  get totalCompletado() { return this.mantenimientos.filter(m => m.estado === 'COMPLETADO').length; }
  get totalGeneral() { return this.mantenimientos.length; }

  // Filtro compuesto
  get filtrados(): Mantenimiento[] {
    const q = this.filtros.nombre.trim().toLowerCase();
    return this.mantenimientos.filter(m => {
      const coincideNombre = !q || m.propiedad.toLowerCase().includes(q);
      const coincideEstado = this.filtros.estado ? m.estado === this.filtros.estado : true;
      const coincideTipo = this.filtros.tipo ? m.tipo === this.filtros.tipo : true;
      const coincideResp = this.filtros.responsable
        ? m.responsable.toLowerCase().includes(this.filtros.responsable.toLowerCase())
        : true;
      return coincideNombre && coincideEstado && coincideTipo && coincideResp;
    });
  }

  // Badge clases
  claseEstado(estado: EstadoMant) {
    return {
      'badge pendiente': estado === 'PENDIENTE' || estado === 'EN REVISIÓN',
      'badge programado': estado === 'PROGRAMADO',
      'badge completado': estado === 'COMPLETADO'
    };
  }

  // Acciones tabla
  handleEstado(id: number, nuevoEstado: EstadoMant) {
    this.mantenimientos = this.mantenimientos.map(m => m.id === id ? { ...m, estado: nuevoEstado } : m);
  }
  handleEditar(m: Mantenimiento) {
    this.editando = { ...m };
    this.form = {
      propiedad: m.propiedad,
      tipo: m.tipo,
      estado: m.estado,
      costo: String(m.costo ?? ''),
      fechaProgramada: m.fechaProgramada,
      responsable: m.responsable,
      descripcion: m.descripcion ?? ''
    };
    this.mostrarForm = true;
  }
  handleEliminar(id: number) {
    const ok = confirm('¿Seguro que deseas eliminar este mantenimiento?');
    if (!ok) return;
    this.mantenimientos = this.mantenimientos.filter(m => m.id !== id);
  }

  // Form modal
  abrirNuevo() {
    this.editando = null;
    this.form = {
      propiedad: '',
      tipo: '',
      estado: 'EN REVISIÓN',
      costo: '',
      fechaProgramada: '',
      responsable: '',
      descripcion: ''
    };
    this.mostrarForm = true;
  }
  cerrarForm() {
    this.mostrarForm = false;
    this.editando = null;
  }
  guardarForm() {
    if (!this.form.propiedad || !this.form.tipo || !this.form.fechaProgramada) {
      alert('Completa los campos obligatorios');
      return;
    }
    const payload: Mantenimiento = {
      id: this.editando ? this.editando.id : this.generarId(),
      propiedad: this.form.propiedad,
      tipo: this.form.tipo,
      estado: this.form.estado,
      costo: Number(this.form.costo || 0),
      fechaProgramada: this.form.fechaProgramada,
      responsable: this.form.responsable || 'Sin asignar',
      descripcion: this.form.descripcion
    };
    if (this.editando) {
      this.mantenimientos = this.mantenimientos.map(m => m.id === this.editando!.id ? { ...payload } : m);
    } else {
      this.mantenimientos = [payload, ...this.mantenimientos];
    }
    this.cerrarForm();
  }
  generarId() {
    return Math.max(0, ...this.mantenimientos.map(m => m.id)) + 1;
  }
}
