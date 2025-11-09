import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Layout
import { Layout } from './layout/layout';

// Pages (no standalone)
import { Home } from './pages/home/home';
import { BeAsesor } from './pages/be-asesor/be-asesor';
import { Offer } from './pages/offer/offer';
import { Properties } from './pages/properties/properties';
import { PropertyDetail } from './pages/properties/propertydetail/propertydetail';
import { Testimony } from './pages/testimony/testimony';

const routes: Routes = [
  {
    path: '',
    component: Layout,
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', component: Home },
      { path: 'beasesor', component: BeAsesor },
      { path: 'offer', component: Offer },
      { path: 'properties', component: Properties },
      { path: 'properties/:id', component: PropertyDetail },
      { path: 'testimony', component: Testimony }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PublicRoutingModule { }
