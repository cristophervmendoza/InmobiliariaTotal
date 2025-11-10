import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

interface Property {
  id: number;
  title: string;
  location: string;
  price: number;
  currency: string;
  bedrooms: number;
  bathrooms: number;
  area: number;
  images: string[];
  type: string;
  status: string;
}

@Component({
  selector: 'app-mis-propiedades',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './mis-propiedades.component.html',
  styleUrls: ['./mis-propiedades.component.css']
})
export class MisPropiedadesComponent implements OnInit {
  properties: Property[] = [];
  filteredProperties: Property[] = [];
  loading = true;
  searchText = '';
  selectedType = '';
  selectedEstado = '';

  ngOnInit(): void {
    this.loadProperties();
  }

  loadProperties(): void {
    const mock: Property[] = [
      {
        id: 1,
        title: 'Casa moderna en Barranco',
        location: 'Av. Grau 234, Barranco',
        price: 450000,
        currency: 'PEN',
        bedrooms: 3,
        bathrooms: 2,
        area: 180,
        images: ['https://via.placeholder.com/400x300'],
        type: 'Casa',
        status: 'Disponible'
      },
      {
        id: 2,
        title: 'Departamento céntrico',
        location: 'Jr. Carabaya 567, Lima Cercado',
        price: 280000,
        currency: 'PEN',
        bedrooms: 2,
        bathrooms: 1,
        area: 85,
        images: ['https://via.placeholder.com/400x300'],
        type: 'Departamento',
        status: 'Alquilado'
      },
      {
        id: 3,
        title: 'Terreno amplio en Chaclacayo',
        location: 'Av. Los Cedros 1200, Chaclacayo',
        price: 350000,
        currency: 'PEN',
        bedrooms: 0,
        bathrooms: 0,
        area: 450,
        images: ['https://via.placeholder.com/400x300'],
        type: 'Terreno',
        status: 'Vendido'
      }
    ];

    this.properties = mock;
    this.filteredProperties = mock;
    this.loading = false;
  }

  applyFilters(): void {
    let filtered = [...this.properties];

    if (this.searchText) {
      filtered = filtered.filter(
        p =>
          p.title.toLowerCase().includes(this.searchText.toLowerCase()) ||
          p.location.toLowerCase().includes(this.searchText.toLowerCase())
      );
    }

    if (this.selectedType) {
      filtered = filtered.filter(p => p.type === this.selectedType);
    }

    if (this.selectedEstado) {
      filtered = filtered.filter(p => p.status === this.selectedEstado);
    }

    this.filteredProperties = filtered;
  }

  get totalCount(): number {
    return this.properties.length;
  }

  get disponiblesCount(): number {
    return this.properties.filter(p => p.status === 'Disponible').length;
  }

  get alquiladasCount(): number {
    return this.properties.filter(p => p.status === 'Alquilado').length;
  }

  get vendidasCount(): number {
    return this.properties.filter(p => p.status === 'Vendido').length;
  }
  handleViewDetails(id: number): void {

    console.log('Ver detalles de propiedad:', id);

  }

}
