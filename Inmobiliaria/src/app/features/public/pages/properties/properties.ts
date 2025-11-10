import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { PropertiesService, Propiedad } from '../../../../core/services/properties.service';

type Moneda = 'PEN' | 'USD' | 'EUR';
type ViewMode = 'grid' | 'list';

interface PropertyVista {
  id: number;
  title: string;
  location: string;
  price: number;
  currency: Moneda;
  bedrooms: number;
  bathrooms: number;
  area: number;
  imageUrl: string;
  type: string;
  status: string;
  featured: boolean;
}

@Component({
  selector: 'app-properties',
  standalone: false,
  templateUrl: './properties.html',
  styleUrl: './properties.css'
})
export class Properties implements OnInit {
  private propertiesService = inject(PropertiesService);
  private router = inject(Router);

  // Estado UI
  isLoading = true;
  showFilters = false;
  viewMode: ViewMode = 'grid';
  sortBy: 'recent' | 'price-low' | 'price-high' = 'recent';

  // Filtros - ✅ CORREGIDO: type string para evitar error de TypeScript
  searchText = '';
  selectedType = '';
  selectedCurrency: string = '';  // ✅ string genérico (puede ser '', 'PEN', 'USD', 'EUR')
  minPrice = '';
  maxPrice = '';
  selectedBedrooms = '';

  // Datos
  properties: PropertyVista[] = [];
  filteredProperties: PropertyVista[] = [];

  // Favoritos
  favoritos: number[] = [];

  // Mapeo de tipos
  private tiposPropiedad: Record<number, string> = {
    1: 'casa',
    2: 'departamento',
    3: 'terreno',
    4: 'oficina'
  };

  private estadosPropiedad: Record<number, string> = {
    1: 'Disponible',
    2: 'Reservada',
    3: 'Vendida',
    4: 'Pausada',
    5: 'Cerrada'
  };

  ngOnInit(): void {
    this.cargarFavoritos();
    this.fetchProperties();
  }

  // ✅ Cargar propiedades desde el backend
  fetchProperties(): void {
    console.log('🔄 Iniciando carga de propiedades...');
    this.isLoading = true;

    this.propertiesService.listarPropiedades().subscribe({
      next: (response) => {
        console.log('📦 Respuesta backend:', response);

        if (response.exito && response.data) {
          // ✅ Mapear todas las propiedades activas
          this.properties = response.data
            .filter(prop => prop.idEstadoPropiedad === 1) // Solo activas
            .map(prop => this.mapearPropiedad(prop));

          console.log('✅ Total propiedades cargadas:', this.properties.length);
          console.log('📄 Propiedades:', this.properties);

          this.applyFilters();
        } else {
          console.warn('⚠️ No se obtuvieron propiedades');
          this.properties = [];
          this.filteredProperties = [];
        }

        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Error al cargar propiedades:', error);
        this.isLoading = false;
        this.properties = [];
        this.filteredProperties = [];
      }
    });
  }

  // ✅ Mapear de backend a vista
  private mapearPropiedad(prop: Propiedad): PropertyVista {
    const mapped = {
      id: prop.idPropiedad,
      title: prop.titulo,
      location: prop.direccion,
      price: prop.precio,
      currency: prop.tipoMoneda as Moneda,
      bedrooms: prop.habitacion || 0,
      bathrooms: prop.bano || 0,
      area: prop.areaTerreno || 0,
      imageUrl: prop.fotoPropiedad
        ? `http://localhost:5000/uploads/propiedades/${prop.fotoPropiedad}`
        : 'https://via.placeholder.com/400x300/06b6d4/ffffff?text=Sin+Imagen',
      type: this.tiposPropiedad[prop.idTipoPropiedad] || 'otro',
      status: this.estadosPropiedad[prop.idEstadoPropiedad] || 'Disponible',
      featured: false
    };

    console.log(`🏠 Propiedad mapeada:`, {
      id: mapped.id,
      title: mapped.title,
      currency: mapped.currency,
      type: mapped.type
    });

    return mapped;
  }

  // ✅ Aplicar filtros - CORREGIDO
  applyFilters(): void {
    console.log('🔍 Aplicando filtros...');
    console.log('📊 Total propiedades:', this.properties.length);

    let filtered = [...this.properties];

    // Filtro por búsqueda de texto
    if (this.searchText.trim()) {
      const q = this.searchText.toLowerCase().trim();
      filtered = filtered.filter(p =>
        p.title.toLowerCase().includes(q) ||
        p.location.toLowerCase().includes(q)
      );
      console.log(`🔍 Después de filtro texto (${q}):`, filtered.length);
    }

    // Filtro por tipo
    if (this.selectedType) {
      filtered = filtered.filter(p => p.type === this.selectedType);
      console.log(`🔍 Después de filtro tipo (${this.selectedType}):`, filtered.length);
    }

    // ✅ Filtro por moneda - CORREGIDO sin error de TypeScript
    if (this.selectedCurrency) {
      filtered = filtered.filter(p => p.currency === this.selectedCurrency);
      console.log(`🔍 Después de filtro moneda (${this.selectedCurrency}):`, filtered.length);
    }

    // Filtro por rango de precio
    if (this.minPrice) {
      const min = Number(this.minPrice);
      filtered = filtered.filter(p => p.price >= min);
      console.log(`🔍 Después de filtro precio mín (${min}):`, filtered.length);
    }

    if (this.maxPrice) {
      const max = Number(this.maxPrice);
      filtered = filtered.filter(p => p.price <= max);
      console.log(`🔍 Después de filtro precio máx (${max}):`, filtered.length);
    }

    // Filtro por habitaciones
    if (this.selectedBedrooms) {
      const beds = Number(this.selectedBedrooms);
      filtered = filtered.filter(p => p.bedrooms >= beds);
      console.log(`🔍 Después de filtro habitaciones (${beds}+):`, filtered.length);
    }

    // Ordenamiento
    if (this.sortBy === 'price-low') {
      filtered.sort((a, b) => a.price - b.price);
      console.log('🔢 Ordenado: precio menor a mayor');
    } else if (this.sortBy === 'price-high') {
      filtered.sort((a, b) => b.price - a.price);
      console.log('🔢 Ordenado: precio mayor a menor');
    } else {
      filtered.sort((a, b) => b.id - a.id);
      console.log('🔢 Ordenado: más recientes');
    }

    this.filteredProperties = filtered;
    console.log('✅ RESULTADO FINAL:', this.filteredProperties.length, 'propiedades');
  }

  // ✅ Formatear precio
  formatPrice(price: number, currency: Moneda): string {
    let symbol = 'S/';
    if (currency === 'USD') symbol = '$';
    else if (currency === 'EUR') symbol = '€';

    return `${symbol} ${price.toLocaleString('es-PE')}`;
  }

  // ✅ Toggle favorito
  toggleFavorito(e: Event, id: number): void {
    e.stopPropagation();

    const index = this.favoritos.indexOf(id);
    if (index >= 0) {
      this.favoritos.splice(index, 1);
      console.log('💔 Removido de favoritos:', id);
    } else {
      this.favoritos.push(id);
      console.log('❤️ Agregado a favoritos:', id);
    }

    this.guardarFavoritos();
  }

  // ✅ Verificar si es favorito
  isFavorito(id: number): boolean {
    return this.favoritos.includes(id);
  }

  // ✅ Guardar favoritos en localStorage
  private guardarFavoritos(): void {
    try {
      localStorage.setItem('favoritos', JSON.stringify(this.favoritos));
    } catch (error) {
      console.error('Error al guardar favoritos:', error);
    }
  }

  // ✅ Cargar favoritos desde localStorage
  private cargarFavoritos(): void {
    try {
      const stored = localStorage.getItem('favoritos');
      if (stored) {
        this.favoritos = JSON.parse(stored);
        console.log('❤️ Favoritos cargados:', this.favoritos.length);
      }
    } catch (error) {
      console.error('Error al cargar favoritos:', error);
      this.favoritos = [];
    }
  }

  // ✅ Navegar a detalle de propiedad
  onCardClick(id: number): void {
    this.router.navigate(['/properties', id]);
  }

  // ✅ Cambiar vista (grid/list)
  setViewMode(mode: ViewMode): void {
    this.viewMode = mode;
  }

  // ✅ Toggle panel de filtros
  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }

  // ✅ Limpiar filtros
  clearFilters(): void {
    this.searchText = '';
    this.selectedType = '';
    this.selectedCurrency = '';
    this.minPrice = '';
    this.maxPrice = '';
    this.selectedBedrooms = '';
    this.sortBy = 'recent';
    this.applyFilters();
  }

  // ✅ Cambiar ordenamiento
  onSortChange(sort: 'recent' | 'price-low' | 'price-high'): void {
    this.sortBy = sort;
    this.applyFilters();
  }
}
