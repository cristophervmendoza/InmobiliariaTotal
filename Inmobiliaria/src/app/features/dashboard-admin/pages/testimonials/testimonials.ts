

import { Component, OnInit } from '@angular/core';
import { TestimonioRecibido, TestimonioPublicado } from './models/testimonio.interface';

@Component({
  selector: 'app-testimonials',
  standalone: false,
  templateUrl: './testimonials.html',
  styleUrls: ['./testimonials.css']
})
export class TestimonialsComponent implements OnInit {  // ← CAMBIA AQUÍ

  activeTab: 'recepcion' | 'publicaciones' = 'recepcion';
  loading = true;

  // Datos
  testimoniosRecibidos: TestimonioRecibido[] = [];
  testimoniosPublicados: TestimonioPublicado[] = [];

  // Modales
  showModalEnviar = false;
  showModalVer = false;
  showModalPublicar = false;
  testimonioSeleccionado: TestimonioRecibido | null = null;
  testimonioParaPublicar: TestimonioRecibido | TestimonioPublicado | null = null;

  ngOnInit(): void {
    this.cargarDatos();
  }

  // ============================================
  // CARGAR DATOS
  // ============================================

  async cargarDatos(): Promise<void> {
    this.loading = true;

    try {
      // TODO: Conectar con API C# usando el service
      // import { TestimonialsService } from './services/testimonials.service';
      // constructor(private testimonialsService: TestimonialsService) {}
      // this.testimonialsService.getTodosTestimonios().subscribe(...)

      // Datos mock temporales
      setTimeout(() => {
        this.testimoniosRecibidos = [
          {
            idTestimonio: 1,
            idUsuario: 101,
            nombreCompleto: 'Juan Pérez González',
            email: 'juan.perez@gmail.com',
            contenido: 'Excelente servicio, muy profesionales. Me mantuvieron informado en cada paso del proceso de venta.',
            valoracion: 5,
            fecha: '2025-01-11T10:30:00',
            estado: 'pendiente'
          },
          {
            idTestimonio: 2,
            idUsuario: 102,
            nombreCompleto: 'María González López',
            email: 'maria.gonzalez@gmail.com',
            contenido: 'Muy satisfecha con el resultado. Proceso transparente y rápido. Totalmente recomendados.',
            valoracion: 5,
            fecha: '2025-10-28T15:20:00',
            estado: 'pendiente'
          },
          {
            idTestimonio: 3,
            idUsuario: 103,
            nombreCompleto: 'Carlos Rodríguez',
            email: 'carlos.r@gmail.com',
            contenido: 'Buen servicio en general, aunque el proceso tomó más tiempo del esperado.',
            valoracion: 4,
            fecha: '2025-09-15T09:15:00',
            estado: 'rechazado'
          }
        ];

        this.testimoniosPublicados = [
          {
            idTestimonio: 10,
            nombre: 'Luis Martínez',
            ubicacion: 'Barranco, Lima',
            valoracion: 5,
            comentario: 'Proceso muy transparente y profesional. Me mantuvieron informado en cada paso y lograron vender mi local en tiempo récord.',
            tipoPropiedad: 'Local comercial',
            tiempo: '28 días',
            fotoUrl: undefined,
            creadoAt: '2025-01-05T00:00:00'
          },
          {
            idTestimonio: 11,
            nombre: 'Ana Flores',
            ubicacion: 'Miraflores, Lima',
            valoracion: 5,
            comentario: 'Increíble experiencia. El equipo fue muy atento y logró superar mis expectativas. Recomiendo 100%.',
            tipoPropiedad: 'Departamento',
            tiempo: '35 días',
            fotoUrl: undefined,
            creadoAt: '2024-12-20T00:00:00'
          }
        ];

        this.loading = false;
      }, 1000);
    } catch (error) {
      console.error('Error cargando datos:', error);
      this.loading = false;
    }
  }

  // ============================================
  // MODALES - ENVIAR FORMULARIO
  // ============================================

  abrirModalEnviar(): void {
    this.showModalEnviar = true;
  }

  cerrarModalEnviar(): void {
    this.showModalEnviar = false;
  }

  onFormularioEnviado(): void {
    console.log('Formulario enviado correctamente');
    // Mostrar notificación de éxito (opcional)
    alert('Formulario enviado exitosamente al cliente');
  }

  // ============================================
  // MODALES - VER TESTIMONIO
  // ============================================

  verTestimonio(testimonio: TestimonioRecibido): void {
    this.testimonioSeleccionado = testimonio;
    this.showModalVer = true;
  }

  cerrarModalVer(): void {
    this.showModalVer = false;
    this.testimonioSeleccionado = null;
  }

  // ============================================
  // MODALES - PUBLICAR/EDITAR TESTIMONIO
  // ============================================

  abrirModalPublicar(testimonio: TestimonioRecibido | TestimonioPublicado | null): void {
    this.testimonioParaPublicar = testimonio;
    this.showModalPublicar = true;
  }

  cerrarModalPublicar(): void {
    this.showModalPublicar = false;
    this.testimonioParaPublicar = null;
  }

  onTestimonioPublicado(datos: TestimonioPublicado): void {
    console.log('Testimonio publicado:', datos);

    // TODO: Guardar en backend
    // this.testimonialsService.publicarTestimonio(datos).subscribe(...)

    // Agregar a la lista de publicados (temporal)
    const nuevoTestimonio: TestimonioPublicado = {
      idTestimonio: this.testimoniosPublicados.length + 1,
      ...datos,
      creadoAt: new Date().toISOString()
    };

    this.testimoniosPublicados.unshift(nuevoTestimonio);

    // Cambiar a tab de publicaciones
    this.activeTab = 'publicaciones';

    // Mostrar notificación
    alert('Testimonio publicado exitosamente');
  }

  // ============================================
  // ACCIONES - TESTIMONIOS RECIBIDOS
  // ============================================

  async rechazarTestimonio(idTestimonio: number): Promise<void> {
    if (!confirm('¿Estás seguro de rechazar este testimonio?')) return;

    try {
      // TODO: Llamar API
      // await this.testimonialsService.rechazarTestimonio(idTestimonio).subscribe(...)

      // Actualizar estado localmente (temporal)
      const testimonio = this.testimoniosRecibidos.find(t => t.idTestimonio === idTestimonio);
      if (testimonio) {
        testimonio.estado = 'rechazado';
      }

      console.log('Testimonio rechazado:', idTestimonio);
      alert('Testimonio rechazado');
    } catch (error) {
      console.error('Error rechazando testimonio:', error);
      alert('Error al rechazar el testimonio');
    }
  }

  // ============================================
  // ACCIONES - TESTIMONIOS PUBLICADOS
  // ============================================

  editarPublicacion(testimonio: TestimonioPublicado): void {
    this.abrirModalPublicar(testimonio);
  }

  async eliminarPublicacion(idTestimonio: number): Promise<void> {
    if (!confirm('¿Estás seguro de eliminar esta publicación?')) return;

    try {
      // TODO: Llamar API
      // await this.testimonialsService.eliminarTestimonio(idTestimonio).subscribe(...)

      // Eliminar localmente (temporal)
      this.testimoniosPublicados = this.testimoniosPublicados.filter(
        t => t.idTestimonio !== idTestimonio
      );

      console.log('Publicación eliminada:', idTestimonio);
      alert('Publicación eliminada');
    } catch (error) {
      console.error('Error eliminando publicación:', error);
      alert('Error al eliminar la publicación');
    }
  }

  // ============================================
  // UTILIDADES
  // ============================================

  getInitials(nombre: string): string {
    return nombre
      .split(' ')
      .map(n => n[0])
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }

  truncateText(text: string, maxLength: number): string {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
  }
}
