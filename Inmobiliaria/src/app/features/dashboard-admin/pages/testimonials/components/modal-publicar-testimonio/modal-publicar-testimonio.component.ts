import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

interface TestimonioPublicado {
  nombre: string;
  ubicacion: string;
  valoracion: number;
  comentario: string;
  tipoPropiedad: string;
  tiempo: string;
  fotoUrl?: string;
  videoUrl?: string;
}

@Component({
  selector: 'app-modal-publicar-testimonio',
  standalone: false,
  templateUrl: './modal-publicar-testimonio.component.html',
  styleUrls: ['./modal-publicar-testimonio.component.css']
})
export class ModalPublicarTestimonioComponent implements OnInit {
  @Input() testimonio: any = null;
  @Input() isVisible = false;
  @Output() close = new EventEmitter<void>();
  @Output() submit = new EventEmitter<TestimonioPublicado>();

  formData: TestimonioPublicado = {
    nombre: '',
    ubicacion: '',
    valoracion: 5,
    comentario: '',
    tipoPropiedad: '',
    tiempo: '',
    fotoUrl: '',
    videoUrl: ''
  };

  previewFoto: string | null = null;
  mediaType: 'foto' | 'video' | null = null;
  caracteresRestantes = 500;
  modoEdicion = false;

  tiposPropiedad = [
    'Departamento',
    'Casa',
    'Terreno'
  ];

  ngOnInit(): void {
    this.inicializarFormulario();
  }

  inicializarFormulario(): void {
    if (this.testimonio) {
      this.modoEdicion = true;
      this.formData = {
        nombre: this.testimonio.nombreCompleto || this.testimonio.nombre || '',
        ubicacion: this.testimonio.ubicacion || '',
        valoracion: this.testimonio.valoracion || 5,
        comentario: this.testimonio.contenido || this.testimonio.comentario || '',
        tipoPropiedad: this.testimonio.tipoPropiedad || '',
        tiempo: this.testimonio.tiempo || '',
        fotoUrl: this.testimonio.fotoUrl || '',
        videoUrl: this.testimonio.videoUrl || ''
      };

      if (this.formData.fotoUrl) {
        this.mediaType = 'foto';
        this.previewFoto = this.formData.fotoUrl;
      } else if (this.formData.videoUrl) {
        this.mediaType = 'video';
      }

      this.actualizarContador();
    }
  }

  setValoracion(valor: number): void {
    this.formData.valoracion = valor;
  }

  handleFotoChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (file) {
      if (file.size > 5 * 1024 * 1024) {
        alert('La imagen no debe superar los 5MB');
        return;
      }

      const reader = new FileReader();
      reader.onload = (e) => {
        this.previewFoto = e.target?.result as string;
        this.formData.fotoUrl = this.previewFoto;
        this.mediaType = 'foto';
        this.formData.videoUrl = '';
      };
      reader.readAsDataURL(file);
    }
  }

  handleVideoChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (file) {
      if (file.size > 50 * 1024 * 1024) {
        alert('El video no debe superar los 50MB');
        return;
      }

      // Aquí normalmente subirías el video a un servidor
      // Por ahora simulamos con un placeholder
      this.formData.videoUrl = 'video-placeholder.mp4';
      this.mediaType = 'video';
      this.formData.fotoUrl = '';
      this.previewFoto = null;
    }
  }

  eliminarFoto(): void {
    this.previewFoto = null;
    this.formData.fotoUrl = '';
    if (this.mediaType === 'foto') {
      this.mediaType = null;
    }
  }

  eliminarVideo(): void {
    this.formData.videoUrl = '';
    if (this.mediaType === 'video') {
      this.mediaType = null;
    }
  }

  actualizarContador(): void {
    this.caracteresRestantes = 500 - this.formData.comentario.length;
  }

  validarFormulario(): boolean {
    if (!this.formData.nombre.trim()) {
      alert('El nombre del cliente es obligatorio');
      return false;
    }

    if (!this.formData.ubicacion.trim()) {
      alert('La ubicación es obligatoria');
      return false;
    }

    if (!this.formData.tipoPropiedad) {
      alert('Debes seleccionar el tipo de propiedad');
      return false;
    }

    if (!this.formData.tiempo.trim()) {
      alert('Los días de venta son obligatorios');
      return false;
    }

    if (!this.formData.comentario.trim()) {
      alert('El testimonio es obligatorio');
      return false;
    }

    if (this.formData.comentario.length > 500) {
      alert('El testimonio no puede superar los 500 caracteres');
      return false;
    }

    return true;
  }

  guardar(): void {
    if (!this.validarFormulario()) return;

    // Emitir datos al componente padre
    this.submit.emit(this.formData);
    this.cerrar();
  }

  cerrar(): void {
    this.close.emit();
    this.resetear();
  }

  resetear(): void {
    this.formData = {
      nombre: '',
      ubicacion: '',
      valoracion: 5,
      comentario: '',
      tipoPropiedad: '',
      tiempo: '',
      fotoUrl: '',
      videoUrl: ''
    };
    this.previewFoto = null;
    this.mediaType = null;
    this.caracteresRestantes = 500;
    this.modoEdicion = false;
  }

  getEstrellas(): number[] {
    return Array(5).fill(0).map((_, i) => i + 1);
  }
}
