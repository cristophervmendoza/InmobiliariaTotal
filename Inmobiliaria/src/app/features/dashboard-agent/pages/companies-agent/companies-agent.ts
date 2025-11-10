import { Component } from '@angular/core';

interface Empresa {
  idEmpresa: number;
  nombre: string;
  ruc: string;
  email: string;
  telefono: string;
  direccion: string;
  tipoEmpresa: string;
  fechaRegistro: string;
  actualizadoAt: string; //ÉL SOLO PUEDE VER ESTE APARTADO
}

@Component({
  selector: 'app-companies-agent',
  templateUrl: './companies-agent.html',
  styleUrls: ['./companies-agent.css'],
  standalone: false
})
export class CompaniesAgent {
  empresas: Empresa[] = [
    {
      idEmpresa: 1,
      nombre: 'Constructora Premier',
      ruc: '102345678901',
      email: 'contacto@premier.com',
      telefono: '987654321',
      direccion: 'Calle 10 #5-50, Lima',
      tipoEmpresa: 'Constructora',
      fechaRegistro: '2024-01-15',
      actualizadoAt: '2024-11-02'
    },
    {
      idEmpresa: 2,
      nombre: 'Desarrollos Inmobiliarios SA',
      ruc: '10568923451',
      email: 'info@desarrollos.com',
      telefono: '926547893',
      direccion: 'Carrera 15 #20-30, Lima',
      tipoEmpresa: 'Desarrolladora',
      fechaRegistro: '2023-06-20',
      actualizadoAt: '2024-10-28'
    },
    {
      idEmpresa: 3,
      nombre: 'Inmobiliaria Central',
      ruc: '11223344556',
      email: 'ventas@central.com',
      telefono: '3105555666',
      direccion: 'Av Carrera 7 #100-50, Lima',
      tipoEmpresa: 'Inmobiliaria',
      fechaRegistro: '2024-03-10',
      actualizadoAt: '2024-11-01'
    }
  ];

  searchTerm = '';
  filterType = 'Todos';
  tiposEmpresa = ['Todos', 'Constructora', 'Desarrolladora', 'Inmobiliaria', 'Otra'];

  modalDetalle = false;
  empresaSeleccionada: Empresa | null = null;

  get empresasFiltradas(): Empresa[] {
    return this.empresas.filter(empresa => {
      const coincideNombre = empresa.nombre.toLowerCase().includes(this.searchTerm.toLowerCase());
      const coincideTipo = this.filterType === 'Todos' || empresa.tipoEmpresa === this.filterType;
      return coincideNombre && coincideTipo;
    });
  }

  handleVerDetalle(empresa: Empresa) {
    this.empresaSeleccionada = empresa;
    this.modalDetalle = true;
  }

  cerrarDetalle() {
    this.modalDetalle = false;
    this.empresaSeleccionada = null;
  }
}
