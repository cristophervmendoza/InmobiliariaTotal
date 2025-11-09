import { NgModule, Injectable } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import {
  HttpClientModule,
  HTTP_INTERCEPTORS,
  HttpClient,
  HttpEvent,
  HttpInterceptor,
  HttpRequest,
  HttpHandler
} from '@angular/common/http';
import { Observable as RxObservable, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

// Layout
import { Layout } from './layout/layout';

// Shared UI del layout
import { Topbar } from './components/topbar/topbar';
import { Navbar } from './components/navbar/navbar';
import { Footer } from './components/footer/footer';

// Pages
import { Home } from './pages/home/home';
import { BeAsesor } from './pages/be-asesor/be-asesor';
import { Offer } from './pages/offer/offer';
import { Properties } from './pages/properties/properties';
import { Testimony } from './pages/testimony/testimony';
import { PropertyDetail } from './pages/properties/propertydetail/propertydetail';
// Routing del feature
import { PublicRoutingModule } from './public-routing.module';

// Lucide Angular
import {
  LucideAngularModule,
  // Íconos que ya tenías
  Phone, Mail, MapPin, Building2,
  User, Calendar, Home as HomeIcon, X, Menu,
  Facebook, Twitter, Instagram,
  Search, Award, TrendingUp, CheckCircle, Clock, Star, ArrowRight, Users,
  Trophy, DollarSign, BookOpen, Users2,
  Upload, Check, AlertCircle, FileText,
  // Íconos añadidos para Properties
  Grid3x3, List, Heart, Bed, Bath, Maximize, SlidersHorizontal
} from 'lucide-angular';


// Servicio de subida (opcional)
@Injectable({ providedIn: 'root' })
export class UploadService {
  constructor(private http: HttpClient) { }
  uploadProperty(url: string, data: FormData): Observable<HttpEvent<unknown>> {
    return this.http.post<unknown>(url, data, {
      reportProgress: true,
      observe: 'events'
    });
  }
}

// Interceptor de progreso (opcional)
@Injectable()
export class ProgressInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<unknown>, next: HttpHandler): RxObservable<HttpEvent<unknown>> {
    return next.handle(req).pipe(
      tap({
        next: () => { /* progreso opcional */ },
        error: () => { /* manejo de error global */ }
      })
    );
  }
}

@NgModule({
  declarations: [
    Layout,
    Topbar, Navbar, Footer,
    Home, BeAsesor, Offer, Properties, Testimony, PropertyDetail
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    PublicRoutingModule,
    HttpClientModule,
    LucideAngularModule.pick({
      // Base
      Phone, Mail, MapPin, Building2, User,
      Calendar,
      Home: HomeIcon,
      X,
      Menu,
      Facebook, Twitter, Instagram,
      Search, Award, TrendingUp, CheckCircle, Clock, Star, ArrowRight, Users,
      Trophy, DollarSign, BookOpen, Users2,
      Upload, Check, AlertCircle, FileText,
      // Requeridos por properties.html
      Grid3x3, List, Heart, Bed, Bath, Maximize, SlidersHorizontal
    })
  ],
  exports: [
    Offer,
    Testimony
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: ProgressInterceptor, multi: true }
  ]
})
export class PublicModule { }
