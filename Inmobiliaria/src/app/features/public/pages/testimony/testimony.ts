import { Component } from '@angular/core';

type TestimonyItem = {
  agentName: string;
  clientNameMasked: string; // p.ej. "Luis G."
  propertyType: 'casa' | 'departamento' | 'terreno' | 'otro';
  location: string;
  saleDate: string; // ISO: yyyy-mm-dd
  salePrice?: number;
  summary: string;
  highlights: string[];
  imageUrl: string;
};
@Component({
  selector: 'app-testimony',
  standalone: false,
  templateUrl: './testimony.html',
  styleUrl: './testimony.css',
})
export class Testimony {
  testimonies: TestimonyItem[] = [
    {
      agentName: 'María Pérez',
      clientNameMasked: 'Luis G.',
      propertyType: 'casa',
      location: 'San Isidro, Lima',
      saleDate: '2025-10-12',
      salePrice: 520000,
      summary: 'Logramos cerrar en 21 días gracias a una estrategia de precios precisa y visitas segmentadas.',
      highlights: ['Cierre en 21 días', '+15 visitas calificadas', 'Negociación efectiva'],
      imageUrl: 'assets/testimonios/cierre-1.jpg'
    },
    {
      agentName: 'Carlos Gómez',
      clientNameMasked: 'Ana R.',
      propertyType: 'departamento',
      location: 'Miraflores, Lima',
      saleDate: '2025-09-28',
      salePrice: 310000,

      summary: 'La clienta buscaba vivir cerca al malecón; coordinamos visitas al atardecer para resaltar el entorno.',
      highlights: ['Fotografía profesional', 'Tour virtual', 'Oferta sobre precio'],
      imageUrl: 'assets/testimonios/cierre-2.jpg'
    },
    {
      agentName: 'Sofía Torres',
      clientNameMasked: 'Jorge S.',
      propertyType: 'terreno',
      location: 'La Molina, Lima',
      saleDate: '2025-08-05',
      salePrice: 450000,
      summary: 'Se evidenció potencial de ampliación y se cerró con una oferta competitiva en menos de un mes.',
      highlights: ['Análisis de zonificación', 'Propuesta de valorización', 'Cierre en 27 días'],
      imageUrl: 'assets/testimonios/cierre-3.jpg'
    }
  ];
}
