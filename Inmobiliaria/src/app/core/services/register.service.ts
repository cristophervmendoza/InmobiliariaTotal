import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';

export interface RegisterResponse {
  exito: boolean;
  mensaje: string;
  idCliente?: number;
  idUsuario?: number;
}

export interface RegisterServerDto {
  Nombre: string;
  Dni: string;        // 8 dígitos
  Telefono: string;   // 9 dígitos peruanos (sin +51)
  Email: string;
  Password: string;
  IdEstadoUsuario: number;
}

@Injectable({ providedIn: 'root' })
export class RegisterService {
  private api = inject(ApiService);

  create(dto: RegisterServerDto) {
    return this.api.post<RegisterResponse>('/api/Cliente', dto);
  }
}
