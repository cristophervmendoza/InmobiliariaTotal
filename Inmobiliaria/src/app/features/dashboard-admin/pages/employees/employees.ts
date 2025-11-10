import { Component, OnInit, inject } from '@angular/core';
import { EmployeesService, Empleado, CrearEmpleadoDto, ActualizarEmpleadoDto } from '../../../../core/services/employees.service';

@Component({
  selector: 'app-employees',
  standalone: false,
  styleUrl: './employees.css',
  templateUrl: './employees.html',
})
export class Employees implements OnInit {
  private employeesService = inject(EmployeesService);

  empleados: Empleado[] = [];
  isLoading = false;

  // Filtros
  searchTerm = '';
  statusFilter: 'todos' | 'activo' | 'inactivo' = 'todos';

  // Formulario
  showForm = false;
  editingEmployee: Empleado | null = null;

  // Modal detalles
  showModal = false;
  selectedEmployee: Empleado | null = null;

  // Modal de confirmación
  showConfirmModal = false;
  confirmAction: 'delete' | 'toggle' | null = null;
  confirmMessage = '';
  confirmEmployeeId: number | null = null;

  // Estado de formulario
  formData = {
    nombre: '',
    dni: '',
    email: '',
    telefono: '',
    password: '',
    idEstadoUsuario: 1,
  };

  errors: Record<string, string> = {};

  ngOnInit() {
    console.log('✅ ngOnInit ejecutado');
    this.cargarEmpleados();
  }

  // ✅ Cargar empleados desde el backend
  cargarEmpleados() {
    console.log('🔄 Cargando empleados...');
    this.isLoading = true;

    this.employeesService.listarEmpleados().subscribe({
      next: (response) => {
        console.log('📦 Respuesta del backend:', response);

        if (response.exito && response.data) {
          this.empleados = response.data.map(emp => ({
            ...emp,
            estado: emp.idEstadoUsuario === 1 ? 'activo' as const : 'inactivo' as const
          }));
          console.log('✅ Empleados procesados:', this.empleados);
          console.log('📊 Total empleados:', this.empleados.length);
        } else {
          console.warn('⚠️ Respuesta sin datos o no exitosa');
          this.empleados = [];
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Error al cargar empleados:', error);
        this.isLoading = false;
        alert('Error al cargar empleados: ' + (error.message || 'Error desconocido'));
      }
    });
  }

  // ✅ Derivados - Con logging
  get filteredEmpleados(): Empleado[] {
    const term = this.searchTerm.toLowerCase().trim();

    const filtered = this.empleados.filter((emp) => {
      const matchesSearch = !term ||
        emp.nombre.toLowerCase().includes(term) ||
        emp.email.toLowerCase().includes(term);

      const matchesStatus = this.statusFilter === 'todos' || emp.estado === this.statusFilter;

      return matchesSearch && matchesStatus;
    });

    console.log('🔍 Empleados filtrados:', filtered.length, 'de', this.empleados.length);
    return filtered;
  }

  get stats() {
    const total = this.empleados.length;
    const activos = this.empleados.filter((e) => e.estado === 'activo').length;
    const inactivos = total - activos;

    return {
      total,
      pendientes: inactivos,
      revisadas: total,
      aprobadas: activos,
    };
  }

  // Abrir formulario para crear
  openCreate() {
    console.log('➕ Abriendo formulario crear');
    this.editingEmployee = null;
    this.resetForm();
    this.showForm = true;
  }

  // ✅ Guardar (crear o editar)
  handleSave() {
    console.log('💾 Intentando guardar...');

    if (!this.validateForm()) {
      console.warn('⚠️ Validación fallida');
      return;
    }

    this.isLoading = true;

    if (this.editingEmployee) {
      // Actualizar
      const dto: ActualizarEmpleadoDto = {
        nombre: this.formData.nombre.trim(),
        email: this.formData.email.trim(),
        telefono: this.formData.telefono.trim(),
        idEstadoUsuario: this.formData.idEstadoUsuario,
      };

      console.log('📝 Actualizando empleado:', dto);

      this.employeesService.actualizarEmpleado(this.editingEmployee.idAgenteInmobiliario, dto).subscribe({
        next: (response) => {
          if (response.exito) {
            alert('✅ Empleado actualizado correctamente');
            this.cargarEmpleados();
            this.closeForm();
          } else {
            alert('❌ ' + (response.mensaje || 'Error al actualizar'));
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ Error al actualizar empleado:', error);
          alert('❌ Error al actualizar empleado');
          this.isLoading = false;
        }
      });
    } else {
      // Crear
      const dto: CrearEmpleadoDto = {
        nombre: this.formData.nombre.trim(),
        dni: this.formData.dni.trim(),
        email: this.formData.email.trim(),
        telefono: this.formData.telefono.trim(),
        password: this.formData.password,
        idEstadoUsuario: this.formData.idEstadoUsuario,
      };

      console.log('➕ Creando empleado:', dto);

      this.employeesService.crearEmpleado(dto).subscribe({
        next: (response) => {
          if (response.exito) {
            alert('✅ Empleado creado correctamente');
            this.cargarEmpleados();
            this.closeForm();
          } else {
            alert('❌ ' + (response.mensaje || 'Error al crear'));
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ Error al crear empleado:', error);
          alert('❌ Error al crear empleado');
          this.isLoading = false;
        }
      });
    }
  }

  // ✅ Eliminar empleado
  handleDelete(id: number) {
    console.log('🗑️ Solicitando eliminar empleado:', id);
    const empleado = this.empleados.find(e => e.idAgenteInmobiliario === id);
    if (!empleado) {
      console.error('❌ Empleado no encontrado');
      return;
    }

    this.confirmEmployeeId = id;
    this.confirmAction = 'delete';
    this.confirmMessage = `¿Estás seguro de que deseas eliminar a ${empleado.nombre}?`;
    this.showConfirmModal = true;
  }

  // ✅ Cambiar estado
  toggleEstado(id: number) {
    console.log('🔄 Cambiando estado del empleado:', id);
    const empleado = this.empleados.find(e => e.idAgenteInmobiliario === id);
    if (!empleado) {
      console.error('❌ Empleado no encontrado');
      return;
    }

    const nuevoEstado = empleado.estado === 'activo' ? 'inactivo' : 'activo';
    const accion = nuevoEstado === 'activo' ? 'activar' : 'desactivar';

    this.confirmEmployeeId = id;
    this.confirmAction = 'toggle';
    this.confirmMessage = `¿Deseas ${accion} a ${empleado.nombre}?`;
    this.showConfirmModal = true;
  }

  // ✅ Confirmar acción (MÉTODO CORREGIDO)
  confirmActionHandler() {
    if (!this.confirmEmployeeId) {
      console.error('❌ No hay empleado seleccionado');
      return;
    }

    console.log('✔️ Confirmando acción:', this.confirmAction);
    this.isLoading = true;

    if (this.confirmAction === 'delete') {
      // Eliminar empleado
      this.employeesService.eliminarEmpleado(this.confirmEmployeeId).subscribe({
        next: (response) => {
          if (response.exito) {
            alert('✅ Empleado eliminado correctamente');
            this.cargarEmpleados();
          } else {
            alert('❌ ' + (response.mensaje || 'Error al eliminar'));
          }
          this.isLoading = false;
          this.closeConfirmModal();
        },
        error: (error) => {
          console.error('❌ Error al eliminar empleado:', error);
          alert('❌ Error al eliminar empleado');
          this.isLoading = false;
          this.closeConfirmModal();
        }
      });
    } else if (this.confirmAction === 'toggle') {
      // Cambiar estado del empleado
      const empleado = this.empleados.find(e => e.idAgenteInmobiliario === this.confirmEmployeeId);
      if (!empleado) {
        console.error('❌ Empleado no encontrado');
        this.isLoading = false;
        this.closeConfirmModal();
        return;
      }

      // ✅ Usar el nuevo método dedicado de cambiar estado
      const nuevoEstado = empleado.estado === 'activo' ? 2 : 1;

      this.employeesService.cambiarEstadoEmpleado(this.confirmEmployeeId, nuevoEstado).subscribe({
        next: (response) => {
          if (response.exito) {
            alert(`✅ ${response.mensaje || 'Estado cambiado correctamente'}`);
            this.cargarEmpleados();
          } else {
            alert('❌ ' + (response.mensaje || 'Error al cambiar estado'));
          }
          this.isLoading = false;
          this.closeConfirmModal();
        },
        error: (error) => {
          console.error('❌ Error al cambiar estado:', error);
          alert('❌ Error al cambiar estado del empleado');
          this.isLoading = false;
          this.closeConfirmModal();
        }
      });
    }
  }

  closeConfirmModal() {
    console.log('❌ Cerrando modal de confirmación');
    this.showConfirmModal = false;
    this.confirmAction = null;
    this.confirmMessage = '';
    this.confirmEmployeeId = null;
  }

  // Editar
  handleEdit(emp: Empleado) {
    console.log('✏️ Editando empleado:', emp);
    this.editingEmployee = emp;
    this.formData = {
      nombre: emp.nombre,
      dni: emp.dni,
      email: emp.email,
      telefono: emp.telefono,
      password: '',
      idEstadoUsuario: emp.idEstadoUsuario,
    };
    this.errors = {};
    this.showForm = true;
  }

  // Ver detalles
  handleView(emp: Empleado) {
    console.log('👁️ Viendo detalles de empleado:', emp);
    this.selectedEmployee = emp;
    this.showModal = true;
  }

  handleEditFromModal() {
    if (!this.selectedEmployee) return;
    this.showModal = false;
    this.handleEdit(this.selectedEmployee);
  }

  // Filtros
  onSearchChange(ev: Event) {
    const target = ev.target as HTMLInputElement | null;
    this.searchTerm = target?.value ?? '';
    console.log('🔍 Búsqueda:', this.searchTerm);
  }

  onStatusChange(status: 'todos' | 'activo' | 'inactivo') {
    this.statusFilter = status;
    console.log('📊 Filtro de estado:', status);
  }

  // Form helpers
  resetForm() {
    this.formData = {
      nombre: '',
      dni: '',
      email: '',
      telefono: '',
      password: '',
      idEstadoUsuario: 1,
    };
    this.errors = {};
  }

  closeForm() {
    console.log('❌ Cerrando formulario');
    this.showForm = false;
    this.editingEmployee = null;
    this.resetForm();
  }

  validateForm(): boolean {
    const e: Record<string, string> = {};

    if (!this.formData.nombre.trim()) {
      e['nombre'] = 'El nombre es requerido';
    }

    const dniTrimmed = this.formData.dni.trim();
    if (!this.editingEmployee) {
      if (!dniTrimmed) {
        e['dni'] = 'El DNI es requerido';
      } else if (!/^\d{8}$/.test(dniTrimmed)) {
        e['dni'] = 'El DNI debe tener 8 dígitos';
      }
    }

    if (!this.formData.email.trim()) {
      e['email'] = 'El correo es requerido';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.formData.email)) {
      e['email'] = 'El correo no es válido';
    }

    const telTrimmed = this.formData.telefono.trim();
    if (!telTrimmed) {
      e['telefono'] = 'El teléfono es requerido';
    } else if (!/^9\d{8}$/.test(telTrimmed)) {
      e['telefono'] = 'El teléfono debe tener 9 dígitos y empezar con 9';
    }

    if (!this.editingEmployee) {
      if (!this.formData.password.trim()) {
        e['password'] = 'La contraseña es requerida';
      } else if (this.formData.password.length < 8) {
        e['password'] = 'La contraseña debe tener al menos 8 caracteres';
      }
    }

    this.errors = e;
    console.log('📋 Errores de validación:', e);
    return Object.keys(e).length === 0;
  }

  handleInputChange(name: keyof typeof this.formData, value: string | number) {
    if (name === 'idEstadoUsuario' && typeof value === 'string') {
      this.formData[name] = parseInt(value, 10);
    } else {
      (this.formData as any)[name] = value;
    }

    if (this.errors[name]) {
      const { [name]: _omit, ...rest } = this.errors;
      this.errors = rest;
    }
  }
}
