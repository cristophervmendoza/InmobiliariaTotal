import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  standalone: false,
  templateUrl: './footer.html',
  styleUrls: ['./footer.css']
})
export class Footer {
  financialEntities: string[] = [
    'Banco de Crédito',
    'Interbank',
    'BBVA',
    'Scotiabank',
    'MiBanco'
  ];

  menuItems = [
    { name: 'Inicio', path: '/home' },
    { name: 'Se un asesor', path: '/beasesor' },
    { name: 'Testimonios', path: '/testimonies' },
    { name: 'Ofrecer tu Inmueble', path: '/offers' },
    { name: 'Propiedades', path: '/properties' }
  ];
}
