import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import {
  TestimonioRecibido,
  TestimonioPublicado,
  Usuario,
  EnviarFormularioRequest,
  PublicarTestimonioRequest,
  ApiResponse,
  TestimoniosResponse,
  EstadisticasTestimonios
} from '../models/testimonio.interface';

@Injectable({
  providedIn: 'root'
})
export class TestimonialsService {
  // URL base del backend C# - Ajustar según tu configuración
  private apiUrl = 'https://localhost:5001/api/testimonios'; // Cambiar en producción

  private httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  constructor(private http: HttpClient) { }

  // ============================================
  // OBTENER DATOS
  // ============================================

  /**
   * Obtener todos los testimonios (recibidos y publicados)
   */
  getTodosTestimonios(): Observable<TestimoniosResponse> {
    return this.http.get<TestimoniosResponse>(`${this.apiUrl}`, this.httpOptions)
      .pipe(
        catchError(this.handleError<TestimoniosResponse>('getTodosTestimonios', {
          recibidos: [],
          publicados: []
        }))
      );
  }

  /**
   * Obtener testimonios recibidos (pendientes y rechazados)
   */
  getTestimoniosRecibidos(): Observable<TestimonioRecibido[]> {
    return this.http.get<TestimonioRecibido[]>(`${this.apiUrl}/recibidos`, this.httpOptions)
      .pipe(
        catchError(this.handleError<TestimonioRecibido[]>('getTestimoniosRecibidos', []))
      );
  }

  /**
   * Obtener testimonios publicados
   */
  getTestimoniosPublicados(): Observable<TestimonioPublicado[]> {
    return this.http.get<TestimonioPublicado[]>(`${this.apiUrl}/publicados`, this.httpOptions)
      .pipe(
        catchError(this.handleError<TestimonioPublicado[]>('getTestimoniosPublicados', []))
      );
  }

  /**
   * Obtener un testimonio específico por ID
   */
  getTestimonioPorId(id: number): Observable<TestimonioRecibido> {
    return this.http.get<TestimonioRecibido>(`${this.apiUrl}/${id}`, this.httpOptions)
      .pipe(
        catchError(this.handleError<TestimonioRecibido>('getTestimonioPorId'))
      );
  }

  /**
   * Obtener estadísticas de testimonios
   */
  getEstadisticas(): Observable<EstadisticasTestimonios> {
    return this.http.get<EstadisticasTestimonios>(`${this.apiUrl}/estadisticas`, this.httpOptions)
      .pipe(
        catchError(this.handleError<EstadisticasTestimonios>('getEstadisticas'))
      );
  }

  // ============================================
  // ENVIAR FORMULARIO A CLIENTE
  // ============================================

  /**
   * Enviar formulario de testimonio a un cliente específico
   */
  enviarFormulario(idUsuario: number): Observable<ApiResponse> {
    const body: EnviarFormularioRequest = { idUsuario };

    return this.http.post<ApiResponse>(
      `${this.apiUrl}/enviar-formulario`,
      body,
      this.httpOptions
    ).pipe(
      catchError(this.handleError<ApiResponse>('enviarFormulario'))
    );
  }

  // ============================================
  // GESTIÓN DE TESTIMONIOS RECIBIDOS
  // ============================================

  /**
   * Rechazar un testimonio recibido
   */
  rechazarTestimonio(idTestimonio: number, motivo?: string): Observable<ApiResponse> {
    return this.http.patch<ApiResponse>(
      `${this.apiUrl}/${idTestimonio}/rechazar`,
      { motivo },
      this.httpOptions
    ).pipe(
      catchError(this.handleError<ApiResponse>('rechazarTestimonio'))
    );
  }

  /**
   * Aprobar un testimonio (cambiar estado a aprobado)
   */
  aprobarTestimonio(idTestimonio: number): Observable<ApiResponse> {
    return this.http.patch<ApiResponse>(
      `${this.apiUrl}/${idTestimonio}/aprobar`,
      {},
      this.httpOptions
    ).pipe(
      catchError(this.handleError<ApiResponse>('aprobarTestimonio'))
    );
  }

  // ============================================
  // GESTIÓN DE TESTIMONIOS PUBLICADOS
  // ============================================

  /**
   * Publicar un nuevo testimonio
   */
  publicarTestimonio(datos: PublicarTestimonioRequest): Observable<TestimonioPublicado> {
    return this.http.post<TestimonioPublicado>(
      `${this.apiUrl}/publicar`,
      datos,
      this.httpOptions
    ).pipe(
      catchError(this.handleError<TestimonioPublicado>('publicarTestimonio'))
    );
  }

  /**
   * Editar un testimonio publicado
   */
  editarTestimonio(idTestimonio: number, datos: Partial<TestimonioPublicado>): Observable<TestimonioPublicado> {
    return this.http.put<TestimonioPublicado>(
      `${this.apiUrl}/publicados/${idTestimonio}`,
      datos,
      this.httpOptions
    ).pipe(
      catchError(this.handleError<TestimonioPublicado>('editarTestimonio'))
    );
  }

  /**
   * Eliminar un testimonio publicado
   */
  eliminarTestimonio(idTestimonio: number): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(
      `${this.apiUrl}/publicados/${idTestimonio}`,
      this.httpOptions
    ).pipe(
      catchError(this.handleError<ApiResponse>('eliminarTestimonio'))
    );
  }

  // ============================================
  // OBTENER USUARIOS/CLIENTES
  // ============================================

  /**
   * Obtener lista de usuarios (para modal enviar formulario)
   */
  getUsuarios(): Observable<Usuario[]> {
    // Ajustar URL según tu estructura de API
    return this.http.get<Usuario[]>(`https://localhost:5001/api/usuarios?rol=cliente`, this.httpOptions)
      .pipe(
        catchError(this.handleError<Usuario[]>('getUsuarios', []))
      );
  }

  // ============================================
  // MANEJO DE ERRORES
  // ============================================

  /**
   * Manejador genérico de errores HTTP
   */
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(`${operation} falló:`, error);

      // Log del error en el servidor (opcional)
      this.log(`${operation} error: ${error.message}`);

      // Retornar resultado vacío para que la app continúe
      return of(result as T);
    };
  }

  /**
   * Logging simple (puedes conectarlo a un servicio de logging real)
   */
  private log(message: string): void {
    console.log(`TestimonialsService: ${message}`);
  }

  // ============================================
  // MÉTODOS DE UTILIDAD
  // ============================================

  /**
   * Subir archivo (foto/video) al servidor
   */
  subirArchivo(file: File, tipo: 'foto' | 'video'): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('tipo', tipo);

    // Sin Content-Type header para permitir multipart/form-data
    return this.http.post<{ url: string }>(
      `${this.apiUrl}/subir-archivo`,
      formData
    ).pipe(
      catchError(this.handleError<{ url: string }>('subirArchivo'))
    );
  }
}
