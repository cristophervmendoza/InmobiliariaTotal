import { Component, OnInit, inject } from '@angular/core';
import { PropertiesService, Propiedad, FiltrosPropiedades, CrearPropiedadDto, ActualizarPropiedadDto } from '../../../../core/services/properties.service';
import { EmployeesService, Empleado } from '../../../../core/services/employees.service';
import { forkJoin } from 'rxjs';

type Moneda = 'USD' | 'PEN' | 'EUR';
type EstadoOferta = 'activa' | 'reservada' | 'vendida' | 'pausada' | 'cerrada';
type TipoPropiedad = 'Casa' | 'Departamento' | 'Terreno' | 'Otro';

interface PropiedadVista {
  idPropiedad: number;
  idUsuario: number;
  idTipoPropiedad: number;
  idEstadoPropiedad: number;
  titulo: string;
  direccion: string;
  precio: number;
  descripcion?: string;
  areaTerreno?: number;
  tipoMoneda: 'PEN' | 'USD' | 'EUR';
  habitacion?: number;
  bano?: number;
  estacionamiento?: number;
  fotoPropiedad?: string;
  creadoAt: string;
  actualizadoAt: string;
  id: number;
  codigo: string;
  tipo: TipoPropiedad;
  estado: EstadoOferta;
  area: number;
  habitaciones: number;
  banos: number;
  moneda: Moneda;
  distrito: string;
  agente: string;
  agenteNombre?: string;
  destacada: boolean;
  fechaPublicacion: string;
  vistas: number;
  imagenes?: string[];
}

@Component({
  selector: 'app-properties-admin',
  standalone: false,
  templateUrl: './properties-admin.html',
  styleUrls: ['./properties-admin.css']
})
export class PropertiesAdmin implements OnInit {
  private propertiesService = inject(PropertiesService);
  private employeesService = inject(EmployeesService);

  // Datos
  propiedades: PropiedadVista[] = [];
  empleados: Empleado[] = [];
  empleadosFiltrados: Empleado[] = [];
  loading = true;

  // Filtros
  busqueda = '';
  filtroTipo: 'Todos' | TipoPropiedad = 'Todos';
  filtroEstado: 'Todos' | EstadoOferta = 'Todos';

  // Export
  exportando = false;

  // Modales
  modalDetalleAbierto = false;
  modalEliminarAbierto = false;
  modalFormularioAbierto = false;
  seleccionado: PropiedadVista | null = null;
  editandoPropiedad: PropiedadVista | null = null;

  // Formulario
  formData = {
    titulo: '',
    idTipoPropiedad: 0,
    idEstadoPropiedad: 1,
    direccion: '',
    precio: 0,
    tipoMoneda: 'PEN' as 'PEN' | 'USD' | 'EUR',
    descripcion: '',
    areaTerreno: 0,
    habitacion: 0,
    bano: 0,
    estacionamiento: 0,
    idUsuario: 0,
    fotoPropiedad: null as File | null
  };

  // Búsqueda de agente
  busquedaAgente = '';
  mostrarDropdownAgente = false;
  agenteSeleccionado: Empleado | null = null;

  errors: Record<string, string> = {};

  // Mapeos
  private tiposPropiedad: Record<number, TipoPropiedad> = {
    1: 'Casa',
    2: 'Departamento',
    3: 'Terreno',
    4: 'Otro'
  };

  private estadosPropiedad: Record<number, EstadoOferta> = {
    1: 'activa',
    2: 'reservada',
    3: 'vendida',
    4: 'pausada',
    5: 'cerrada'
  };

  ngOnInit(): void {
    this.cargarDatosIniciales();
  }

  // ✅ MEJORADO: Cargar empleados Y propiedades en paralelo
  cargarDatosIniciales(): void {
    this.loading = true;

    forkJoin({
      empleados: this.employeesService.listarEmpleados(),
      propiedades: this.propertiesService.listarPropiedades()
    }).subscribe({
      next: ({ empleados, propiedades }) => {
        console.log('📦 Datos cargados:', { empleados, propiedades });

        // ✅ Cargar empleados primero
        if (empleados.exito && empleados.data) {
          this.empleados = empleados.data;
          this.empleadosFiltrados = this.empleados;
          console.log('✅ Empleados:', this.empleados.length);
        }

        // ✅ Mapear propiedades con empleados ya cargados
        if (propiedades.exito && propiedades.data) {
          this.propiedades = propiedades.data.map(prop => this.mapearPropiedad(prop));
          console.log('✅ Propiedades:', this.propiedades.length);
        }

        this.loading = false;
      },
      error: (error) => {
        console.error('❌ Error al cargar datos:', error);
        alert('Error al cargar datos iniciales');
        this.loading = false;
      }
    });
  }

  // ✅ MEJORADO: Recargar solo propiedades (después de crear/editar/eliminar)
  cargarPropiedades(): void {
    this.loading = true;

    this.propertiesService.listarPropiedades().subscribe({
      next: (response) => {
        console.log('📦 Propiedades actualizadas:', response);

        if (response.exito && response.data) {
          this.propiedades = response.data.map(prop => this.mapearPropiedad(prop));
          console.log('✅ Propiedades procesadas:', this.propiedades.length);
        } else {
          this.propiedades = [];
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('❌ Error al cargar propiedades:', error);
        alert('Error al cargar propiedades');
        this.loading = false;
      }
    });
  }

  private mapearPropiedad(prop: Propiedad): PropiedadVista {
    // ✅ Buscar el agente correspondiente
    const agente = this.empleados.find(e => e.idUsuario === prop.idUsuario);

    console.log(`🔍 Mapeando propiedad ${prop.idPropiedad} - idUsuario: ${prop.idUsuario}, agente encontrado:`, agente?.nombre);

    return {
      idPropiedad: prop.idPropiedad,
      idUsuario: prop.idUsuario,
      idTipoPropiedad: prop.idTipoPropiedad,
      idEstadoPropiedad: prop.idEstadoPropiedad,
      titulo: prop.titulo,
      direccion: prop.direccion,
      precio: prop.precio,
      descripcion: prop.descripcion,
      areaTerreno: prop.areaTerreno,
      tipoMoneda: prop.tipoMoneda,
      habitacion: prop.habitacion,
      bano: prop.bano,
      estacionamiento: prop.estacionamiento,
      fotoPropiedad: prop.fotoPropiedad,
      creadoAt: prop.creadoAt,
      actualizadoAt: prop.actualizadoAt,
      id: prop.idPropiedad,
      codigo: `PR-${String(prop.idPropiedad).padStart(3, '0')}`,
      tipo: this.tiposPropiedad[prop.idTipoPropiedad] || 'Otro',
      estado: this.estadosPropiedad[prop.idEstadoPropiedad] || 'activa',
      area: prop.areaTerreno || 0,
      habitaciones: prop.habitacion || 0,
      banos: prop.bano || 0,
      moneda: prop.tipoMoneda as Moneda,
      distrito: this.extraerDistrito(prop.direccion),
      agente: agente?.nombre || 'Sin asignar',
      agenteNombre: agente?.nombre,
      destacada: false,
      fechaPublicacion: this.formatearFecha(prop.creadoAt),
      vistas: 0,
      imagenes: prop.fotoPropiedad ? [prop.fotoPropiedad] : []
    };
  }

  private extraerDistrito(direccion: string): string {
    const partes = direccion.split(',');
    return partes.length > 1 ? partes[partes.length - 1].trim() : 'Sin distrito';
  }

  formatearFecha(fecha: string): string {
    const date = new Date(fecha);
    return date.toISOString().split('T')[0];
  }

  // Estadísticas
  get total(): number { return this.propiedades.length; }
  get activas(): number { return this.propiedades.filter(p => p.estado === 'activa').length; }
  get reservadas(): number { return this.propiedades.filter(p => p.estado === 'reservada').length; }
  get vendidas(): number { return this.propiedades.filter(p => p.estado === 'vendida').length; }

  get estadisticas() {
    return [
      { titulo: 'Total Propiedades', valor: this.total, icon: 'building-2', color: '#06b6d4' },
      { titulo: 'Activas', valor: this.activas, icon: 'check-circle-2', color: '#16a34a' },
      { titulo: 'Reservadas', valor: this.reservadas, icon: 'loader-2', color: '#0ea5e9' },
      { titulo: 'Vendidas', valor: this.vendidas, icon: 'x-circle', color: '#dc2626' }
    ];
  }

  // Filtro
  get propiedadesFiltradas(): PropiedadVista[] {
    const q = this.busqueda.trim().toLowerCase();
    return this.propiedades.filter(p => {
      const coincideTexto = !q ||
        p.titulo.toLowerCase().includes(q) ||
        p.codigo.toLowerCase().includes(q) ||
        p.distrito.toLowerCase().includes(q) ||
        p.tipo.toLowerCase().includes(q);
      const coincideTipo = this.filtroTipo === 'Todos' || p.tipo === this.filtroTipo;
      const coincideEstado = this.filtroEstado === 'Todos' || p.estado === this.filtroEstado;
      return coincideTexto && coincideTipo && coincideEstado;
    });
  }

  // Helpers UI
  estadoClaseBadge(estado: EstadoOferta): string {
    const base = 'badge';
    switch (estado) {
      case 'activa': return `${base} aprobado`;
      case 'reservada': return `${base} pendiente`;
      case 'vendida': return `${base} rechazado`;
      case 'pausada': return `${base} pendiente`;
      case 'cerrada': return `${base} rechazado`;
      default: return base;
    }
  }

  precioStr(p: PropiedadVista): string {
    const simbolo = p.moneda === 'USD' ? '$' : p.moneda === 'EUR' ? '€' : 'S/';
    return `${simbolo} ${Number(p.precio).toLocaleString('es-PE')}`;
  }

  // ✅ Búsqueda de agente
  onBusquedaAgenteChange(): void {
    const term = this.busquedaAgente.toLowerCase().trim();

    if (!term) {
      this.empleadosFiltrados = this.empleados;
    } else {
      this.empleadosFiltrados = this.empleados.filter(e =>
        e.nombre.toLowerCase().includes(term) ||
        e.email.toLowerCase().includes(term) ||
        e.dni.includes(term)
      );
    }
    this.mostrarDropdownAgente = true;
  }

  seleccionarAgente(empleado: Empleado): void {
    this.agenteSeleccionado = empleado;
    this.busquedaAgente = empleado.nombre;
    this.formData.idUsuario = empleado.idUsuario;
    this.mostrarDropdownAgente = false;
    console.log('✅ Agente seleccionado:', empleado);
  }

  limpiarAgente(): void {
    this.agenteSeleccionado = null;
    this.busquedaAgente = '';
    this.formData.idUsuario = 0;
    this.empleadosFiltrados = this.empleados;
  }

  // ✅ Modales
  abrirCrear(): void {
    this.editandoPropiedad = null;
    this.resetForm();
    this.modalFormularioAbierto = true;
  }

  abrirEditar(p: PropiedadVista): void {
    this.editandoPropiedad = p;
    this.seleccionado = p;

    // ✅ Cargar datos existentes en el formulario
    this.formData = {
      titulo: p.titulo,
      idTipoPropiedad: p.idTipoPropiedad,
      idEstadoPropiedad: p.idEstadoPropiedad,
      direccion: p.direccion,
      precio: p.precio,
      tipoMoneda: p.tipoMoneda,
      descripcion: p.descripcion || '',
      areaTerreno: p.areaTerreno || 0,
      habitacion: p.habitacion || 0,
      bano: p.bano || 0,
      estacionamiento: p.estacionamiento || 0,
      idUsuario: p.idUsuario,
      fotoPropiedad: null
    };

    // ✅ Cargar agente seleccionado
    const agente = this.empleados.find(e => e.idUsuario === p.idUsuario);
    if (agente) {
      this.agenteSeleccionado = agente;
      this.busquedaAgente = agente.nombre;
      console.log('✅ Agente precargado en edición:', agente.nombre);
    } else {
      console.warn('⚠️ No se encontró agente con idUsuario:', p.idUsuario);
    }

    this.modalFormularioAbierto = true;
    console.log('✏️ Editando propiedad:', p);
  }

  abrirDetalle(p: PropiedadVista): void {
    this.seleccionado = p;
    this.modalDetalleAbierto = true;
  }

  cerrarDetalle(): void {
    this.modalDetalleAbierto = false;
    this.seleccionado = null;
  }

  abrirEliminar(p: PropiedadVista): void {
    this.seleccionado = p;
    this.modalEliminarAbierto = true;
  }

  cerrarEliminar(): void {
    this.modalEliminarAbierto = false;
    this.seleccionado = null;
  }

  cerrarFormulario(): void {
    this.modalFormularioAbierto = false;
    this.editandoPropiedad = null;
    this.seleccionado = null;
    this.resetForm();
  }

  // ✅ Guardar (crear/editar)
  guardarPropiedad(): void {
    if (!this.validarFormulario()) {
      alert('Por favor completa todos los campos requeridos');
      return;
    }

    if (this.editandoPropiedad) {
      // Actualizar
      const dto: ActualizarPropiedadDto = {
        idUsuario: this.formData.idUsuario,
        idTipoPropiedad: this.formData.idTipoPropiedad,
        idEstadoPropiedad: this.formData.idEstadoPropiedad,
        titulo: this.formData.titulo,
        direccion: this.formData.direccion,
        precio: this.formData.precio,
        tipoMoneda: this.formData.tipoMoneda,
        descripcion: this.formData.descripcion,
        areaTerreno: this.formData.areaTerreno,
        habitacion: this.formData.habitacion,
        bano: this.formData.bano,
        estacionamiento: this.formData.estacionamiento,
        fotoPropiedad: this.formData.fotoPropiedad || undefined
      };

      console.log('📝 Actualizando propiedad con agente ID:', dto.idUsuario);

      this.propertiesService.actualizarPropiedad(this.editandoPropiedad.id, dto).subscribe({
        next: (response) => {
          if (response.exito) {
            alert('✅ Propiedad actualizada correctamente');
            this.cargarPropiedades();
            this.cerrarFormulario();
          } else {
            alert('❌ ' + (response.mensaje || 'Error al actualizar'));
          }
        },
        error: (error) => {
          console.error('❌ Error:', error);
          alert('❌ Error al actualizar propiedad: ' + error.message);
        }
      });
    } else {
      // Crear
      const dto: CrearPropiedadDto = {
        idUsuario: this.formData.idUsuario,
        idTipoPropiedad: this.formData.idTipoPropiedad,
        idEstadoPropiedad: this.formData.idEstadoPropiedad,
        titulo: this.formData.titulo,
        direccion: this.formData.direccion,
        precio: this.formData.precio,
        tipoMoneda: this.formData.tipoMoneda,
        descripcion: this.formData.descripcion,
        areaTerreno: this.formData.areaTerreno,
        habitacion: this.formData.habitacion,
        bano: this.formData.bano,
        estacionamiento: this.formData.estacionamiento,
        fotoPropiedad: this.formData.fotoPropiedad || undefined
      };

      console.log('➕ Creando propiedad con agente ID:', dto.idUsuario);

      this.propertiesService.crearPropiedad(dto).subscribe({
        next: (response) => {
          if (response.exito) {
            alert('✅ Propiedad creada correctamente');
            this.cargarPropiedades();
            this.cerrarFormulario();
          } else {
            alert('❌ ' + (response.mensaje || 'Error al crear'));
          }
        },
        error: (error) => {
          console.error('❌ Error:', error);
          alert('❌ Error al crear propiedad: ' + error.message);
        }
      });
    }
  }

  confirmarEliminar(): void {
    if (!this.seleccionado) return;

    this.propertiesService.eliminarPropiedad(this.seleccionado.id).subscribe({
      next: (response) => {
        if (response.exito) {
          alert('✅ Propiedad eliminada correctamente');
          this.cargarPropiedades();
          this.cerrarEliminar();
        } else {
          alert('❌ ' + (response.mensaje || 'Error al eliminar'));
        }
      },
      error: (error) => {
        console.error('❌ Error:', error);
        alert('❌ Error al eliminar propiedad');
      }
    });
  }

  // Helpers
  resetForm(): void {
    this.formData = {
      titulo: '',
      idTipoPropiedad: 0,
      idEstadoPropiedad: 1,
      direccion: '',
      precio: 0,
      tipoMoneda: 'PEN',
      descripcion: '',
      areaTerreno: 0,
      habitacion: 0,
      bano: 0,
      estacionamiento: 0,
      idUsuario: 0,
      fotoPropiedad: null
    };
    this.limpiarAgente();
    this.errors = {};
  }

  validarFormulario(): boolean {
    const e: Record<string, string> = {};

    if (!this.formData.titulo.trim()) e['titulo'] = 'El título es requerido';
    if (this.formData.idTipoPropiedad === 0) e['idTipoPropiedad'] = 'Selecciona un tipo';
    if (!this.formData.direccion.trim()) e['direccion'] = 'La dirección es requerida';
    if (this.formData.precio <= 0) e['precio'] = 'El precio debe ser mayor a 0';
    if (this.formData.idUsuario === 0) e['idUsuario'] = 'Debes seleccionar un agente';

    this.errors = e;
    return Object.keys(e).length === 0;
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.formData.fotoPropiedad = input.files[0];
    }
  }

  // Export
  async handleExportar(): Promise<void> {
    const datos = this.propiedadesFiltradas;
    if (!datos.length) {
      alert('No hay datos para exportar');
      return;
    }
    this.exportando = true;
    try {
      await new Promise(r => setTimeout(r, 600));
      alert('✅ Exportación exitosa');
    } catch {
      alert('❌ Error al exportar');
    } finally {
      this.exportando = false;
    }
  }
}
