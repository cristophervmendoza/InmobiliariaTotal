import { Component } from '@angular/core';
import { Router } from '@angular/router';

interface Property {
  id: number;
  title: string;
  location: string;
  price: string;
  bedrooms: number;
  bathrooms: number;
  area: string;
  imageUrl: string;
  featured: boolean;
}

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home {
  constructor(private router: Router) { }

  // Estado del buscador
  searchLocation = '';
  searchType = '';
  searchCurrency: 'soles' | 'dolares' = 'soles';
  searchPriceRange = '';

  // Datos de ejemplo
  featuredProperties: Property[] = [
    { id: 1, title: 'Casa Moderna en San Isidro', location: 'San Isidro, Lima', price: 'S/ 850,000', bedrooms: 4, bathrooms: 3, area: '250 m²', imageUrl: 'https://pics.nuroa.com/moderna_casa_en_la_molina_de_700_m2_con_5_habit_con_ba%C3%B1o_3_estac_880_000_2880001757578372093.jpg', featured: true },
    { id: 2, title: 'Departamento Vista al Mar', location: 'Miraflores, Lima', price: 'S/ 680,000', bedrooms: 3, bathrooms: 2, area: '180 m²', imageUrl: 'https://img10.naventcdn.com/avisos/111/01/45/46/50/54/360x266/1511431791.jpg?isFirstImage=true', featured: true },
    { id: 3, title: 'Casa Campestre Exclusiva', location: 'La Molina, Lima', price: 'S/ 1,200,000', bedrooms: 5, bathrooms: 4, area: '320 m²', imageUrl: 'https://img10.naventcdn.com/avisos/11/01/42/43/91/16/360x266/1393254687.jpg?isFirstImage=true', featured: true }
  ];

  stats = [
    { icon: 'building-2', value: '1000+', label: 'Propiedades Disponibles' },
    { icon: 'users', value: '500+', label: 'Clientes Satisfechos' },
    { icon: 'award', value: '10+', label: 'Años de Experiencia' },
    { icon: 'trending-up', value: '95%', label: 'Tasa de Satisfacción' }
  ];

  benefits = [
    { icon: 'check-circle', title: 'Asesoría Personalizada', description: 'Expertos dedicados a encontrar tu hogar ideal según tus necesidades.' },
    { icon: 'clock', title: 'Atención 24/7', description: 'Estamos disponibles en todo momento para responder tus consultas.' },
    { icon: 'award', title: 'Propiedades Verificadas', description: 'Todas nuestras propiedades son verificadas y validadas legalmente.' },
    { icon: 'star', title: 'Mejor Valoración', description: 'Calificados con 5 estrellas por nuestros clientes satisfechos.' }
  ];

  testimonials = [
    {
      id: 1,
      name: 'María González',
      role: 'Compradora',
      text: 'Excelente servicio, encontré mi casa ideal en menos de un mes. Muy profesionales.',
      rating: 5,
      avatar: 'MG',
      imageUrl: 'https://i.pinimg.com/originals/c3/f3/4a/c3f34ac8951dbe0a768e5aeea89408a0.jpg',
      propertyImage: 'https://pics.nuroa.com/moderna_casa_en_la_molina_de_700_m2_con_5_habit_con_ba%C3%B1o_3_estac_880_000_2880001757578372093.jpg'
    },
    {
      id: 2,
      name: 'Carlos Ramírez',
      role: 'Vendedor',
      text: 'Vendí mi propiedad rápidamente gracias a su estrategia de marketing efectiva.',
      rating: 5,
      avatar: 'CR',
      imageUrl: 'https://upload.wikimedia.org/wikipedia/commons/8/81/Antonio_traje_brazos_cruzados_peru.jpg',
      propertyImage: 'https://img10.naventcdn.com/avisos/111/01/45/46/50/54/360x266/1511431791.jpg?isFirstImage=true'
    },
    {
      id: 3,
      name: 'Ana Torres',
      role: 'Arrendataria',
      text: 'Proceso muy sencillo y transparente. Me ayudaron en cada paso del camino.',
      rating: 5,
      avatar: 'AT',
      imageUrl: 'https://i.pinimg.com/474x/88/63/9b/88639b52d323aa31fea8217ed673e1ae.jpg',
      propertyImage: 'https://img10.naventcdn.com/avisos/11/01/42/43/91/16/360x266/1393254687.jpg?isFirstImage=true'
    }


  ];


  priceRanges = {
    soles: [
      { value: '', label: 'Rango de precio' },
      { value: '0-300000', label: 'S/ 0 - S/ 300,000' },
      { value: '300000-600000', label: 'S/ 300,000 - S/ 600,000' },
      { value: '600000-1000000', label: 'S/ 600,000 - S/ 1,000,000' },
      { value: '1000000+', label: 'S/ 1,000,000+' }
    ],
    dolares: [
      { value: '', label: 'Rango de precio' },
      { value: '0-100000', label: '$0 - $100,000' },
      { value: '100000-200000', label: '$100,000 - $200,000' },
      { value: '200000-400000', label: '$200,000 - $400,000' },
      { value: '400000+', label: '$400,000+' }
    ]
  };

  handleCurrencyChange(v: 'soles' | 'dolares'): void {
    this.searchCurrency = v;
    this.searchPriceRange = '';
  }

  handleSearch(): void {
    // Conecta con backend aquí
    this.router.navigate(['/properties']);
  }

  goTo(url: string): void {
    this.router.navigate([url]);
  }

  arrayFrom(n: number): number[] {
    return Array.from({ length: n }, (_, i) => i);
  }
}
