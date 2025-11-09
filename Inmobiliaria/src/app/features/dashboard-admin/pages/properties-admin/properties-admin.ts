import { Component, OnInit } from '@angular/core';

type Moneda = 'USD' | 'PEN';
type EstadoOferta = 'activa' | 'reservada' | 'vendida' | 'pausada' | 'cerrada';
type TipoPropiedad = 'Casa' | 'Departamento' | 'Terreno' | 'Otro';

interface Propiedad {
  id: number;
  codigo: string;
  titulo: string;
  tipo: TipoPropiedad;
  precio: number;
  moneda: Moneda;
  estado: EstadoOferta;
  area: number;
  habitaciones: number;
  banos: number;
  direccion: string;
  distrito: string;
  agente: string;
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
  // Datos
  propiedades: Propiedad[] = [];
  loading = true;

  // Filtros
  busqueda = '';
  filtroTipo: 'Todos' | TipoPropiedad = 'Todos';
  filtroEstado: 'Todos' | EstadoOferta = 'Todos';

  // Export
  exportando = false;

  // Modales simples
  modalDetalleAbierto = false;
  modalEliminarAbierto = false;
  seleccionado: Propiedad | null = null;

  ngOnInit(): void {
    setTimeout(() => {
      this.propiedades = [
        {
          id: 1,
          codigo: 'PR-001',
          titulo: 'Casa en San Isidro',
          tipo: 'Casa',
          precio: 250000,
          moneda: 'USD',
          estado: 'activa',
          area: 180,
          habitaciones: 3,
          banos: 2,
          direccion: 'Calle Los Rosales 456',
          distrito: 'San Isidro',
          agente: 'Juan Pérez',
          destacada: true,
          fechaPublicacion: '2025-10-12',
          vistas: 45,
          imagenes: ['/ejemplo1.jpg']
        },
        {
          id: 2,
          codigo: 'PR-002',
          titulo: 'Departamento Moderno en Miraflores',
          tipo: 'Departamento',
          precio: 1500,
          moneda: 'USD',
          estado: 'activa',
          area: 120,
          habitaciones: 2,
          banos: 2,
          direccion: 'Av. Larco 1234',
          distrito: 'Miraflores',
          agente: 'María García',
          destacada: false,
          fechaPublicacion: '2025-10-21',
          vistas: 89,
          imagenes: ['/ejemplo2.jpg']
        },
        {
          id: 3,
          codigo: 'PR-003',
          titulo: 'Terreno Comercial en Surco',
          tipo: 'Terreno',
          precio: 180000,
          moneda: 'USD',
          estado: 'pausada',
          area: 300,
          habitaciones: 0,
          banos: 0,
          direccion: 'Av. El Derby 890',
          distrito: 'Surco',
          agente: 'Carlos López',
          destacada: false,
          fechaPublicacion: '2025-09-28',
          vistas: 23,
          imagenes: ['/ejemplo3.jpg']
        },
        {
          id: 4,
          codigo: 'PR-004',
          titulo: 'Casa amplia en La Molina',
          tipo: 'Casa',
          precio: 780000,
          moneda: 'USD',
          estado: 'vendida',
          area: 320,
          habitaciones: 4,
          banos: 3,
          direccion: 'Calle Pinos 120',
          distrito: 'La Molina',
          agente: 'Ana Rodríguez',
          destacada: false,
          fechaPublicacion: '2025-10-05',
          vistas: 11,
          imagenes: ['/ejemplo4.jpg']
        }
      ];
      this.loading = false;
    }, 800);
  }

  // Estadísticas (tarjetas)
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

  // Filtro compuesto
  get propiedadesFiltradas(): Propiedad[] {
    const q = this.busqueda.trim().toLowerCase();
    return this.propiedades.filter(p => {
      const coincideTexto =
        !q ||
        p.titulo.toLowerCase().includes(q) ||
        p.codigo.toLowerCase().includes(q) ||
        p.distrito.toLowerCase().includes(q) ||
        p.tipo.toLowerCase().includes(q);
      const coincideTipo = this.filtroTipo === 'Todos' || p.tipo === this.filtroTipo;
      const coincideEstado = this.filtroEstado === 'Todos' || p.estado === this.filtroEstado;
      return coincideTexto && coincideTipo && coincideEstado;
    });
  }

  // Helpers de UI
  estadoClaseBadge(estado: EstadoOferta) {
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

  precioStr(p: Propiedad): string {
    const simbolo = p.moneda === 'USD' ? '$' : 'S/';
    return `${simbolo} ${Number(p.precio).toLocaleString('es-PE')}`;
  }

  // Acciones
  abrirDetalle(p: Propiedad) { this.seleccionado = p; this.modalDetalleAbierto = true; }
  cerrarDetalle() { this.modalDetalleAbierto = false; this.seleccionado = null; }
  abrirEliminar(p: Propiedad) { this.seleccionado = p; this.modalEliminarAbierto = true; }
  cerrarEliminar() { this.modalEliminarAbierto = false; this.seleccionado = null; }

  async exportarAExcel(datos: Propiedad[]): Promise<void> {
    await new Promise(r => setTimeout(r, 600));
  }
  async handleExportar(): Promise<void> {
    const datos = this.propiedadesFiltradas;
    if (!datos.length) { alert('No hay datos para exportar'); return; }
    this.exportando = true;
    try { await this.exportarAExcel(datos); }
    catch { alert('Error al exportar'); }
    finally { this.exportando = false; }
  }

  confirmarEliminar(): void {
    if (!this.seleccionado) return;
    this.propiedades = this.propiedades.filter(p => p.id !== this.seleccionado!.id);
    this.cerrarEliminar();
  }
}
