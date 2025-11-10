

import { Component, OnInit } from '@angular/core';

type EstadoTestimonio = 'Publicado' | 'Pendiente' | 'Oculto';

interface Testimonio {
  id: number;
  cliente: string;
  contenido: string;
  valoracion: number; // 1..5
  fecha: string;      // ISO o yyyy-mm-dd
  propiedad?: string;
  estado: EstadoTestimonio;
}

@Component({
  selector: 'app-testimonies',
  standalone: false,
  templateUrl: './testimonials.html',
  styleUrls: ['./testimonials.css']
})
export class Testimonials implements OnInit {
  // Datos
  testimonios: Testimonio[] = [];
  loading = true;

  // Filtros
  filtroTexto = '';
  filtroEstado: 'Todos' | EstadoTestimonio = 'Todos';
  filtroValorMin = 0;

  // Modal y formulario
  mostrarForm = false;
  editando: Testimonio | null = null;

  // Form state
  form = {
    cliente: '',
    contenido: '',
    valoracion: 5,
    fecha: '',
    propiedad: '',
    estado: 'Publicado' as EstadoTestimonio
  };

  ngOnInit(): void {
    // Simulación de carga (lazy)
    setTimeout(() => {
      this.testimonios = [
        { id: 1, cliente: 'Carlos M.', contenido: 'Excelente servicio, muy recomendados.', valoracion: 5, fecha: '2025-10-01', propiedad: 'Dpto. Miraflores', estado: 'Publicado' },
        { id: 2, cliente: 'Rosa P.', contenido: 'Atención rápida y confiable.', valoracion: 4, fecha: '2025-10-12', propiedad: 'Casa La Molina', estado: 'Publicado' },
        { id: 3, cliente: 'Luis T.', contenido: 'Todo bien, aunque demoró un poco.', valoracion: 3, fecha: '2025-10-18', propiedad: 'Terreno Lurín', estado: 'Pendiente' },
        { id: 4, cliente: 'Andrea R.', contenido: 'El proceso fue claro y eficiente.', valoracion: 5, fecha: '2025-11-02', propiedad: 'Dpto. Barranco', estado: 'Publicado' }
      ];
      this.loading = false;
    }, 800);
  }

  // Métricas
  get total(): number { return this.testimonios.length; }
  get publicados(): number { return this.testimonios.filter(t => t.estado === 'Publicado').length; }
  get pendientes(): number { return this.testimonios.filter(t => t.estado === 'Pendiente').length; }
  get ocultos(): number { return this.testimonios.filter(t => t.estado === 'Oculto').length; }
  get promedio(): number {
    const arr = this.testimonios;
    if (!arr.length) return 0;
    const s = arr.reduce((acc, t) => acc + (Number(t.valoracion) || 0), 0);
    return Math.round((s / arr.length) * 10) / 10;
  }

  // Filtro compuesto
  get listaFiltrada(): Testimonio[] {
    const q = this.filtroTexto.trim().toLowerCase();
    const min = Math.max(0, Math.min(5, Number(this.filtroValorMin) || 0));
    return this.testimonios.filter(t => {
      const coincideTexto = !q
        || t.cliente.toLowerCase().includes(q)
        || t.contenido.toLowerCase().includes(q)
        || (t.propiedad || '').toLowerCase().includes(q);
      const coincideEstado = this.filtroEstado === 'Todos' || t.estado === this.filtroEstado;
      const coincideValor = (Number(t.valoracion) || 0) >= min;
      return coincideTexto && coincideEstado && coincideValor;
    });
  }

  // Helpers
  estrellas(valor: number): number[] {
    const v = Math.max(0, Math.min(5, Math.floor(Number(valor) || 0)));
    return Array.from({ length: 5 }, (_, i) => i < v ? 1 : 0);
  }
  claseEstado(estado: EstadoTestimonio) {
    switch (estado) {
      case 'Publicado': return 'badge aprobado';
      case 'Pendiente': return 'badge pendiente';
      case 'Oculto': return 'badge rechazado';
    }
  }

  // Acciones
  abrirNuevo() {
    this.editando = null;
    this.form = { cliente: '', contenido: '', valoracion: 5, fecha: '', propiedad: '', estado: 'Publicado' };
    this.mostrarForm = true;
  }
  abrirEditar(t: Testimonio) {
    this.editando = { ...t };
    this.form = {
      cliente: t.cliente,
      contenido: t.contenido,
      valoracion: t.valoracion,
      fecha: t.fecha,
      propiedad: t.propiedad || '',
      estado: t.estado
    };
    this.mostrarForm = true;
  }
  cerrarForm() { this.mostrarForm = false; this.editando = null; }

  guardarForm() {
    if (!this.form.cliente || !this.form.contenido || !this.form.fecha) {
      alert('Cliente, contenido y fecha son obligatorios');
      return;
    }
    const payload: Testimonio = {
      id: this.editando ? this.editando.id : this.genId(),
      cliente: this.form.cliente,
      contenido: this.form.contenido,
      valoracion: Math.max(1, Math.min(5, Number(this.form.valoracion) || 1)),
      fecha: this.form.fecha,
      propiedad: this.form.propiedad || '',
      estado: this.form.estado
    };
    if (this.editando) {
      this.testimonios = this.testimonios.map(t => t.id === this.editando!.id ? payload : t);
    } else {
      this.testimonios = [payload, ...this.testimonios];
    }
    this.cerrarForm();
  }
  eliminar(id: number) {
    const ok = confirm('¿Eliminar este testimonio?');
    if (!ok) return;
    this.testimonios = this.testimonios.filter(t => t.id !== id);
  }
  cambiarEstado(id: number, estado: EstadoTestimonio) {
    this.testimonios = this.testimonios.map(t => t.id === id ? { ...t, estado } : t);
  }

  genId() { return Math.max(0, ...this.testimonios.map(t => t.id)) + 1; }
}
