import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

interface Usuario {
  idUsuario: number;
  nombreCompleto: string;
  email: string;
}

@Component({
  selector: 'app-modal-enviar-formulario',
  standalone: false,
  templateUrl: './modal-enviar-formulario.component.html',
  styleUrls: ['./modal-enviar-formulario.component.css']
})
export class ModalEnviarFormularioComponent implements OnInit {
  @Input() isVisible = false;
  @Output() close = new EventEmitter<void>();
  @Output() success = new EventEmitter<number>();

  usuarios: Usuario[] = [];
  usuariosFiltrados: Usuario[] = [];
  busqueda = '';
  usuarioSeleccionado: Usuario | null = null;
  loading = true;
  enviando = false;

  ngOnInit(): void {
    this.cargarUsuarios();
  }

  async cargarUsuarios(): Promise<void> {
    this.loading = true;

    try {
      // TODO: Conectar con API C#
      // const response = await fetch('/api/admin/usuarios?rol=cliente');
      // this.usuarios = await response.json();

      // Mock data temporal
      setTimeout(() => {
        this.usuarios = [
          { idUsuario: 1, nombreCompleto: 'Juan Pérez González', email: 'juan.perez@gmail.com' },
          { idUsuario: 2, nombreCompleto: 'María González López', email: 'maria.gonzalez@gmail.com' },
          { idUsuario: 3, nombreCompleto: 'Carlos Rodríguez Silva', email: 'carlos.rodriguez@gmail.com' },
          { idUsuario: 4, nombreCompleto: 'Ana López Martínez', email: 'ana.lopez@gmail.com' },
          { idUsuario: 5, nombreCompleto: 'Pedro Sánchez Torres', email: 'pedro.sanchez@gmail.com' },
          { idUsuario: 6, nombreCompleto: 'Laura Fernández Ruiz', email: 'laura.fernandez@gmail.com' }
        ];
        this.usuariosFiltrados = [...this.usuarios];
        this.loading = false;
      }, 800);
    } catch (error) {
      console.error('Error cargando usuarios:', error);
      this.loading = false;
    }
  }

  filtrarUsuarios(): void {
    const termino = this.busqueda.toLowerCase().trim();

    if (!termino) {
      this.usuariosFiltrados = [...this.usuarios];
      return;
    }

    this.usuariosFiltrados = this.usuarios.filter(u =>
      u.nombreCompleto.toLowerCase().includes(termino) ||
      u.email.toLowerCase().includes(termino)
    );
  }

  seleccionarUsuario(usuario: Usuario): void {
    this.usuarioSeleccionado = usuario;
  }

  async enviarFormulario(): Promise<void> {
    if (!this.usuarioSeleccionado || this.enviando) return;

    this.enviando = true;

    try {
      // TODO: Conectar con API C#
      // const response = await fetch('/api/admin/testimonios/enviar-formulario', {
      //   method: 'POST',
      //   headers: { 'Content-Type': 'application/json' },
      //   body: JSON.stringify({ idUsuario: this.usuarioSeleccionado.idUsuario })
      // });

      // Simulación de envío
      setTimeout(() => {
        console.log('Formulario enviado a:', this.usuarioSeleccionado);
        this.success.emit(this.usuarioSeleccionado!.idUsuario);
        this.cerrar();
        this.enviando = false;
      }, 1500);
    } catch (error) {
      console.error('Error enviando formulario:', error);
      this.enviando = false;
      alert('Error al enviar el formulario');
    }
  }

  cerrar(): void {
    this.close.emit();
    this.resetear();
  }

  resetear(): void {
    this.busqueda = '';
    this.usuarioSeleccionado = null;
    this.usuariosFiltrados = [...this.usuarios];
  }

  getInitials(nombre: string): string {
    return nombre
      .split(' ')
      .map(n => n[0])
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }
}
