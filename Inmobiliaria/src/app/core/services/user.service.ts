import { Injectable, inject } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiService } from './api.service';

// ============= INTERFACES =============

export interface Usuario {
  idUsuario: number;
  nombre: string;
  dni: string;
  email: string;
  telefono?: string;
  intentosLogin: number;
  idEstadoUsuario?: number;
  ultimoLoginAt?: string;
  creadoAt: string;
  actualizadoAt: string;
  estaBloqueado: boolean;
  requiereCambioPassword: boolean;
  esNuevo: boolean;
  diasDesdeUltimoLogin?: number;
  nombreCorto: string;
  iniciales: string;
  esActivo: boolean;
  esInactivo: boolean;
}

export interface UsuarioRegistroDto {
  nombre: string;
  dni: string;
  email: string;
  password: string;
  telefono?: string;
  idEstadoUsuario?: number;
}

export interface UsuarioUpdateDto {
  nombre: string;
  email: string;
  telefono?: string;
  idEstadoUsuario?: number;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface CambiarPasswordDto {
  passwordActual: string;
  nuevaPassword: string;
}

export interface CambiarUsuarioEstadoDto {
  idEstadoUsuario: number;
}

export interface FiltrosUsuarios {
  termino?: string;
  bloqueados?: boolean;
  idEstado?: number;
}

export interface EstadisticasUsuarios {
  totalUsuarios: number;
  bloqueados: number;
  nuncaLogueados: number;
  activos: number;
  inactivos: number;
}

export interface ApiResponse<T> {
  exito: boolean;
  mensaje?: string;
  data?: T;
  total?: number;
  id?: number;
  rol?: string;
  usuario?: T;
}

export interface ApiResponsePaginado<T> {
  exito: boolean;
  mensaje?: string;
  data?: T[];
  paginaActual: number;
  tamanoPagina: number;
  totalRegistros: number;
  totalPaginas: number;
}

export interface ValidacionPassword {
  exito: boolean;
  fuerza: number;
  esSegura: boolean;
  nivel: string;
}

// ============= SERVICIO =============

@Injectable({ providedIn: 'root' })
export class UserService {
  private api = inject(ApiService);
  private readonly BASE_PATH = '/api/Usuario';

  // ✅ Registro
  registrar(dto: UsuarioRegistroDto): Observable<ApiResponse<any>> {
    return this.api.post<ApiResponse<any>>(`${this.BASE_PATH}/registro`, dto).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ Login
  login(dto: LoginDto): Observable<ApiResponse<Usuario>> {
    return this.api.post<ApiResponse<Usuario>>(`${this.BASE_PATH}/login`, dto).pipe(
      map(response => {
        console.log('🔐 Login exitoso:', response);
        return response;
      }),
      catchError(this.handleError)
    );
  }

  // ✅ Obtener usuario por ID
  obtenerUsuarioPorId(id: number): Observable<ApiResponse<Usuario>> {
    return this.api.get<ApiResponse<Usuario>>(`${this.BASE_PATH}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ Obtener usuario por email
  obtenerUsuarioPorEmail(email: string): Observable<ApiResponse<Usuario>> {
    return this.api.get<ApiResponse<Usuario>>(`${this.BASE_PATH}/email/${email}`).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ Obtener usuario por DNI
  obtenerUsuarioPorDni(dni: string): Observable<ApiResponse<Usuario>> {
    return this.api.get<ApiResponse<Usuario>>(`${this.BASE_PATH}/dni/${dni}`).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ Listar todos los usuarios
  listarUsuarios(): Observable<ApiResponse<Usuario[]>> {
    return this.api.get<ApiResponse<Usuario[]>>(this.BASE_PATH).pipe(
      map(response => {
        console.log('👥 Usuarios obtenidos:', response.total || 0);
        return response;
      }),
      catchError(this.handleError)
    );
  }

  // ✅ Listar usuarios paginados
  listarUsuariosPaginados(
    pagina: number = 1,
    tamanoPagina: number = 10
  ): Observable<ApiResponsePaginado<Usuario>> {
    return this.api.get<ApiResponsePaginado<Usuario>>(
      `${this.BASE_PATH}/paginado?pagina=${pagina}&tamanoPagina=${tamanoPagina}`
    ).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ Buscar usuarios con filtros
  buscarUsuarios(filtros: FiltrosUsuarios = {}): Observable<ApiResponse<Usuario[]>> {
    const params = this.construirQueryParams(filtros);
    return this.api.get<ApiResponse<Usuario[]>>(`${this.BASE_PATH}/buscar${params}`).pipe(
      map(response => {
        console.log('🔍 Búsqueda:', filtros, 'Resultados:', response.total || 0);
        return response;
      }),
      catchError(this.handleError)
    );
  }

  // ✅ Obtener usuarios bloqueados
  obtenerUsuariosBloqueados(): Observable<ApiResponse<Usuario[]>> {
    return this.api.get<ApiResponse<Usuario[]>>(`${this.BASE_PATH}/bloqueados`).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ Obtener usuarios inactivos
  obtenerUsuariosInactivos(diasInactividad: number = 90): Observable<ApiResponse<Usuario[]>> {
    return this.api.get<ApiResponse<Usuario[]>>(
      `${this.BASE_PATH}/inactivos?diasInactividad=${diasInactividad}`
    ).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ Obtener estadísticas
  obtenerEstadisticas(): Observable<ApiResponse<EstadisticasUsuarios>> {
    return this.api.get<ApiResponse<EstadisticasUsuarios>>(`${this.BASE_PATH}/estadisticas`).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ Actualizar usuario
  actualizarUsuario(id: number, dto: UsuarioUpdateDto): Observable<ApiResponse<any>> {
    return this.api.put<ApiResponse<any>>(`${this.BASE_PATH}/${id}`, dto).pipe(
      map(response => {
        console.log('✅ Usuario actualizado:', id);
        return response;
      }),
      catchError(this.handleError)
    );
  }

  // ✅ Cambiar contraseña
  cambiarPassword(id: number, dto: CambiarPasswordDto): Observable<ApiResponse<any>> {
    return this.api.put<ApiResponse<any>>(`${this.BASE_PATH}/${id}/cambiar-password`, dto).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ Desbloquear usuario
  desbloquearUsuario(id: number): Observable<ApiResponse<any>> {
    return this.api.put<ApiResponse<any>>(`${this.BASE_PATH}/${id}/desbloquear`, {}).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ Eliminar usuario
  eliminarUsuario(id: number): Observable<ApiResponse<any>> {
    return this.api.delete<ApiResponse<any>>(`${this.BASE_PATH}/${id}`).pipe(
      map(response => {
        console.log('🗑️ Usuario eliminado:', id);
        return response;
      }),
      catchError(this.handleError)
    );
  }

  // ✅ Validar fuerza de contraseña
  validarFuerzaPassword(password: string): Observable<ValidacionPassword> {
    return this.api.get<ValidacionPassword>(
      `${this.BASE_PATH}/validar-fuerza-password?password=${encodeURIComponent(password)}`
    ).pipe(
      catchError(this.handleError)
    );
  }

  // ============= MÉTODOS AUXILIARES =============

  private construirQueryParams(filtros: FiltrosUsuarios): string {
    const params = new URLSearchParams();

    if (filtros.termino?.trim()) {
      params.append('termino', filtros.termino.trim());
    }
    if (filtros.bloqueados !== undefined) {
      params.append('bloqueados', filtros.bloqueados.toString());
    }
    if (filtros.idEstado !== undefined && filtros.idEstado > 0) {
      params.append('idEstado', filtros.idEstado.toString());
    }

    const queryString = params.toString();
    return queryString ? `?${queryString}` : '';
  }

  // ✅ Validación de DNI
  validarDni(dni: string): boolean {
    return /^\d{8}$/.test(dni);
  }

  // ✅ Validación de email
  validarEmail(email: string): boolean {
    return /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/.test(email);
  }

  // ✅ Validación de teléfono peruano
  validarTelefono(telefono: string): boolean {
    return /^9\d{8}$/.test(telefono);
  }

  // ✅ Validación de contraseña segura
  validarPasswordSegura(password: string): boolean {
    // Mínimo 8 caracteres, al menos una mayúscula, una minúscula, un número y un carácter especial
    return password.length >= 8 &&
      /[A-Z]/.test(password) &&
      /[a-z]/.test(password) &&
      /\d/.test(password) &&
      /[@$!%*?&#]/.test(password);
  }

  // ✅ Manejo de errores
  private handleError(error: any): Observable<never> {
    console.error('❌ Error en UsuariosService:', error);

    let errorMessage = 'Error desconocido';

    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error del cliente: ${error.error.message}`;
    } else {
      errorMessage = error.error?.mensaje ||
        error.message ||
        `Error del servidor: ${error.status}`;
    }

    return throwError(() => new Error(errorMessage));
  }
}
