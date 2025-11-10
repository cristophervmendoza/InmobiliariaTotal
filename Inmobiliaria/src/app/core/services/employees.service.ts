import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

// Interfaces
export interface Empleado {
  idAgenteInmobiliario: number;
  idUsuario: number;
  nombre: string;
  dni: string;
  email: string;
  telefono: string;
  fechaIngreso: string;
  idEstadoUsuario: number;
  estado: 'activo' | 'inactivo';
  fotoPerfil?: string;
  direccion?: string;
}

export interface CrearEmpleadoDto {
  nombre: string;
  dni: string;
  email: string;
  password: string;
  telefono: string;
  idEstadoUsuario?: number;
}

export interface ActualizarEmpleadoDto {
  nombre: string;
  email: string;
  telefono: string;
  idEstadoUsuario?: number;
}
// ✅ Agregar interface al inicio (después de ActualizarEmpleadoDto)
export interface CambiarUsuarioEstadoDto {
  idEstadoUsuario: number;
}

export interface CambiarPasswordDto {
  passwordActual: string;
  nuevaPassword: string;
}

export interface ApiResponse<T> {
  exito: boolean;
  mensaje?: string;
  data?: T;
  total?: number;
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

export interface Estadisticas {
  total: number;
  activos: number;
  inactivos: number;
}

@Injectable({ providedIn: 'root' })
export class EmployeesService {
  private api = inject(ApiService);
  private readonly BASE_PATH = '/api/AgenteInmobiliario';

  // Crear agente inmobiliario
  crearEmpleado(dto: CrearEmpleadoDto): Observable<ApiResponse<any>> {
    return this.api.post<ApiResponse<any>>(this.BASE_PATH, dto);
  }

  // Obtener agente por ID
  obtenerEmpleadoPorId(id: number): Observable<ApiResponse<Empleado>> {
    return this.api.get<ApiResponse<Empleado>>(`${this.BASE_PATH}/${id}`);
  }

  // Obtener agente por ID de usuario
  obtenerEmpleadoPorIdUsuario(idUsuario: number): Observable<ApiResponse<Empleado>> {
    return this.api.get<ApiResponse<Empleado>>(`${this.BASE_PATH}/usuario/${idUsuario}`);
  }

  // Listar todos los agentes
  listarEmpleados(): Observable<ApiResponse<Empleado[]>> {
    return this.api.get<ApiResponse<Empleado[]>>(this.BASE_PATH);
  }

  // Listar agentes paginados
  listarEmpleadosPaginados(pagina: number = 1, tamanoPagina: number = 10): Observable<ApiResponsePaginado<Empleado>> {
    return this.api.get<ApiResponsePaginado<Empleado>>(
      `${this.BASE_PATH}/paginado?pagina=${pagina}&tamanoPagina=${tamanoPagina}`
    );
  }

  // Buscar agentes
  buscarEmpleados(termino?: string): Observable<ApiResponse<Empleado[]>> {
    const query = termino ? `?termino=${encodeURIComponent(termino)}` : '';
    return this.api.get<ApiResponse<Empleado[]>>(`${this.BASE_PATH}/buscar${query}`);
  }

  // Actualizar agente
  actualizarEmpleado(id: number, dto: ActualizarEmpleadoDto): Observable<ApiResponse<any>> {
    return this.api.put<ApiResponse<any>>(`${this.BASE_PATH}/${id}`, dto);
  }

  // Cambiar contraseña
  cambiarPassword(id: number, dto: CambiarPasswordDto): Observable<ApiResponse<any>> {
    return this.api.put<ApiResponse<any>>(`${this.BASE_PATH}/${id}/cambiar-password`, dto);
  }

  // Verificar si es agente
  verificarEsAgente(idUsuario: number): Observable<ApiResponse<{ esAgente: boolean }>> {
    return this.api.get<ApiResponse<{ esAgente: boolean }>>(`${this.BASE_PATH}/verificar/${idUsuario}`);
  }

  // Obtener estadísticas
  obtenerEstadisticas(): Observable<ApiResponse<Estadisticas>> {
    return this.api.get<ApiResponse<Estadisticas>>(`${this.BASE_PATH}/estadisticas`);
  }

  // Eliminar agente
  eliminarEmpleado(id: number): Observable<ApiResponse<any>> {
    return this.api.delete<ApiResponse<any>>(`${this.BASE_PATH}/${id}`);
  }



// ✅ Agregar método al servicio (después de actualizarEmpleado)
cambiarEstadoEmpleado(id: number, nuevoEstado: number): Observable < ApiResponse < any >> {
  return this.api.put<ApiResponse<any>>(
    `${this.BASE_PATH}/${id}/cambiar-estado`,
    { idEstadoUsuario: nuevoEstado }
  );
}

}
