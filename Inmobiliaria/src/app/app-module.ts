import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http'; // <-- importa HTTP

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';

import { Loader } from './shared/components/loader/loader';
import { Alert } from './shared/components/alert/alert';
import { Dbconnection as Dbconnection } from './dbconnection/dbconnection'; // Asegura la ruta/clase

@NgModule({
  declarations: [
    App,
    Loader,
    Alert,
    Dbconnection,
  ],
  imports: [
    BrowserModule,
    HttpClientModule, // <-- requerido para HttpClient
    AppRoutingModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners()
  ],
  bootstrap: [App]
})
export class AppModule { }
