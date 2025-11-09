import { Component, OnInit } from '@angular/core';

type Estado = 'Pendiente' | 'Aprobado' | 'Rechazado';
type TipoVista = 'pendientes' | 'aprobados' | 'rechazados' | 'todos';

interface InmuebleOfrecido {
  id: number;
  propertyType: string;
  fullAddress: string;
  district: string;
  description: string;
  image: string | null;
  estado: Estado;
  nombrePropietario: string;
  email: string;
}

@Component({
  selector: 'app-analysis',
  standalone: false,
  templateUrl: './analysis.html',
  styleUrls: ['./analysis.css']
})
export class Analysis implements OnInit {
  // Datos
  inmuebles: InmuebleOfrecido[] = [];
  seleccionado: InmuebleOfrecido | null = null;

  // Filtros
  filtroTexto = '';
  filtroEstado: 'Todos' | Estado = 'Todos';
  filtroTipo: 'Todos' | 'Casa' | 'Departamento' | 'Terreno' | 'Otro' = 'Todos';

  // Vista/reportes estilo selector
  tipos: TipoVista[] = ['todos', 'pendientes', 'aprobados', 'rechazados'];
  tipoActivo: TipoVista = 'todos';

  // Exportación
  exportando = false;

  ngOnInit(): void {
    setTimeout(() => {
      this.inmuebles = [
        {
          id: 1,
          propertyType: 'Casa',
          fullAddress: 'Av. Las Flores 456',
          district: 'Miraflores',
          description: 'Casa moderna con jardín amplio y acabados de lujo.',
          image: '/ejemplo1.jpg',
          estado: 'Pendiente',
          nombrePropietario: 'Juan Pérez González',
          email: 'juan.perez@gmail.com'
        },
        {
          id: 2,
          propertyType: 'Departamento',
          fullAddress: 'Calle Central 102',
          district: 'San Isidro',
          description: 'Departamento con vista al parque y cochera techada.',
          image: '/ejemplo2.jpg',
          estado: 'Aprobado',
          nombrePropietario: 'María López',
          email: 'maria.lopez@gmail.com'
        },
        {
          id: 3,
          propertyType: 'Terreno',
          fullAddress: 'Av. El Sol 230',
          district: 'Lurín',
          description: 'Terreno plano ideal para proyectos residenciales o comerciales.',
          image: '/ejemplo3.jpg',
          estado: 'Pendiente',
          nombrePropietario: 'Carlos Ramos',
          email: 'carlos.ramos@hotmail.com'
        },
        {
          id: 4,
          propertyType: 'Casa',
          fullAddress: 'Jr. Los Álamos 98',
          district: 'Surco',
          description: 'Amplia casa de dos pisos con patio interior y cochera doble.',
          image: '/ejemplo4.jpg',
          estado: 'Rechazado',
          nombrePropietario: 'Ana Fernández',
          email: 'ana.fernandez@gmail.com'
        },
        {
          id: 5,
          propertyType: 'Departamento',
          fullAddress: 'Malecón Balta 785',
          district: 'Barranco',
          description: 'Departamento frente al mar con terraza panorámica y acabados premium.',
          image: '/ejemplo5.jpg',
          estado: 'Aprobado',
          nombrePropietario: 'Luis García',
          email: 'luis.garcia@outlook.com'
        }
      ];
    }, 800);
  }

  // Estadísticas para tarjetas
  get total(): number { return this.inmuebles.length; }
  get pendientes(): number { return this.inmuebles.filter(i => i.estado === 'Pendiente').length; }
  get aprobados(): number { return this.inmuebles.filter(i => i.estado === 'Aprobado').length; }
  get rechazados(): number { return this.inmuebles.filter(i => i.estado === 'Rechazado').length; }

  get estadisticas() {
    return [
      { titulo: 'Total de Inmuebles', valor: this.total, icon: 'building-2', color: '#06b6d4' },
      { titulo: 'Pendientes', valor: this.pendientes, icon: 'loader-2', color: '#0ea5e9' },
      { titulo: 'Aprobados', valor: this.aprobados, icon: 'check-circle-2', color: '#16a34a' },
      { titulo: 'Rechazados', valor: this.rechazados, icon: 'x-circle', color: '#dc2626' }
    ];
  }

  // Vista activa (estilo selector)
  setTipoActivo(tipo: TipoVista) { this.tipoActivo = tipo; }

  // Filtro compuesto por texto, estado, tipo y vista activa
  get inmueblesFiltrados(): InmuebleOfrecido[] {
    const texto = this.filtroTexto.trim().toLowerCase();
    return this.inmuebles
      .filter(i => {
        const coincideTexto =
          !texto ||
          i.nombrePropietario.toLowerCase().includes(texto) ||
          i.fullAddress.toLowerCase().includes(texto);
        const coincideEstado = this.filtroEstado === 'Todos' || i.estado === this.filtroEstado;
        const coincideTipo = this.filtroTipo === 'Todos' || i.propertyType === this.filtroTipo;
        return coincideTexto && coincideEstado && coincideTipo;
      })
      .filter(i => {
        switch (this.tipoActivo) {
          case 'pendientes': return i.estado === 'Pendiente';
          case 'aprobados': return i.estado === 'Aprobado';
          case 'rechazados': return i.estado === 'Rechazado';
          default: return true;
        }
      });
  }

  // Acciones
  actualizarEstado(id: number, nuevo: Estado) {
    this.inmuebles = this.inmuebles.map(i => i.id === id ? { ...i, estado: nuevo } : i);
  }
  abrirModal(i: InmuebleOfrecido) { this.seleccionado = i; }
  cerrarModal() { this.seleccionado = null; }

  // Exportación simulada
  async exportarAExcel(datos: any[]): Promise<void> {
    // Simulación de exportación
    await new Promise(r => setTimeout(r, 600));
  }
  async handleExportar(): Promise<void> {
    const datos = this.inmueblesFiltrados;
    if (!datos.length) { alert('No hay datos para exportar'); return; }
    this.exportando = true;
    try { await this.exportarAExcel(datos); }
    catch (e) { console.error(e); alert('Error al exportar'); }
    finally { this.exportando = false; }
  }

  // Helpers UI
  estadoClase(estado: Estado) {
    switch (estado) {
      case 'Aprobado': return 'badge aprobado';
      case 'Rechazado': return 'badge rechazado';
      default: return 'badge pendiente';
    }
  }
}
