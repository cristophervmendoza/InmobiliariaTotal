import { Component, ViewEncapsulation } from '@angular/core';

type TipoUsuario = 'Cliente' | 'Agente';

interface UsuarioBloqueado {
  idUsuario: number;
  nombre: string;
  email: string;
  telefono: string;
  dni: string;
  intentosLogin: number;
  ultimoLoginAt: string;
  creadoAt: string;
  tipo: TipoUsuario;
  idReferencia: number;
}

@Component({
  selector: 'app-blockedaccounts',
  standalone: false,
  templateUrl: './blockedaccounts.html',
  styleUrls: ['./blockedaccounts.css'],
  encapsulation: ViewEncapsulation.Emulated
})
export class Blockedaccounts {
  // Filtros
  busqueda = '';
  tipoFiltro: 'todos' | TipoUsuario = 'todos';

  // Modal
  modalAbierto = false;
  usuarioSeleccionado: UsuarioBloqueado | null = null;
  motivo = '';

  // Datos de ejemplo
  usuariosBloqueados: UsuarioBloqueado[] = [
    {
      idUsuario: 1,
      nombre: 'Carlos Mendoza',
      email: 'carlos.mendoza@email.com',
      telefono: '987654321',
      dni: '12345678',
      intentosLogin: 5,
      ultimoLoginAt: '2025-10-15',
      creadoAt: '2025-10-15 14:30:00',
      tipo: 'Cliente',
      idReferencia: 101
    },
    {
      idUsuario: 2,
      nombre: 'María Torres',
      email: 'maria.torres@email.com',
      telefono: '956781234',
      dni: '87654321',
      intentosLogin: 6,
      ultimoLoginAt: '2025-10-16',
      creadoAt: '2025-10-16 09:15:00',
      tipo: 'Agente',
      idReferencia: 202
    },
    {
      idUsuario: 3,
      nombre: 'Pedro Salazar',
      email: 'pedro.salazar@email.com',
      telefono: '912345678',
      dni: '45678912',
      intentosLogin: 4,
      ultimoLoginAt: '2025-10-17',
      creadoAt: '2025-10-17 16:45:00',
      tipo: 'Cliente',
      idReferencia: 103
    }
  ];

  // Derivados
  get usuariosFiltrados(): UsuarioBloqueado[] {
    const q = this.busqueda.toLowerCase().trim();
    return this.usuariosBloqueados.filter(u => {
      const coincideBusqueda =
        u.nombre.toLowerCase().includes(q) ||
        u.email.toLowerCase().includes(q) ||
        u.dni.includes(this.busqueda);
      const coincideTipo = this.tipoFiltro === 'todos' || u.tipo === this.tipoFiltro;
      return coincideBusqueda && coincideTipo;
    });
  }

  get totalBloqueados() {
    return this.usuariosBloqueados.length;
  }
  get totalClientes() {
    return this.usuariosBloqueados.filter(u => u.tipo === 'Cliente').length;
  }
  get totalAgentes() {
    return this.usuariosBloqueados.filter(u => u.tipo === 'Agente').length;
  }

  // Utils
  formatearFecha(fecha: string) {
    const d = new Date(fecha);
    return d.toLocaleDateString('es-PE', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  obtenerIniciales(nombre: string) {
    return nombre
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  // Handlers filtros
  onBusqueda(ev: Event) {
    const target = ev.target as HTMLInputElement | null;
    this.busqueda = target?.value ?? '';
  }

  onTipoChange(val: 'todos' | TipoUsuario) {
    this.tipoFiltro = val;
  }

  // Handlers modal
  abrirModal(usuario: UsuarioBloqueado) {
    this.usuarioSeleccionado = usuario;
    this.motivo = '';
    this.modalAbierto = true;
  }

  cerrarModal() {
    this.modalAbierto = false;
    this.usuarioSeleccionado = null;
    this.motivo = '';
  }

  confirmarDesbloqueo() {
    const m = this.motivo.trim();
    if (!m || !this.usuarioSeleccionado) return;
    // Aquí va tu llamada a API para desbloquear
    console.log('Desbloqueando usuario:', this.usuarioSeleccionado, 'Motivo:', m);
    this.cerrarModal();
  }
}
