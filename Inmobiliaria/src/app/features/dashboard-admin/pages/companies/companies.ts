import { Component, OnInit } from '@angular/core';

type Estado = 'activo' | 'inactivo';

interface Company {
  id: string;
  nombre: string;
  ruc: string;
  correo: string;
  direccion: string;
  logo: string; //SOLO PUEDE VISUALIZAR LOS DETALLES 
  fechaRegistro: string;
  telefono: string;
  estado: Estado;
}

interface StatCard {
  titulo: string;
  valor: number;
  color: string;
  icon: string;
  pct?: number; // porcentaje para barra
}

type ErrorKeys = 'nombre' | 'ruc' | 'correo' | 'direccion' | 'telefono' | 'foto';
type ErrorsMap = Record<ErrorKeys, string>;

@Component({
  selector: 'app-companies',
  standalone: false,
  templateUrl: './companies.html',
  styleUrls: ['./companies.css']
})
export class Companies implements OnInit {
  companies: Company[] = [];
  loading = true;

  // Header actions
  exportando = false;

  // Filtros
  searchTerm = '';
  statusFilter: 'todos' | Estado = 'todos';

  // UI: detalle, eliminar, crear/editar
  showDetails = false;
  showDelete = false;
  showForm = false;
  selected: Company | null = null;
  editing: Company | null = null;

  // Form state
  formData: Omit<Company, 'id'> = {
    nombre: '',
    ruc: '',
    correo: '',
    direccion: '',
    logo: '',
    fechaRegistro: new Date().toISOString().slice(0, 10),
    telefono: '',
    estado: 'activo'
  };
  errors: ErrorsMap = { nombre: '', ruc: '', correo: '', direccion: '', telefono: '', foto: '' };
  imagePreview = '';

  ngOnInit(): void {
    this.simularCarga();
  }

  async simularCarga() {
    await new Promise(r => setTimeout(r, 600));
    this.companies = [
      { id: '1', nombre: 'IdealHome SAC', ruc: '20481234567', correo: 'contacto@idealhome.pe', direccion: 'Av. Central 123, Lima', logo: '/company-1.png', fechaRegistro: '2024-03-10', telefono: '999888777', estado: 'activo' },
      { id: '2', nombre: 'UrbanSpace SRL', ruc: '20678901234', correo: 'info@urbanspace.com', direccion: 'Calle Las Flores 567, Lima', logo: '/company-2.png', fechaRegistro: '2023-11-05', telefono: '987654321', estado: 'inactivo' }
    ];
    this.loading = false;
  }

  // Métricas
  get total(): number { return this.companies.length; }
  get activas(): number { return this.companies.filter(c => c.estado === 'activo').length; }
  get inactivas(): number { return this.companies.filter(c => c.estado === 'inactivo').length; }
  private pct(value: number, base: number): number {
    if (!base) return 0;
    return Math.max(0, Math.min(100, Math.round((value / base) * 100)));
  }

  get estadisticas(): StatCard[] {
    const total = this.total;
    const activas = this.activas;
    const inactivas = this.inactivas;
    return [
      { titulo: 'Total', valor: total, color: '#06b6d4', icon: 'building-2', pct: this.pct(total, total || 1) },
      { titulo: 'Activas', valor: activas, color: '#10b981', icon: 'check-circle-2', pct: this.pct(activas, total || 1) },
      { titulo: 'Inactivas', valor: inactivas, color: '#f59e0b', icon: 'clock', pct: this.pct(inactivas, total || 1) }
    ];
  }

  // Filtrado
  get filtered(): Company[] {
    const q = this.searchTerm.trim().toLowerCase();
    const s = this.statusFilter;
    return this.companies.filter(c => {
      const matchesSearch =
        !q ||
        c.nombre.toLowerCase().includes(q) ||
        c.ruc.toLowerCase().includes(q) ||
        c.correo.toLowerCase().includes(q) ||
        c.direccion.toLowerCase().includes(q);
      const matchesStatus = s === 'todos' || c.estado === s;
      return matchesSearch && matchesStatus;
    });
  }

  estadoBadge(estado: Estado) {
    return `badge ${estado === 'activo' ? 'aprobado' : 'rechazado'}`;
  }

  // Export
  async exportarAExcel(datos: Company[]): Promise<void> { await new Promise(r => setTimeout(r, 800)); }
  async handleExportar() {
    const datos = this.filtered;
    if (!datos.length) { alert('No hay datos para exportar'); return; }
    this.exportando = true;
    try { await this.exportarAExcel(datos); }
    catch { alert('Error al exportar'); }
    finally { this.exportando = false; }
  }

  // Detalle / Eliminar
  abrirDetalle(c: Company) { this.selected = c; this.showDetails = true; }
  cerrarDetalle() { this.showDetails = false; this.selected = null; }

  abrirEliminar(c: Company) { this.selected = c; this.showDelete = true; }
  cerrarEliminar() { this.showDelete = false; this.selected = null; }
  confirmarEliminar() {
    if (!this.selected) return;
    this.companies = this.companies.filter(x => x.id !== this.selected!.id);
    this.cerrarEliminar();
  }

  // Crear / Editar
  newCompany() {
    this.editing = null;
    this.formData = {
      nombre: '', ruc: '', correo: '', direccion: '', logo: '',
      fechaRegistro: new Date().toISOString().slice(0, 10),
      telefono: '', estado: 'activo'
    };
    this.imagePreview = '';
    this.clearAllErrors();
    this.showForm = true;
  }
  editCompany(c: Company) {
    this.editing = c;
    this.formData = { nombre: c.nombre, ruc: c.ruc, correo: c.correo, direccion: c.direccion, logo: c.logo, fechaRegistro: c.fechaRegistro, telefono: c.telefono, estado: c.estado };
    this.imagePreview = c.logo;
    this.clearAllErrors();
    this.showForm = true;
  }
  closeForm() { this.showForm = false; this.editing = null; this.clearAllErrors(); this.imagePreview = ''; }

  clearAllErrors() { this.errors = { nombre: '', ruc: '', correo: '', direccion: '', telefono: '', foto: '' }; }
  clearError(k: ErrorKeys) { this.errors[k] = ''; }

  validate(): boolean {
    const e: ErrorsMap = { ...this.errors };
    const f = this.formData;
    e.nombre = !f.nombre.trim() ? 'El nombre es requerido' : '';
    e.ruc = !f.ruc.trim() ? 'El RUC es requerido' : '';
    if (!f.correo.trim()) e.correo = 'El correo es requerido';
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(f.correo)) e.correo = 'El correo no es válido';
    else e.correo = '';
    e.direccion = !f.direccion.trim() ? 'La dirección es requerida' : '';
    e.telefono = !f.telefono.trim() ? 'El teléfono es requerido' : '';
    this.errors = e;
    return Object.values(e).every(v => !v);
  }

  async onFileChange(ev: Event) {
    const input = ev.target as HTMLInputElement;
    const file = input.files && input.files[0];
    if (!file) return;
    if (!file.type.startsWith('image/')) { this.errors.foto = 'Selecciona una imagen válida'; return; }
    if (file.size > 5 * 1024 * 1024) { this.errors.foto = 'La imagen no debe superar 5MB'; return; }
    const reader = new FileReader();
    reader.onloadend = () => {
      const base64 = String(reader.result || '');
      this.formData.logo = base64;
      this.imagePreview = base64;
      this.errors.foto = '';
    };
    reader.readAsDataURL(file);
  }

  submitForm() {
    if (!this.validate()) return;
    if (this.editing) {
      const id = this.editing.id;
      const updated: Company = { id, ...this.formData };
      this.companies = this.companies.map(c => c.id === id ? updated : c);
    } else {
      const newC: Company = { id: Date.now().toString(), ...this.formData };
      this.companies = [newC, ...this.companies];
    }
    this.closeForm();
  }
}
