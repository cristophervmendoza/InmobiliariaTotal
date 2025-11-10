export interface RegisterDto {
  nombre: string;
  dni: string;
  telefono: string;
  email: string;
  password: string;
  idEstadoUsuario: number; // según tu backend
}

export interface RegisterResponse {
  exito: boolean;
  mensaje: string;
  idCliente?: number;
  idUsuario?: number;
}
