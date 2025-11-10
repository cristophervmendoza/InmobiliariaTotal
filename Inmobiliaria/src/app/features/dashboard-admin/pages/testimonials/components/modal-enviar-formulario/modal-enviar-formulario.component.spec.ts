import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalEnviarFormularioComponent } from './modal-enviar-formulario.component';

describe('ModalEnviarFormularioComponent', () => {
  let component: ModalEnviarFormularioComponent;
  let fixture: ComponentFixture<ModalEnviarFormularioComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ModalEnviarFormularioComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModalEnviarFormularioComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
