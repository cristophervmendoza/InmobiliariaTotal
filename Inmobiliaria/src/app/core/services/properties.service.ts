import { Injectable, inject } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiService } from './api.service';

// ============= INTERFACES =============

export interface Propiedad {
  idPropiedad: number;
  idUsuario: number;
  idTipoPropiedad: number;
  idEstadoPropiedad: number;
  titulo: string;
  direccion: string;
  precio: number;
  descripcion?: string;
  areaTerreno?: number;
  tipoMoneda: 'PEN' | 'USD' | 'EUR';
  habitacion?: number;
  bano?: number;
  estacionamiento?: number;
  fotoPropiedad?: string;
  creadoAt: string;
  actualizadoAt: string;
}

export interface CrearPropiedadDto {
  idUsuario: number;
  idTipoPropiedad: number;
  idEstadoPropiedad: number;
  titulo: string;
  direccion: string;
  precio: number;
  descripcion?: string;
  areaTerreno?: number;
  tipoMoneda: 'PEN' | 'USD' | 'EUR';
  habitacion?: number;
  bano?: number;
  estacionamiento?: number;
  fotoPropiedad?: File;
}

export interface ActualizarPropiedadDto {
  idUsuario?: number;           // ✅ AGREGADO: Permitir cambiar el agente
  idTipoPropiedad: number;
  idEstadoPropiedad: number;
  titulo: string;
  direccion: string;
  precio: number;
  descripcion?: string;
  areaTerreno?: number;
  tipoMoneda: 'PEN' | 'USD' | 'EUR';
  habitacion?: number;
  bano?: number;
  estacionamiento?: number;
  fotoPropiedad?: File;
}


export interface FiltrosPropiedades {
  termino?: string;
  idTipoPropiedad?: number;
  idEstadoPropiedad?: number;
  precioMin?: number;
  precioMax?: number;
  habitacionesMin?: number;
  tipoMoneda?: string;
  idUsuario?: number;
}

export interface Estadisticas {
  totalPropiedades: number;
  precioPromedio: number;
  precioMinimo: number;
  precioMaximo: number;
  conFoto: number;
  usuariosUnicos: number;
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

// ============= SERVICIO =============

@Injectable({ providedIn: 'root' })
export class PropertiesService {
  private api = inject(ApiService);
  private readonly BASE_PATH = '/api/Propiedad';

  // ✅ MEJORADO: Validación antes de enviar
  crearPropiedad(dto: CrearPropiedadDto): Observable<ApiResponse<any>> {
    // Validaciones básicas
    if (!dto.titulo?.trim()) {
      return throwError(() => new Error('El título es requerido'));
    }
    if (!dto.direccion?.trim()) {
      return throwError(() => new Error('La dirección es requerida'));
    }
    if (dto.precio <= 0) {
      return throwError(() => new Error('El precio debe ser mayor a 0'));
    }
    if (dto.idUsuario <= 0) {
      return throwError(() => new Error('Debe seleccionar un agente'));
    }

    const formData = this.convertirAFormData(dto);
    return this.api.post<ApiResponse<any>>(this.BASE_PATH, formData).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ MEJORADO: Con manejo de errores
  obtenerPropiedadPorId(id: number): Observable<ApiResponse<Propiedad>> {
    if (id <= 0) {
      return throwError(() => new Error('ID inválido'));
    }
    return this.api.get<ApiResponse<Propiedad>>(`${this.BASE_PATH}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ MEJORADO: Con logging
  listarPropiedades(): Observable<ApiResponse<Propiedad[]>> {
    return this.api.get<ApiResponse<Propiedad[]>>(this.BASE_PATH).pipe(
      map(response => {
        console.log('📦 Propiedades obtenidas:', response.data?.length || 0);
        return response;
      }),
      catchError(this.handleError)
    );
  }

  listarPropiedadesPaginadas(
    pagina: number = 1,
    tamanoPagina: number = 10
  ): Observable<ApiResponsePaginado<Propiedad>> {
    if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100) {
      return throwError(() => new Error('Parámetros de paginación inválidos'));
    }

    return this.api.get<ApiResponsePaginado<Propiedad>>(
      `${this.BASE_PATH}/paginado?pagina=${pagina}&tamanoPagina=${tamanoPagina}`
    ).pipe(
      catchError(this.handleError)
    );
  }

  buscarPropiedades(filtros: FiltrosPropiedades = {}): Observable<ApiResponse<Propiedad[]>> {
    const params = this.construirQueryParams(filtros);
    return this.api.get<ApiResponse<Propiedad[]>>(`${this.BASE_PATH}/buscar${params}`).pipe(
      map(response => {
        console.log('🔍 Búsqueda:', filtros, 'Resultados:', response.data?.length || 0);
        return response;
      }),
      catchError(this.handleError)
    );
  }

  obtenerPropiedadesPorUsuario(idUsuario: number): Observable<ApiResponse<Propiedad[]>> {
    if (idUsuario <= 0) {
      return throwError(() => new Error('ID de usuario inválido'));
    }
    return this.api.get<ApiResponse<Propiedad[]>>(`${this.BASE_PATH}/usuario/${idUsuario}`).pipe(
      catchError(this.handleError)
    );
  }

  obtenerPropiedadesPorRangoPrecio(
    precioMin: number,
    precioMax: number,
    tipoMoneda?: string
  ): Observable<ApiResponse<Propiedad[]>> {
    if (precioMin < 0 || precioMax < 0 || precioMin > precioMax) {
      return throwError(() => new Error('Rango de precios inválido'));
    }

    let params = `?precioMin=${precioMin}&precioMax=${precioMax}`;
    if (tipoMoneda) params += `&tipoMoneda=${tipoMoneda}`;
    return this.api.get<ApiResponse<Propiedad[]>>(`${this.BASE_PATH}/precio${params}`).pipe(
      catchError(this.handleError)
    );
  }

  obtenerEstadisticas(): Observable<ApiResponse<Estadisticas>> {
    return this.api.get<ApiResponse<Estadisticas>>(`${this.BASE_PATH}/estadisticas`).pipe(
      catchError(this.handleError)
    );
  }

  // ✅ MEJORADO: Validación de actualización
  actualizarPropiedad(id: number, dto: ActualizarPropiedadDto): Observable<ApiResponse<any>> {
    if (id <= 0) {
      return throwError(() => new Error('ID inválido'));
    }
    if (!dto.titulo?.trim()) {
      return throwError(() => new Error('El título es requerido'));
    }

    const formData = this.convertirAFormDataActualizar(dto);
    return this.api.put<ApiResponse<any>>(`${this.BASE_PATH}/${id}`, formData).pipe(
      map(response => {
        console.log('✅ Propiedad actualizada:', id);
        return response;
      }),
      catchError(this.handleError)
    );
  }

  // ✅ MEJORADO: Confirmación de eliminación
  eliminarPropiedad(id: number): Observable<ApiResponse<any>> {
    if (id <= 0) {
      return throwError(() => new Error('ID inválido'));
    }
    return this.api.delete<ApiResponse<any>>(`${this.BASE_PATH}/${id}`).pipe(
      map(response => {
        console.log('🗑️ Propiedad eliminada:', id);
        return response;
      }),
      catchError(this.handleError)
    );
  }

  // ============= MÉTODOS AUXILIARES MEJORADOS =============

  private convertirAFormData(dto: CrearPropiedadDto): FormData {
    const formData = new FormData();

    // Campos requeridos
    formData.append('IdUsuario', dto.idUsuario.toString());
    formData.append('IdTipoPropiedad', dto.idTipoPropiedad.toString());
    formData.append('IdEstadoPropiedad', dto.idEstadoPropiedad.toString());
    formData.append('Titulo', dto.titulo.trim());
    formData.append('Direccion', dto.direccion.trim());
    formData.append('Precio', dto.precio.toString());
    formData.append('TipoMoneda', dto.tipoMoneda);

    // Campos opcionales (solo si tienen valor)
    if (dto.descripcion?.trim()) {
      formData.append('Descripcion', dto.descripcion.trim());
    }
    if (dto.areaTerreno && dto.areaTerreno > 0) {
      formData.append('AreaTerreno', dto.areaTerreno.toString());
    }
    if (dto.habitacion && dto.habitacion > 0) {
      formData.append('Habitacion', dto.habitacion.toString());
    }
    if (dto.bano && dto.bano > 0) {
      formData.append('Bano', dto.bano.toString());
    }
    if (dto.estacionamiento && dto.estacionamiento > 0) {
      formData.append('Estacionamiento', dto.estacionamiento.toString());
    }
    if (dto.fotoPropiedad) {
      // Validar tamaño de archivo (máx 5MB)
      if (dto.fotoPropiedad.size > 5 * 1024 * 1024) {
        console.warn('⚠️ Advertencia: La foto excede los 5MB');
      }
      formData.append('FotoPropiedad', dto.fotoPropiedad, dto.fotoPropiedad.name);
    }

    return formData;
  }

  private convertirAFormDataActualizar(dto: ActualizarPropiedadDto): FormData {
    const formData = new FormData();

    // ✅ AGREGADO: Incluir IdUsuario si se cambió el agente
    if (dto.idUsuario && dto.idUsuario > 0) {
      formData.append('IdUsuario', dto.idUsuario.toString());
    }

    formData.append('IdTipoPropiedad', dto.idTipoPropiedad.toString());
    formData.append('IdEstadoPropiedad', dto.idEstadoPropiedad.toString());
    formData.append('Titulo', dto.titulo.trim());
    formData.append('Direccion', dto.direccion.trim());
    formData.append('Precio', dto.precio.toString());
    formData.append('TipoMoneda', dto.tipoMoneda);

    if (dto.descripcion?.trim()) {
      formData.append('Descripcion', dto.descripcion.trim());
    }
    if (dto.areaTerreno && dto.areaTerreno > 0) {
      formData.append('AreaTerreno', dto.areaTerreno.toString());
    }
    if (dto.habitacion && dto.habitacion > 0) {
      formData.append('Habitacion', dto.habitacion.toString());
    }
    if (dto.bano && dto.bano > 0) {
      formData.append('Bano', dto.bano.toString());
    }
    if (dto.estacionamiento && dto.estacionamiento > 0) {
      formData.append('Estacionamiento', dto.estacionamiento.toString());
    }
    if (dto.fotoPropiedad) {
      if (dto.fotoPropiedad.size > 5 * 1024 * 1024) {
        console.warn('⚠️ Advertencia: La foto excede los 5MB');
      }
      formData.append('FotoPropiedad', dto.fotoPropiedad, dto.fotoPropiedad.name);
    }

    return formData;
  }


  private construirQueryParams(filtros: FiltrosPropiedades): string {
    const params = new URLSearchParams();

    if (filtros.termino?.trim()) {
      params.append('termino', filtros.termino.trim());
    }
    if (filtros.idTipoPropiedad && filtros.idTipoPropiedad > 0) {
      params.append('idTipoPropiedad', filtros.idTipoPropiedad.toString());
    }
    if (filtros.idEstadoPropiedad && filtros.idEstadoPropiedad > 0) {
      params.append('idEstadoPropiedad', filtros.idEstadoPropiedad.toString());
    }
    if (filtros.precioMin && filtros.precioMin >= 0) {
      params.append('precioMin', filtros.precioMin.toString());
    }
    if (filtros.precioMax && filtros.precioMax >= 0) {
      params.append('precioMax', filtros.precioMax.toString());
    }
    if (filtros.habitacionesMin && filtros.habitacionesMin > 0) {
      params.append('habitacionesMin', filtros.habitacionesMin.toString());
    }
    if (filtros.tipoMoneda?.trim()) {
      params.append('tipoMoneda', filtros.tipoMoneda.trim());
    }
    if (filtros.idUsuario && filtros.idUsuario > 0) {
      params.append('idUsuario', filtros.idUsuario.toString());
    }

    const queryString = params.toString();
    return queryString ? `?${queryString}` : '';
  }

  // ✅ NUEVO: Manejo centralizado de errores
  private handleError(error: any): Observable<never> {
    console.error('❌ Error en PropertiesService:', error);

    let errorMessage = 'Error desconocido';

    if (error.error instanceof ErrorEvent) {
      // Error del cliente
      errorMessage = `Error del cliente: ${error.error.message}`;
    } else {
      // Error del servidor
      errorMessage = error.error?.mensaje ||
        error.message ||
        `Error del servidor: ${error.status}`;
    }

    return throwError(() => new Error(errorMessage));
  }
}
