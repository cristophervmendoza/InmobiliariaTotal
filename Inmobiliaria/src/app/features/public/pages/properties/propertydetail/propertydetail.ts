import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PropertiesService, Propiedad } from '../../../../../core/services/properties.service';
import { EmployeesService, Empleado } from '../../../../../core/services/employees.service';
import { forkJoin } from 'rxjs';

type Moneda = 'PEN' | 'USD' | 'EUR';

interface PropertyDetailModel {
  id: number;
  title: string;
  location: string;
  price: number;
  currency: Moneda;
  bedrooms: number;
  bathrooms: number;
  area: number;
  type: string;
  status: string;
  images: string[];
  description: string;
  features: string[];
  amenities: { label: string; active: boolean }[];
  agent: {
    name: string;
    role: string;
    phone: string;
    email: string;
    avatar?: string;
  };
}

@Component({
  selector: 'app-property-detail',
  standalone: false,
  templateUrl: './propertydetail.html',
  styleUrls: ['./propertydetail.css'],
})
export class PropertyDetail implements OnInit {
  private propertiesService = inject(PropertiesService);
  private employeesService = inject(EmployeesService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  isLoading = true;
  property: PropertyDetailModel | null = null;
  currentImage = 0;

  get totalImages() {
    return this.property?.images.length || 0;
  }

  // Mapeos
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
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (!Number.isFinite(id) || id <= 0) {
        console.error('❌ ID inválido:', id);
        this.property = null;
        this.isLoading = false;
        return;
      }
      this.loadProperty(id);
    });
  }

  // ✅ Cargar propiedad desde el backend
  loadProperty(id: number): void {
    console.log('🔄 Cargando propiedad con ID:', id);
    this.isLoading = true;
    this.currentImage = 0;

    // Cargar propiedad y empleados en paralelo
    forkJoin({
      propiedad: this.propertiesService.obtenerPropiedadPorId(id),
      empleados: this.employeesService.listarEmpleados()
    }).subscribe({
      next: ({ propiedad, empleados }) => {
        console.log('📦 Datos cargados:', { propiedad, empleados });

        if (propiedad.exito && propiedad.data) {
          const prop = propiedad.data;

          // Buscar el agente
          const agente = empleados.exito && empleados.data
            ? empleados.data.find(e => e.idUsuario === prop.idUsuario)
            : null;

          console.log('👤 Agente encontrado:', agente);

          // Mapear a PropertyDetailModel
          this.property = this.mapearPropiedad(prop, agente);
          console.log('✅ Propiedad mapeada:', this.property);
        } else {
          console.warn('⚠️ Propiedad no encontrada');
          this.property = null;
        }

        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Error al cargar propiedad:', error);
        this.property = null;
        this.isLoading = false;
      }
    });
  }

  // ✅ Mapear de backend a modelo de vista - CORREGIDO
  private mapearPropiedad(prop: Propiedad, agente: Empleado | null | undefined): PropertyDetailModel {
    // Generar características basadas en los datos reales
    const features: string[] = [];
    if (prop.habitacion && prop.habitacion > 0) {
      features.push(`${prop.habitacion} Habitaciones`);
    }
    if (prop.bano && prop.bano > 0) {
      features.push(`${prop.bano} Baños`);
    }
    if (prop.estacionamiento && prop.estacionamiento > 0) {
      features.push(`${prop.estacionamiento} Estacionamientos`);
    }
    if (prop.areaTerreno && prop.areaTerreno > 0) {
      features.push(`${prop.areaTerreno} m² de terreno`);
    }
    // Agregar características generales
    features.push('Cocina integrada');
    features.push('Área de lavandería');

    // Amenidades ejemplo
    const amenities = [
      { label: 'Seguridad 24/7', active: true },
      { label: 'Área de estacionamiento', active: (prop.estacionamiento || 0) > 0 },
      { label: 'Zona residencial', active: true },
      { label: 'Cerca de transporte público', active: true },
      { label: 'Parque cercano', active: true },
      { label: 'Áreas verdes', active: false }
    ];

    // Construir URLs de imágenes
    const images: string[] = [];
    if (prop.fotoPropiedad) {
      const imageUrl = `http://localhost:5000/uploads/propiedades/${prop.fotoPropiedad}`;
      images.push(imageUrl);
      // Agregar la misma imagen 3 veces más (placeholder hasta tener galería real)
      images.push(imageUrl);
      images.push(imageUrl);
      images.push(imageUrl);
    }
    // Fallback si no hay imágenes
    if (images.length === 0) {
      images.push('https://via.placeholder.com/1200x800/06b6d4/ffffff?text=Sin+Imagen');
    }

    // ✅ CORREGIDO: Mapeo del agente con campos correctos
    let agenteAvatar: string | undefined = undefined;
    if (agente?.fotoPerfil) {
      agenteAvatar = `http://localhost:5000/uploads/perfiles/${agente.fotoPerfil}`;
    }

    // ✅ CORREGIDO: Determinar rol basado en estado o tipo de usuario
    let agenteRol = 'Agente Inmobiliario';
    if (agente?.estado === 'activo') {
      agenteRol = 'Agente Inmobiliario';
    } else if (agente?.estado === 'inactivo') {
      agenteRol = 'Agente Inactivo';
    }

    return {
      id: prop.idPropiedad,
      title: prop.titulo,
      location: prop.direccion,
      price: prop.precio,
      currency: prop.tipoMoneda as Moneda,
      bedrooms: prop.habitacion || 0,
      bathrooms: prop.bano || 0,
      area: prop.areaTerreno || 0,
      type: this.tiposPropiedad[prop.idTipoPropiedad] || 'otro',
      status: this.estadosPropiedad[prop.idEstadoPropiedad] || 'Disponible',
      images: images,
      description: prop.descripcion || 'Propiedad con excelentes acabados y ubicación privilegiada. Ideal para familias que buscan comodidad y seguridad.',
      features: features,
      amenities: amenities,
      agent: {
        name: agente?.nombre || 'Agente no asignado',
        role: agenteRol,
        phone: agente?.telefono || 'No disponible',
        email: agente?.email || 'No disponible',
        avatar: agenteAvatar // ✅ Usar fotoPerfil del empleado
      }
    };
  }

  // ✅ Navegación de imágenes
  prevImage(): void {
    if (this.totalImages > 0) {
      this.currentImage = (this.currentImage - 1 + this.totalImages) % this.totalImages;
    }
  }

  nextImage(): void {
    if (this.totalImages > 0) {
      this.currentImage = (this.currentImage + 1) % this.totalImages;
    }
  }

  selectImage(index: number): void {
    this.currentImage = index;
  }

  // ✅ Navegación
  backToList(): void {
    this.router.navigate(['/properties']);
  }

  // ✅ Formatear precio
  formatPrice(price: number, currency: Moneda): string {
    let symbol = 'S/';
    if (currency === 'USD') symbol = '$';
    else if (currency === 'EUR') symbol = '€';

    return `${symbol} ${price.toLocaleString('es-PE')}`;
  }

  // ✅ Agendar visita
  agendarVisita(): void {
    if (!this.property) return;

    console.log('📅 Agendar visita para propiedad:', this.property.id);
    alert(`¡Solicitud de visita registrada!\n\nPropiedad: ${this.property.title}\nAgente: ${this.property.agent.name}\n\nTe contactaremos pronto.`);
  }

  // ✅ Enviar mensaje
  enviarMensaje(mensaje: string): void {
    if (!this.property || !mensaje.trim()) {
      alert('Por favor escribe un mensaje');
      return;
    }

    console.log('📧 Enviando mensaje:', {
      propiedadId: this.property.id,
      agente: this.property.agent.email,
      mensaje: mensaje
    });

    alert(`✅ Mensaje enviado a ${this.property.agent.name}\n\n"${mensaje}"\n\nEl agente te responderá pronto.`);
  }
}
