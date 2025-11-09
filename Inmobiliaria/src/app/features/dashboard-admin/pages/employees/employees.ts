import { Component, EventEmitter, Input, Output } from '@angular/core';

export interface Empleado {
  id: string;
  nombre: string;
  apellido: string;
  dni: string;
  correo: string;
  direccion: string;
  fotoPerfil: string;
  fechaIngreso: string;
  telefono: string;
  estado: 'activo' | 'inactivo';
}

@Component({
  selector: 'app-employees',
  standalone: false,
  styleUrl: './employees.css',
  templateUrl: './employees.html',

})
export class Employees {
  @Input() empleadosIniciales: Empleado[] = [];
  @Output() empleadosChange = new EventEmitter<Empleado[]>();

  empleados: Empleado[] = [];

  // Filtros
  searchTerm = '';
  statusFilter: 'todos' | 'activo' | 'inactivo' = 'todos';

  // Formulario
  showForm = false;
  editingEmployee: Empleado | null = null;

  // Modal detalles
  showModal = false;
  selectedEmployee: Empleado | null = null;

  // Estado de formulario (ASCII keys)
  formData: {
    nombre: string;
    apellido: string;
    dni: string;
    correo: string;
    direccion: string;
    fotoPerfil: string;
    fechaIngreso: string;
    telefono: string;
    contrasena: string;
    estado: 'activo' | 'inactivo';
  } = {
      nombre: '',
      apellido: '',
      dni: '',
      correo: '',
      direccion: '',
      fotoPerfil: '',
      fechaIngreso: new Date().toISOString().split('T')[0],
      telefono: '',
      contrasena: '',
      estado: 'activo',
    };

  // Usa también claves ASCII en errors
  errors: Record<string, string> = {};
  imagePreview = '';

  ngOnInit() {
    this.empleados = [...this.empleadosIniciales];
  }

  // Derivados
  get filteredEmpleados(): Empleado[] {
    const term = this.searchTerm.toLowerCase().trim();
    return this.empleados.filter((emp) => {
      const matchesSearch =
        emp.nombre.toLowerCase().includes(term) ||
        emp.apellido.toLowerCase().includes(term);
      const matchesStatus = this.statusFilter === 'todos' || emp.estado === this.statusFilter;
      return matchesSearch && matchesStatus;
    });
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

  // Acciones
  openCreate() {
    this.editingEmployee = null;
    this.resetForm();
    this.showForm = true;
  }

  handleSave() {
    if (!this.validateForm()) return;

    const payload: Omit<Empleado, 'id'> = {
      nombre: this.formData.nombre,
      apellido: this.formData.apellido,
      dni: this.formData.dni,
      correo: this.formData.correo,
      direccion: this.formData.direccion,
      fotoPerfil: this.formData.fotoPerfil || '/assets/business-agent.png',
      fechaIngreso: this.formData.fechaIngreso,
      telefono: this.formData.telefono,
      estado: this.formData.estado,
    };

    if (this.editingEmployee) {
      this.empleados = this.empleados.map((emp) =>
        emp.id === this.editingEmployee!.id ? { ...payload, id: emp.id } : emp
      );
    } else {
      const newEmpleado: Empleado = { ...payload, id: Date.now().toString() };
      this.empleados = [...this.empleados, newEmpleado];
    }

    this.empleadosChange.emit(this.empleados);
    this.closeForm();
  }

  handleDelete(id: string) {
    if (!confirm('¿Estás seguro de que deseas eliminar este empleado?')) return;
    this.empleados = this.empleados.filter((e) => e.id !== id);
    this.empleadosChange.emit(this.empleados);
  }

  handleEdit(emp: Empleado) {
    this.editingEmployee = emp;
    this.formData = {
      nombre: emp.nombre,
      apellido: emp.apellido,
      dni: emp.dni,
      correo: emp.correo,
      direccion: emp.direccion,
      fotoPerfil: emp.fotoPerfil,
      fechaIngreso: emp.fechaIngreso,
      telefono: emp.telefono,
      contrasena: '',
      estado: emp.estado,
    };
    this.imagePreview = emp.fotoPerfil;
    this.errors = {};
    this.showForm = true;
  }

  handleView(emp: Empleado) {
    this.selectedEmployee = emp;
    this.showModal = true;
  }

  // Filtros (unificada a Event siempre)
  onSearchChange(ev: Event) {
    const target = ev.target as HTMLInputElement | null;
    this.searchTerm = target?.value ?? '';
  }

  onStatusChange(status: 'todos' | 'activo' | 'inactivo') {
    this.statusFilter = status;
  }

  // Form helpers
  resetForm() {
    this.formData = {
      nombre: '',
      apellido: '',
      dni: '',
      correo: '',
      direccion: '',
      fotoPerfil: '',
      fechaIngreso: new Date().toISOString().split('T')[0],
      telefono: '',
      contrasena: '',
      estado: 'activo',
    };
    this.errors = {};
    this.imagePreview = '';
  }

  closeForm() {
    this.showForm = false;
    this.editingEmployee = null;
  }

  validateForm(): boolean {
    const e: Record<string, string> = {};
    if (!this.formData.nombre.trim()) e['nombre'] = 'El nombre es requerido';
    if (!this.formData.apellido.trim()) e['apellido'] = 'El apellido es requerido';
    if (!this.formData.dni.trim()) e['dni'] = 'El DNI es requerido';
    if (!this.formData.correo.trim()) e['correo'] = 'El correo es requerido';
    if (this.formData.correo && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.formData.correo)) {
      e['correo'] = 'El correo no es válido';
    }
    if (!this.formData.direccion.trim()) e['direccion'] = 'La dirección es requerida';
    if (!this.formData.telefono.trim()) e['telefono'] = 'El teléfono es requerido';
    if (!this.editingEmployee && !this.formData.contrasena.trim()) {
      e['contrasena'] = 'La contraseña es requerida';
    }
    this.errors = e;
    return Object.keys(e).length === 0;
  }

  handleInputChange(name: string, value: string) {
    this.formData = { ...this.formData, [name]: value } as any;
    if (this.errors[name]) {
      const { [name]: _omit, ...rest } = this.errors;
      this.errors = rest;
    }
  }

  handleImageChange(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.errors = { ...this.errors, fotoPerfil: 'Por favor selecciona una imagen válida' };
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.errors = { ...this.errors, fotoPerfil: 'La imagen no debe superar 5MB' };
      return;
    }

    const reader = new FileReader();
    reader.onloadend = () => {
      const base64String = reader.result as string;
      this.formData = { ...this.formData, fotoPerfil: base64String };
      this.imagePreview = base64String;
      const { fotoPerfil, ...rest } = this.errors;
      this.errors = rest;
    };
    reader.readAsDataURL(file);
  }
}
