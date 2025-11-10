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
  amenities: string[];
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
  downPayment: number = 90000;
  interestRate: number = 5;
  loanTerm: number = 20;
  monthlyPayment: number = 0;
  showAgentInfo: boolean = true; // 👈 Nueva variable para controlar si mostrar info del agente

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    // 👇 Detectar si viene desde "properties" (mis propiedades) o "catalog" (catálogo general)
    const currentUrl = this.router.url;
    this.showAgentInfo = currentUrl.includes('/catalog'); // Solo mostrar si viene del catálogo

    this.loadProperty(id);
    this.calculateMortgage();
  }

  loadProperty(id: number): void {
    const mockProperties: Property[] = [
      {
        id: 1,
        title: 'Casa Moderna en Miraflores',
        location: 'Miraflores, Lima',
        price: 450000,
        currency: 'USD',
        bedrooms: 4,
        bathrooms: 3,
        area: 220,
        landArea: 180,
        parking: 2,
        description: 'Hermosa casa moderna con acabados de primera calidad...',
        images: ['img1.jpg', 'img2.jpg'],
        type: 'Casa',
        status: 'Disponible',
        yearBuilt: 2020,
        features: ['Aire acondicionado', 'Cocina equipada', 'Closets empotrados'],
        amenities: ['Piscina', 'Jardín', 'Seguridad 24/7'],
        agent: {
          name: 'Carlos Rodríguez',
          phone: '+51 987 654 321',
          email: 'carlos@inmobiliaria.com'
        }
      },
      {
        id: 2,
        title: 'Departamento en San Isidro',
        location: 'San Isidro, Lima',
        price: 320000,
        currency: 'USD',
        bedrooms: 3,
        bathrooms: 2,
        area: 150,
        parking: 2,
        description: 'Exclusivo departamento en zona residencial...',
        images: ['img3.jpg', 'img4.jpg'],
        type: 'Departamento',
        status: 'Disponible',
        yearBuilt: 2019,
        features: ['Vista panorámica', 'Balcón amplio', 'Pisos de porcelanato'],
        amenities: ['Gimnasio', 'Área de juegos', 'Salón de eventos'],
        agent: {
          name: 'María González',
          phone: '+51 912 345 678',
          email: 'maria@inmobiliaria.com'
        }
      }
    ];

    this.property = mockProperties.find(p => p.id === id) || null;
  }

  calculateMortgage(): void {
    if (!this.property) return;
    const principal = this.property.price - this.downPayment;
    const monthlyRate = this.interestRate / 100 / 12;
    const numPayments = this.loanTerm * 12;
    if (monthlyRate === 0) {
      this.monthlyPayment = principal / numPayments;
    } else {
      this.monthlyPayment = principal * (monthlyRate * Math.pow(1 + monthlyRate, numPayments)) / (Math.pow(1 + monthlyRate, numPayments) - 1);
    }
  }

  contactAgent(): void {
    if (this.property?.agent) {
      const message = `Hola ${this.property.agent.name}, estoy interesado en ${this.property.title}`;
      const phone = this.property.agent.phone.replace(/\s/g, '');
      window.open(`https://wa.me/${phone}?text=${encodeURIComponent(message)}`, '_blank');
    }
  }

  goBack(): void {
    this.router.navigate(['/agent/properties']);
  }
}
