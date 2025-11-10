export interface LoginDto {
  email: string;
  password: string;
}

export interface LoginUser {
  idUsuario: number;
  nombre: string;
  email: string;
  idEstadoUsuario?: number;
  nombreCorto?: string;
  iniciales?: string;
  rol: 'admin' | 'agent' | 'client' | 'user';
}

export interface LoginResponse {
  exito: boolean;
  mensaje: string;
  usuario?: LoginUser;
}
