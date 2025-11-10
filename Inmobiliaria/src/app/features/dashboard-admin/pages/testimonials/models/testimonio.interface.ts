// ============================================
// TIPOS Y ESTADOS
// ============================================

export type EstadoTestimonio = 'pendiente' | 'rechazado' | 'publicado';

// ============================================
// TESTIMONIOS RECIBIDOS (Tab Recepción)
// ============================================

export interface TestimonioRecibido {
  idTestimonio: number;
  idUsuario: number;
  nombreCompleto: string;
  email: string;
  contenido: string;
  valoracion: number; // 1-5
  fecha: string; // ISO format: "2025-11-10T10:30:00"
  estado: EstadoTestimonio;
}

// ============================================
// TESTIMONIOS PUBLICADOS (Tab Publicaciones)
// ============================================

export interface TestimonioPublicado {
  idTestimonio?: number; // Opcional al crear nuevo
  nombre: string;
  ubicacion: string;
  valoracion: number; // 1-5
  comentario: string;
  tipoPropiedad: string; // "Departamento", "Casa", "Terreno", etc.
  tiempo: string; // "28 días", "45 días", etc.
  fotoUrl?: string;
  videoUrl?: string;
  creadoAt?: string; // ISO format
}

// ============================================
// USUARIOS/CLIENTES (Modal Enviar Formulario)
// ============================================

export interface Usuario {
  idUsuario: number;
  nombreCompleto: string;
  email: string;
  telefono?: string;
  rol?: string;
}

// ============================================
// DTOs PARA API BACKEND C#
// ============================================

// Request: Enviar formulario a cliente
export interface EnviarFormularioRequest {
  idUsuario: number;
  emailCliente?: string; // Opcional, backend puede obtenerlo del usuario
}

// Request: Rechazar testimonio
export interface RechazarTestimonioRequest {
  idTestimonio: number;
  motivo?: string;
}

// Request: Publicar testimonio
export interface PublicarTestimonioRequest {
  idTestimonioRecibido?: number; // Si proviene de un testimonio recibido
  nombre: string;
  ubicacion: string;
  valoracion: number;
  comentario: string;
  tipoPropiedad: string;
  tiempo: string;
  fotoUrl?: string;
  videoUrl?: string;
}

// Response: Lista de testimonios
export interface TestimoniosResponse {
  recibidos: TestimonioRecibido[];
  publicados: TestimonioPublicado[];
}

// Response: Operación exitosa
export interface ApiResponse {
  success: boolean;
  message: string;
  data?: any;
}

// ============================================
// FILTROS Y BÚSQUEDA
// ============================================

export interface FiltrosTestimonios {
  estado?: EstadoTestimonio;
  busqueda?: string;
  fechaDesde?: string;
  fechaHasta?: string;
  valoracion?: number;
}

// ============================================
// ESTADÍSTICAS (Opcional)
// ============================================

export interface EstadisticasTestimonios {
  total: number;
  pendientes: number;
  publicados: number;
  rechazados: number;
  valoracionPromedio: number;
}
