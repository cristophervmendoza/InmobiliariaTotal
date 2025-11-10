import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalPublicarTestimonioComponent } from './modal-publicar-testimonio.component';

describe('ModalPublicarTestimonioComponent', () => {
  let component: ModalPublicarTestimonioComponent;
  let fixture: ComponentFixture<ModalPublicarTestimonioComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ModalPublicarTestimonioComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(ModalPublicarTestimonioComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
