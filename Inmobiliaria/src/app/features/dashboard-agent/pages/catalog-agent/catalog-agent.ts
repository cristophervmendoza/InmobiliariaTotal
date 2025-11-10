import { Component, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';

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
  selector: 'app-catalog-agent',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DecimalPipe],
  templateUrl: './catalog-agent.html',
  styleUrls: ['./catalog-agent.css']
})
export class CatalogAgent implements OnInit {
  properties: Property[] = [];
  filteredProperties: Property[] = [];
  loading = true;
  searchText = '';
  selectedType = '';
  selectedEstado = '';

  constructor(private router: Router) { }

  ngOnInit(): void {
    this.loadProperties();
  }

  loadProperties() {
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
        title: 'Terreno amplio en Pachacamac',
        location: 'Pachacamac, Lima',
        price: 190000,
        currency: 'PEN',
        bedrooms: 0,
        bathrooms: 0,
        area: 500,
        images: ['https://via.placeholder.com/400x300'],
        type: 'Terreno',
        status: 'Disponible'
      }
    ];

    this.properties = mock;
    this.filteredProperties = mock;
    this.loading = false;
  }

  applyFilters() {
    let filtered = [...this.properties];

    if (this.searchText) {
      filtered = filtered.filter(
        (p) =>
          p.title.toLowerCase().includes(this.searchText.toLowerCase()) ||
          p.location.toLowerCase().includes(this.searchText.toLowerCase())
      );
    }

    if (this.selectedType) {
      filtered = filtered.filter((p) => p.type === this.selectedType);
    }

    if (this.selectedEstado) {
      filtered = filtered.filter((p) => p.status === this.selectedEstado);
    }

    this.filteredProperties = filtered;
  }

  handleViewDetails(id: number) {
    this.router.navigate(['/agent/properties', id]);
  }
}
