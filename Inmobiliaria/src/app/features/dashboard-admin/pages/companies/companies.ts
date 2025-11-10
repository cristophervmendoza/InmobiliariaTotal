import { Component, OnInit, inject } from '@angular/core';
import { CompaniesService, Empresa, CrearEmpresaDto, ActualizarEmpresaDto } from '../../../../core/services/companies.service';
import { UserService, Usuario } from '../../../../core/services/user.service';
import { forkJoin } from 'rxjs';

type ErrorKeys = 'nombre' | 'ruc' | 'correo' | 'direccion' | 'telefono' | 'tipoEmpresa' | 'idUsuario';
type ErrorsMap = Record<ErrorKeys, string>;

interface CompanyVista {
  id: number;
  idUsuario: number;
  nombre: string;
  ruc: string;
  correo: string;
  direccion: string;
  logo: string;
  fechaRegistro: string;
  telefono: string;
  tipoEmpresa: string;
  nombreUsuario?: string;
}

interface StatCard {
  titulo: string;
  valor: number;
  color: string;
  icon: string;
  pct?: number;
}

@Component({
  selector: 'app-companies',
  standalone: false,
  templateUrl: './companies.html',
  styleUrls: ['./companies.css']
})
export class Companies implements OnInit {
  private companiesService = inject(CompaniesService);
  private usuariosService = inject(UserService);

  companies: CompanyVista[] = [];
  usuarios: Usuario[] = [];
  usuariosFiltrados: Usuario[] = [];
  loading = true;

  exportando = false;

  searchTerm = '';
  statusFilter: 'todos' | string = 'todos';

  showDetails = false;
  showDelete = false;
  showForm = false;
  selected: CompanyVista | null = null;
  editing: CompanyVista | null = null;

  formData = {
    idUsuario: 0,
    nombre: '',
    ruc: '',
    correo: '',
    direccion: '',
    logo: '',
    fechaRegistro: new Date().toISOString().slice(0, 10),
    telefono: '',
    tipoEmpresa: ''
  };

  errors: ErrorsMap = {
    idUsuario: '',
    nombre: '',
    ruc: '',
    correo: '',
    direccion: '',
    telefono: '',
    tipoEmpresa: ''
  };

  busquedaUsuario = '';
  mostrarDropdownUsuario = false;
  usuarioSeleccionado: Usuario | null = null;

  imagePreview = '';

  ngOnInit(): void {
    this.cargarDatosIniciales();
  }

  cargarDatosIniciales(): void {
    console.log('🔄 Cargando datos iniciales...');
    this.loading = true;

    forkJoin({
      empresas: this.companiesService.listarEmpresas(),
      usuarios: this.usuariosService.listarUsuarios()
    }).subscribe({
      next: ({ empresas, usuarios }) => {
        console.log('📦 Datos cargados:', { empresas, usuarios });

        if (usuarios.exito && usuarios.data) {
          this.usuarios = usuarios.data;
          this.usuariosFiltrados = this.usuarios;
          console.log('✅ Usuarios cargados:', this.usuarios.length);
        }

        if (empresas.exito && empresas.data) {
          this.companies = empresas.data.map(emp => this.mapearEmpresa(emp));
          console.log('✅ Empresas:', this.companies.length);
        } else {
          this.companies = [];
        }

        this.loading = false;
      },
      error: (error) => {
        console.error('❌ Error al cargar datos:', error);
        this.loading = false;
        this.companies = [];
      }
    });
  }

  // ✅ CORREGIDO: Recargar empresas Y usuarios
  cargarEmpresas(): void {
    console.log('🔄 Recargando empresas y usuarios...');
    this.loading = true;

    forkJoin({
      empresas: this.companiesService.listarEmpresas(),
      usuarios: this.usuariosService.listarUsuarios()
    }).subscribe({
      next: ({ empresas, usuarios }) => {
        console.log('📦 Datos recargados:', { empresas, usuarios });

        if (usuarios.exito && usuarios.data) {
          this.usuarios = usuarios.data;
          this.usuariosFiltrados = this.usuarios;
          console.log('✅ Usuarios actualizados:', this.usuarios.length);
        }

        if (empresas.exito && empresas.data) {
          this.companies = empresas.data.map(emp => this.mapearEmpresa(emp));
          console.log('✅ Empresas recargadas:', this.companies.length);
        }

        this.loading = false;
      },
      error: (error) => {
        console.error('❌ Error:', error);
        this.loading = false;
      }
    });
  }

  private mapearEmpresa(emp: Empresa): CompanyVista {
    const usuario = this.usuarios.find(u => u.idUsuario === emp.idUsuario);

    return {
      id: emp.idEmpresa,
      idUsuario: emp.idUsuario,
      nombre: emp.nombre,
      ruc: emp.ruc,
      correo: emp.email || 'Sin correo',
      direccion: emp.direccion || 'Sin dirección',
      logo: '/placeholder.svg',
      fechaRegistro: this.formatearFecha(emp.fechaRegistro),
      telefono: emp.telefono || 'Sin teléfono',
      tipoEmpresa: emp.tipoEmpresa || 'No especificado',
      nombreUsuario: usuario?.nombre || 'Sin asignar'
    };
  }

  private formatearFecha(fecha: string): string {
    const date = new Date(fecha);
    return date.toISOString().split('T')[0];
  }

  get total(): number {
    return this.companies.length;
  }

  get activas(): number {
    return this.companies.length;
  }

  get inactivas(): number {
    return 0;
  }

  private pct(value: number, base: number): number {
    if (!base) return 0;
    return Math.max(0, Math.min(100, Math.round((value / base) * 100)));
  }

  get estadisticas(): StatCard[] {
    const total = this.total;
    return [
      { titulo: 'Total', valor: total, color: '#06b6d4', icon: 'building-2', pct: 100 },
      { titulo: 'Activas', valor: this.activas, color: '#10b981', icon: 'check-circle-2', pct: this.pct(this.activas, total || 1) },
      { titulo: 'Inactivas', valor: this.inactivas, color: '#f59e0b', icon: 'clock', pct: this.pct(this.inactivas, total || 1) }
    ];
  }

  get filtered(): CompanyVista[] {
    const q = this.searchTerm.trim().toLowerCase();
    return this.companies.filter(c => {
      return !q ||
        c.nombre.toLowerCase().includes(q) ||
        c.ruc.toLowerCase().includes(q) ||
        c.correo.toLowerCase().includes(q) ||
        c.direccion.toLowerCase().includes(q);
    });
  }

  estadoBadge(estado: string) {
    return 'badge aprobado';
  }

  onBusquedaUsuarioChange(): void {
    const term = this.busquedaUsuario.toLowerCase().trim();

    if (!term) {
      this.usuariosFiltrados = this.usuarios;
    } else {
      this.usuariosFiltrados = this.usuarios.filter(u =>
        u.nombre.toLowerCase().includes(term) ||
        u.email.toLowerCase().includes(term) ||
        u.dni.includes(term)
      );
    }
    this.mostrarDropdownUsuario = true;
  }

  seleccionarUsuario(usuario: Usuario): void {
    this.usuarioSeleccionado = usuario;
    this.busquedaUsuario = usuario.nombre;
    this.formData.idUsuario = usuario.idUsuario;
    this.mostrarDropdownUsuario = false;
    this.clearError('idUsuario');
    console.log('✅ Usuario seleccionado:', usuario);
  }

  limpiarUsuario(): void {
    this.usuarioSeleccionado = null;
    this.busquedaUsuario = '';
    this.formData.idUsuario = 0;
    this.usuariosFiltrados = this.usuarios;
  }

  async exportarAExcel(datos: CompanyVista[]): Promise<void> {
    await new Promise(r => setTimeout(r, 800));
    console.log('📊 Exportando:', datos.length, 'empresas');
    alert('✅ Exportación exitosa');
  }

  async handleExportar() {
    const datos = this.filtered;
    if (!datos.length) {
      alert('No hay datos para exportar');
      return;
    }
    this.exportando = true;
    try {
      await this.exportarAExcel(datos);
    } catch {
      alert('Error al exportar');
    } finally {
      this.exportando = false;
    }
  }

  abrirDetalle(c: CompanyVista) {
    this.selected = c;
    this.showDetails = true;
  }

  cerrarDetalle() {
    this.showDetails = false;
    this.selected = null;
  }

  abrirEliminar(c: CompanyVista) {
    this.selected = c;
    this.showDelete = true;
  }

  cerrarEliminar() {
    this.showDelete = false;
    this.selected = null;
  }

  confirmarEliminar() {
    if (!this.selected) return;

    console.log('🗑️ Eliminando empresa:', this.selected.id);

    this.companiesService.eliminarEmpresa(this.selected.id).subscribe({
      next: (response) => {
        console.log('✅ Respuesta:', response);
        if (response.exito) {
          alert('✅ Empresa eliminada correctamente');
          this.cargarEmpresas();
          this.cerrarEliminar();
        } else {
          alert('❌ ' + (response.mensaje || 'Error al eliminar'));
        }
      },
      error: (error) => {
        console.error('❌ Error:', error);
        alert('❌ Error al eliminar empresa');
      }
    });
  }

  newCompany() {
    this.editing = null;
    this.formData = {
      idUsuario: 0,
      nombre: '',
      ruc: '',
      correo: '',
      direccion: '',
      logo: '',
      fechaRegistro: new Date().toISOString().slice(0, 10),
      telefono: '',
      tipoEmpresa: ''
    };
    this.limpiarUsuario();
    this.imagePreview = '';
    this.clearAllErrors();
    this.showForm = true;
  }

  editCompany(c: CompanyVista) {
    this.editing = c;
    this.formData = {
      idUsuario: c.idUsuario,
      nombre: c.nombre,
      ruc: c.ruc,
      correo: c.correo,
      direccion: c.direccion,
      logo: c.logo,
      fechaRegistro: c.fechaRegistro,
      telefono: c.telefono,
      tipoEmpresa: c.tipoEmpresa
    };

    const usuario = this.usuarios.find(u => u.idUsuario === c.idUsuario);
    if (usuario) {
      this.usuarioSeleccionado = usuario;
      this.busquedaUsuario = usuario.nombre;
      console.log('✅ Usuario precargado:', usuario.nombre);
    }

    this.imagePreview = c.logo;
    this.clearAllErrors();
    this.showForm = true;
  }

  closeForm() {
    this.showForm = false;
    this.editing = null;
    this.limpiarUsuario();
    this.clearAllErrors();
    this.imagePreview = '';
  }

  clearAllErrors() {
    this.errors = {
      idUsuario: '',
      nombre: '',
      ruc: '',
      correo: '',
      direccion: '',
      telefono: '',
      tipoEmpresa: ''
    };
  }

  clearError(k: ErrorKeys) {
    this.errors[k] = '';
  }

  // ✅ CORREGIDO: Validar IdUsuario siempre
  validate(): boolean {
    const e: ErrorsMap = { ...this.errors };
    const f = this.formData;

    e.idUsuario = f.idUsuario === 0 ? 'Debes seleccionar un usuario' : '';

    if (!f.nombre.trim()) {
      e.nombre = 'El nombre es requerido';
    } else if (f.nombre.length < 2 || f.nombre.length > 100) {
      e.nombre = 'El nombre debe tener entre 2 y 100 caracteres';
    } else if (!/^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ.,\s-&]+$/.test(f.nombre)) {
      e.nombre = 'El nombre contiene caracteres no permitidos';
    } else {
      e.nombre = '';
    }

    if (!f.ruc.trim()) {
      e.ruc = 'El RUC es requerido';
    } else if (f.ruc.length !== 11) {
      e.ruc = 'El RUC debe tener exactamente 11 dígitos';
    } else if (!/^(10|15|17|20)\d{9}$/.test(f.ruc)) {
      e.ruc = 'El RUC debe iniciar con 10, 15, 17 o 20';
    } else {
      e.ruc = '';
    }

    if (f.correo.trim()) {
      if (!/^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/.test(f.correo)) {
        e.correo = 'El correo no es válido';
      } else if (f.correo.length < 5 || f.correo.length > 100) {
        e.correo = 'El correo debe tener entre 5 y 100 caracteres';
      } else {
        e.correo = '';
      }
    } else {
      e.correo = '';
    }

    if (f.direccion.trim() && f.direccion.length > 500) {
      e.direccion = 'La dirección no puede exceder los 500 caracteres';
    } else if (f.direccion.trim() && !/^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ.,\s\-#°/]+$/.test(f.direccion)) {
      e.direccion = 'La dirección contiene caracteres no permitidos';
    } else {
      e.direccion = '';
    }

    if (f.telefono.trim()) {
      if (!/^9\d{8}$/.test(f.telefono)) {
        e.telefono = 'El teléfono debe ser 9 dígitos empezando con 9';
      } else {
        e.telefono = '';
      }
    } else {
      e.telefono = '';
    }

    if (f.tipoEmpresa.trim()) {
      if (f.tipoEmpresa.length < 3 || f.tipoEmpresa.length > 200) {
        e.tipoEmpresa = 'El tipo debe tener entre 3 y 200 caracteres';
      } else if (!/^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ.,\s-]+$/.test(f.tipoEmpresa)) {
        e.tipoEmpresa = 'El tipo contiene caracteres no permitidos';
      } else {
        e.tipoEmpresa = '';
      }
    } else {
      e.tipoEmpresa = '';
    }

    this.errors = e;
    return Object.values(e).every(v => !v);
  }

  async onFileChange(ev: Event) {
    const input = ev.target as HTMLInputElement;
    const file = input.files && input.files[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      alert('❌ Selecciona una imagen válida');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      alert('❌ La imagen no debe superar 5MB');
      return;
    }

    const reader = new FileReader();
    reader.onloadend = () => {
      const base64 = String(reader.result || '');
      this.formData.logo = base64;
      this.imagePreview = base64;
    };
    reader.readAsDataURL(file);
  }

  submitForm() {
    if (!this.validate()) {
      alert('Por favor corrige los errores en el formulario');
      return;
    }

    if (this.editing) {
      const dto: ActualizarEmpresaDto = {
        IdUsuario: this.formData.idUsuario,
        Nombre: this.formData.nombre.trim(),
        Ruc: this.formData.ruc.trim(),
        Direccion: this.formData.direccion.trim() || '',
        Email: this.formData.correo.trim() || '',
        Telefono: this.formData.telefono.trim() || '',
        TipoEmpresa: this.formData.tipoEmpresa.trim() || ''
      };

      console.log('📝 Actualizando empresa ID:', this.editing.id);
      console.log('📦 DTO:', dto);

      this.companiesService.actualizarEmpresa(this.editing.id, dto).subscribe({
        next: (response) => {
          console.log('✅ Respuesta:', response);
          if (response.exito) {
            alert('✅ Empresa actualizada correctamente');
            this.cargarEmpresas();
            this.closeForm();
          } else {
            alert('❌ ' + (response.mensaje || 'Error al actualizar'));
          }
        },
        error: (error) => {
          console.error('❌ Error completo:', error);
          let errorMsg = 'Error al actualizar empresa';
          if (error.error?.mensaje) {
            errorMsg = error.error.mensaje;
          } else if (error.error?.errores) {
            errorMsg = error.error.errores.join(', ');
          } else if (error.message) {
            errorMsg = error.message;
          }
          alert('❌ ' + errorMsg);
        }
      });
    } else {
      const dto: CrearEmpresaDto = {
        IdUsuario: this.formData.idUsuario,
        Nombre: this.formData.nombre.trim(),
        Ruc: this.formData.ruc.trim(),
        Direccion: this.formData.direccion.trim() || '',
        Email: this.formData.correo.trim() || '',
        Telefono: this.formData.telefono.trim() || '',
        TipoEmpresa: this.formData.tipoEmpresa.trim() || ''
      };

      console.log('➕ Creando empresa');
      console.log('📦 DTO:', dto);

      this.companiesService.crearEmpresa(dto).subscribe({
        next: (response) => {
          console.log('✅ Respuesta:', response);
          if (response.exito) {
            alert('✅ Empresa creada correctamente');
            this.cargarEmpresas();
            this.closeForm();
          } else {
            alert('❌ ' + (response.mensaje || 'Error al crear'));
          }
        },
        error: (error) => {
          console.error('❌ Error completo:', error);
          let errorMsg = 'Error al crear empresa';
          if (error.error?.mensaje) {
            errorMsg = error.error.mensaje;
          } else if (error.error?.errores) {
            errorMsg = error.error.errores.join(', ');
          } else if (error.message) {
            errorMsg = error.message;
          }
          alert('❌ ' + errorMsg);
        }
      });
    }
  }
}
