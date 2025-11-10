import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface Property {
  id: number;
  title: string;
  location: string;
  price: number;
  currency: string;
  bedrooms: number;
  bathrooms: number;
  area: number;
  landArea?: number;
  parking?: number;
  description: string;
  images: string[];
  type: string;
  status: string;
  yearBuilt: number;
  features: string[];
  amenities: string[]; // 👈 ahora es un arreglo de strings (para mostrar con *ngFor)
  agent: {
    name: string;
    phone: string;
    email: string;
  };
}

@Component({
  selector: 'app-detalle-propiedad-agente',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './detalle-propiedad-agente.component.html',
  styleUrls: ['./detalle-propiedad-agente.component.css']
})
export class DetallePropiedadAgenteComponent implements OnInit {
  property: Property | null = null;
  loading = true;

  // Calculadora
  downPayment = '';
  interestRate = '8';
  loanTerm = '20';
  monthlyPayment = 0;

  constructor(private route: ActivatedRoute, private router: Router) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      this.loadProperty(id);
    });
  }



  loadProperty(id: number) {
    const mockProperty: Property = {
      id,
      title: 'Casa moderna en Barranco',
      location: 'Av. Grau 234, Barranco',
      price: 450000,
      currency: 'USD',
      bedrooms: 3,
      bathrooms: 2,
      area: 180,
      landArea: 200,
      parking: 2,
      description:
        'Hermosa casa moderna ubicada en el corazón de Barranco. Cuenta con amplios espacios, iluminación natural y acabados de primera calidad. Ideal para familias que buscan confort y estilo en una zona privilegiada.',
      images: ['https://via.placeholder.com/800x500'],
      type: 'Casa',
      status: 'Disponible',
      yearBuilt: 2020,
      features: [
        'Cocina equipada',
        'Closets empotrados',
        'Pisos de porcelanato',
        'Ventanas amplias'
      ],
      amenities: [
        'Amoblado',
        'Piscina',
        'Jardín',
        'Balcón',
        'Seguridad 24/7',
        'Gimnasio'
      ],
      agent: {
        name: 'Juan Pérez',
        phone: '+51 987 654 321',
        email: 'juan.perez@inmobiliaria.com'
      }
    };

    this.property = mockProperty;
    this.loading = false;
  }

  /** Calculadora de hipoteca */
  calculateMortgage() {
    if (!this.property) return;

    const principal = this.property.price - parseFloat(this.downPayment || '0');
    const monthlyRate = parseFloat(this.interestRate) / 100 / 12;
    const numPayments = parseInt(this.loanTerm) * 12;

    if (principal <= 0 || monthlyRate <= 0 || numPayments <= 0) {
      this.monthlyPayment = 0;
      return;
    }

    const payment =
      (principal * monthlyRate * Math.pow(1 + monthlyRate, numPayments)) /
      (Math.pow(1 + monthlyRate, numPayments) - 1);

    this.monthlyPayment = payment;
  }

  goBack() {
    this.router.navigate(['/agent/catalog']);
  }
}
