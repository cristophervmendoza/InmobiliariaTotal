import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

// ============= INTERFACES =============

export interface Empresa {
  idEmpresa: number;
  idUsuario: number;
  nombre: string;
  ruc: string;
  direccion?: string;
  email?: string;
  telefono?: string;
  tipoEmpresa?: string;
  fechaRegistro: string;
  actualizadoAt: string;
  nombreUsuario?: string;
}

export interface CrearEmpresaDto {
  IdUsuario: number;
  Nombre: string;
  Ruc: string;
  Direccion?: string;
  Email?: string;
  Telefono?: string;
  TipoEmpresa?: string;
}

export interface ActualizarEmpresaDto {
  IdUsuario: number;
  Nombre: string;
  Ruc: string;
  Direccion?: string;
  Email?: string;
  Telefono?: string;
  TipoEmpresa?: string;
}

export interface FiltrosEmpresas {
  termino?: string;
  tipoEmpresa?: string;
  idUsuario?: number;
}

export interface EstadisticasEmpresas {
  totalEmpresas: number;
  empresasJuridicas: number;
  personasNaturales: number;
  tiposEmpresa: number;
  usuariosConEmpresas: number;
}

export interface ApiResponse<T> {
  exito: boolean;
  mensaje?: string;
  data?: T;
  total?: number;
  id?: number;
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

// ============= SERVICIO =============

@Injectable({ providedIn: 'root' })
export class CompaniesService {
  private api = inject(ApiService);
  private readonly BASE_PATH = '/api/Empresa';

  crearEmpresa(dto: CrearEmpresaDto): Observable<ApiResponse<any>> {
    return this.api.post<ApiResponse<any>>(this.BASE_PATH, dto);
  }

  obtenerEmpresaPorId(id: number): Observable<ApiResponse<Empresa>> {
    return this.api.get<ApiResponse<Empresa>>(`${this.BASE_PATH}/${id}`);
  }

  obtenerEmpresaPorRuc(ruc: string): Observable<ApiResponse<Empresa>> {
    return this.api.get<ApiResponse<Empresa>>(`${this.BASE_PATH}/ruc/${ruc}`);
  }

  listarEmpresas(): Observable<ApiResponse<Empresa[]>> {
    return this.api.get<ApiResponse<Empresa[]>>(this.BASE_PATH);
  }

  listarEmpresasPaginadas(
    pagina: number = 1,
    tamanoPagina: number = 10
  ): Observable<ApiResponsePaginado<Empresa>> {
    return this.api.get<ApiResponsePaginado<Empresa>>(
      `${this.BASE_PATH}/paginado?pagina=${pagina}&tamanoPagina=${tamanoPagina}`
    );
  }

  buscarEmpresas(filtros: FiltrosEmpresas = {}): Observable<ApiResponse<Empresa[]>> {
    const params = this.construirQueryParams(filtros);
    return this.api.get<ApiResponse<Empresa[]>>(`${this.BASE_PATH}/buscar${params}`);
  }

  obtenerEmpresasPorUsuario(idUsuario: number): Observable<ApiResponse<Empresa[]>> {
    return this.api.get<ApiResponse<Empresa[]>>(`${this.BASE_PATH}/usuario/${idUsuario}`);
  }

  obtenerEmpresasPorTipo(tipoEmpresa: string): Observable<ApiResponse<Empresa[]>> {
    return this.api.get<ApiResponse<Empresa[]>>(`${this.BASE_PATH}/tipo/${tipoEmpresa}`);
  }

  obtenerEstadisticas(): Observable<ApiResponse<EstadisticasEmpresas>> {
    return this.api.get<ApiResponse<EstadisticasEmpresas>>(`${this.BASE_PATH}/estadisticas`);
  }

  validarRuc(ruc: string): Observable<ApiResponse<{ existe: boolean; mensaje: string }>> {
    return this.api.get<ApiResponse<{ existe: boolean; mensaje: string }>>(
      `${this.BASE_PATH}/validar-ruc/${ruc}`
    );
  }

  actualizarEmpresa(id: number, dto: ActualizarEmpresaDto): Observable<ApiResponse<any>> {
    return this.api.put<ApiResponse<any>>(`${this.BASE_PATH}/${id}`, dto);
  }

  eliminarEmpresa(id: number): Observable<ApiResponse<any>> {
    return this.api.delete<ApiResponse<any>>(`${this.BASE_PATH}/${id}`);
  }

  private construirQueryParams(filtros: FiltrosEmpresas): string {
    const params = new URLSearchParams();

    if (filtros.termino?.trim()) {
      params.append('termino', filtros.termino.trim());
    }
    if (filtros.tipoEmpresa?.trim()) {
      params.append('tipoEmpresa', filtros.tipoEmpresa.trim());
    }
    if (filtros.idUsuario && filtros.idUsuario > 0) {
      params.append('idUsuario', filtros.idUsuario.toString());
    }

    const queryString = params.toString();
    return queryString ? `?${queryString}` : '';
  }

  validarFormatoRuc(ruc: string): boolean {
    const regex = /^(10|15|17|20)\d{9}$/;
    return regex.test(ruc);
  }

  validarEmail(email: string): boolean {
    const regex = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return regex.test(email);
  }

  validarTelefono(telefono: string): boolean {
    const regex = /^9\d{8}$/;
    return regex.test(telefono);
  }
}
