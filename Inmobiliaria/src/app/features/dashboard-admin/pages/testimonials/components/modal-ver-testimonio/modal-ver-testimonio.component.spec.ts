import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalVerTestimonioComponent } from './modal-ver-testimonio.component';

describe('ModalVerTestimonioComponent', () => {
  let component: ModalVerTestimonioComponent;
  let fixture: ComponentFixture<ModalVerTestimonioComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ModalVerTestimonioComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(ModalVerTestimonioComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
