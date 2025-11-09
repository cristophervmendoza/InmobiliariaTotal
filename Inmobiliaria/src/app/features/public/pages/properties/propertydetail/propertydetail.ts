// src/app/features/public/pages/properties/propertydetail/propertydetail.ts
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

type PropertyDetailModel = {
  id: number;
  title: string;
  location: string;
  price: number;
  currency: 'PEN' | 'USD';
  bedrooms: number;
  bathrooms: number;
  area: number;
  type: string;
  status: string;
  images: string[];
  description: string;
  features: string[];
  amenities: { label: string; active: boolean }[];
  agent: { name: string; role: string; phone: string; email: string };
};

@Component({
  selector: 'app-property-detail',
  standalone: false,
  templateUrl: './propertydetail.html',
  styleUrls: ['./propertydetail.css'],
})
export class PropertyDetail implements OnInit {
  isLoading = true;
  property: PropertyDetailModel | null = null;
  currentImage = 0;
  get totalImages() { return this.property?.images.length || 0; }

  constructor(private route: ActivatedRoute, private router: Router) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(pm => {
      const id = Number(pm.get('id'));
      if (!Number.isFinite(id)) {
        this.property = null;
        return;
      }
      this.loadProperty(id);
    });
  }

  async loadProperty(id: number) {
    this.isLoading = true;
    try {
      await new Promise(r => setTimeout(r, 300));

      // Generar datos distintos según id
      const baseTitle = ['Casa Moderna en San Isidro', 'Departamento Vista al Mar', 'Casa Campestre Exclusiva', 'Departamento en Surco', 'Casa en Los Olivos', 'Oficina en San Borja'];
      const baseLoc = ['San Isidro, Lima', 'Miraflores, Lima', 'La Molina, Lima', 'Santiago de Surco, Lima', 'Los Olivos, Lima', 'San Borja, Lima'];
      const idx = (id - 1) % baseTitle.length;

      this.property = {
        id,
        title: baseTitle[idx],
        location: baseLoc[idx],
        price: [850000, 680000, 1200000, 450000, 380000, 320000][idx],
        currency: 'PEN',
        bedrooms: [4, 3, 5, 2, 3, 0][idx],
        bathrooms: [3, 2, 4, 2, 2, 2][idx],
        area: [250, 180, 320, 120, 150, 80][idx],
        type: ['casa', 'departamento', 'casa', 'departamento', 'casa', 'oficina'][idx],
        status: 'Disponible',
        images: [
          `https://via.placeholder.com/1200x800/06b6d4/ffffff?text=Propiedad+${id}+Foto+1`,
          `https://via.placeholder.com/1200x800/0891b2/ffffff?text=Propiedad+${id}+Foto+2`,
          `https://via.placeholder.com/1200x800/06b6d4/ffffff?text=Propiedad+${id}+Foto+3`,
          `https://via.placeholder.com/1200x800/0891b2/ffffff?text=Propiedad+${id}+Foto+4`
        ],
        description: 'Propiedad con excelente iluminación natural y acabados de primera.',
        features: ['Cocina integrada', 'Balcón', 'Cochera', 'Área de parrilla'],
        amenities: [
          { label: 'Parque cercano', active: true },
          { label: 'Seguridad 24/7', active: true },
          { label: 'Ascensor', active: idx % 2 === 1 },
          { label: 'Pet-friendly', active: true }
        ],
        agent: { name: 'María Pérez', role: 'Agente Senior', phone: '+51 999 999 999', email: 'maria@idealhome.pe' }
      };

      // Reinicia índice de imagen al cambiar de propiedad
      this.currentImage = 0;
    } finally {
      this.isLoading = false;
    }
  }


  prevImage() { if (this.totalImages) this.currentImage = (this.currentImage - 1 + this.totalImages) % this.totalImages; }
  nextImage() { if (this.totalImages) this.currentImage = (this.currentImage + 1) % this.totalImages; }
  selectImage(i: number) { this.currentImage = i; }

  backToList() { this.router.navigate(['/properties']); }

  formatPrice(price: number, currency: 'PEN' | 'USD') {
    const symbol = currency === 'PEN' ? 'S/' : '$';
    return `${symbol} ${price.toLocaleString('es-PE')}`;
  }
}
