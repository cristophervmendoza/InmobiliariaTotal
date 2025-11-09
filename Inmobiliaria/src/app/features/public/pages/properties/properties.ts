import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

type Property = {
  id: number;
  title: string;
  location: string;
  price: number;
  currency: 'PEN' | 'USD';
  bedrooms: number;
  bathrooms: number;
  area: number;
  imageUrl: string;
  type: 'casa' | 'departamento' | 'oficina' | 'terreno' | string;
  status: string;
  featured: boolean;
};

type ViewMode = 'grid' | 'list';

@Component({
  selector: 'app-properties',
  standalone : false,
  templateUrl: './properties.html',
  styleUrl: './properties.css'
})
export class Properties implements OnInit {
  // Estado UI
  isLoading = true;
  showFilters = false;
  viewMode: ViewMode = 'grid';
  sortBy: 'recent' | 'price-low' | 'price-high' = 'recent';

  // Filtros
  searchText = '';
  selectedType = '';
  selectedCurrency: 'PEN' | 'USD' = 'PEN';
  minPrice = '';
  maxPrice = '';
  selectedBedrooms = '';

  // Datos
  properties: Property[] = [];
  filteredProperties: Property[] = [];

  // Favoritos simples
  favoritos: number[] = [];

  constructor(private router: Router) { }

  ngOnInit(): void {
    this.fetchProperties();
  }

  async fetchProperties() {
    this.isLoading = true;
    try {
      await new Promise((r) => setTimeout(r, 500));
      const mock: Property[] = [
        { id: 1, title: 'Casa Moderna en San Isidro', location: 'San Isidro, Lima', price: 850000, currency: 'PEN', bedrooms: 4, bathrooms: 3, area: 250, imageUrl: 'https://via.placeholder.com/400x300/06b6d4/ffffff?text=Casa+1', type: 'casa', status: 'Disponible', featured: true },
        { id: 2, title: 'Departamento Vista al Mar', location: 'Miraflores, Lima', price: 680000, currency: 'PEN', bedrooms: 3, bathrooms: 2, area: 180, imageUrl: 'https://via.placeholder.com/400x300/0891b2/ffffff?text=Depto+1', type: 'departamento', status: 'Disponible', featured: true },
        { id: 3, title: 'Casa Campestre Exclusiva', location: 'La Molina, Lima', price: 1200000, currency: 'PEN', bedrooms: 5, bathrooms: 4, area: 320, imageUrl: 'https://via.placeholder.com/400x300/06b6d4/ffffff?text=Casa+2', type: 'casa', status: 'Disponible', featured: true },
        { id: 4, title: 'Departamento en Surco', location: 'Santiago de Surco, Lima', price: 450000, currency: 'PEN', bedrooms: 2, bathrooms: 2, area: 120, imageUrl: 'https://via.placeholder.com/400x300/0891b2/ffffff?text=Depto+2', type: 'departamento', status: 'Disponible', featured: false },
        { id: 5, title: 'Casa en Los Olivos', location: 'Los Olivos, Lima', price: 380000, currency: 'PEN', bedrooms: 3, bathrooms: 2, area: 150, imageUrl: 'https://via.placeholder.com/400x300/06b6d4/ffffff?text=Casa+3', type: 'casa', status: 'Disponible', featured: false },
        { id: 6, title: 'Oficina en San Borja', location: 'San Borja, Lima', price: 320000, currency: 'PEN', bedrooms: 0, bathrooms: 2, area: 80, imageUrl: 'https://via.placeholder.com/400x300/0891b2/ffffff?text=Oficina+1', type: 'oficina', status: 'Disponible', featured: false }
      ];
      this.properties = mock;
      this.applyFilters();
    } finally {
      this.isLoading = false;
    }
  }

  applyFilters() {
    let filtered = [...this.properties];

    if (this.searchText) {
      const q = this.searchText.toLowerCase();
      filtered = filtered.filter(p => p.title.toLowerCase().includes(q) || p.location.toLowerCase().includes(q));
    }
    if (this.selectedType) filtered = filtered.filter(p => p.type === this.selectedType);
    if (this.minPrice) filtered = filtered.filter(p => p.price >= Number(this.minPrice));
    if (this.maxPrice) filtered = filtered.filter(p => p.price <= Number(this.maxPrice));
    if (this.selectedBedrooms) filtered = filtered.filter(p => p.bedrooms >= Number(this.selectedBedrooms));

    if (this.sortBy === 'price-low') filtered.sort((a, b) => a.price - b.price);
    else if (this.sortBy === 'price-high') filtered.sort((a, b) => b.price - a.price);
    else filtered.sort((a, b) => b.id - a.id);

    this.filteredProperties = filtered;
  }

  formatPrice(price: number, currency: 'PEN' | 'USD') {
    const symbol = currency === 'PEN' ? 'S/' : '$';
    return `${symbol} ${price.toLocaleString('es-PE')}`;
  }

  toggleFavorito(e: Event, id: number) {
    e.stopPropagation();
    const i = this.favoritos.indexOf(id);
    if (i >= 0) this.favoritos.splice(i, 1);
    else this.favoritos.push(id);
  }

  onCardClick(id: number) {
    this.router.navigate(['/properties', id]);
  }
}
