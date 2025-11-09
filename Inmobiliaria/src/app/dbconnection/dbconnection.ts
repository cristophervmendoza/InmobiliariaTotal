import { Component, inject } from '@angular/core';
import { ApiService } from '../core/services/api.service';
import { HttpErrorResponse } from '@angular/common/http';


@Component({
  selector: 'app-dbconnection',
  standalone: false,
  templateUrl: './dbconnection.html',
  styleUrl: './dbconnection.css',
})
export class Dbconnection {
  private api = inject(ApiService);

  status: 'idle' | 'loading' | 'ok' | 'error' = 'idle';
  message = '';

  // Ajusta el endpoint a algo que exponga tu API, por ejemplo /weatherforecast o /api/health
  testEndpoint = '/api/Database/test-connection';

  probarConexion() {
    this.status = 'loading';
    this.message = 'Probando conexión...';

    this.api.get<any>(this.testEndpoint).subscribe({
      next: (res) => {
        this.status = 'ok';
        this.message = `Conexión OK. Respuesta: ${JSON.stringify(res)}`;
      },
      error: (err: HttpErrorResponse) => {
        this.status = 'error';
        this.message = `Error: ${err.status} ${err.statusText} - ${err.message}`;
      }
    });
  }
}
