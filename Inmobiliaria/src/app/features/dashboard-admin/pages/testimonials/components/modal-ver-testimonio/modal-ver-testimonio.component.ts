import { Component, EventEmitter, Input, Output } from '@angular/core';

interface Testimonio {
  idTestimonio: number;
  nombreCompleto: string;
  email: string;
  contenido: string;
  valoracion: number;
  fecha: string;
  estado: string;
}

@Component({
  selector: 'app-modal-ver-testimonio',
  standalone: false,
  templateUrl: './modal-ver-testimonio.component.html',
  styleUrls: ['./modal-ver-testimonio.component.css']
})
export class ModalVerTestimonioComponent {
  @Input() testimonio: Testimonio | null = null;
  @Input() isVisible = false;
  @Output() close = new EventEmitter<void>();

  cerrar(): void {
    this.close.emit();
  }

  getInitials(nombre: string): string {
    return nombre
      .split(' ')
      .map(n => n[0])
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }

  formatearFecha(fechaString: string): string {
    const fecha = new Date(fechaString);
    const opciones: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    };
    return fecha.toLocaleDateString('es-ES', opciones);
  }

  getEstrellas(): number[] {
    return Array(5).fill(0).map((_, i) => i + 1);
  }
}
