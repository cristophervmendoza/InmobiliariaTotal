import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TestimonialsComponent } from './testimonials';  // ← CAMBIAR ESTO

describe('TestimonialsComponent', () => {  // ← CAMBIAR ESTO
  let component: TestimonialsComponent;  // ← CAMBIAR ESTO
  let fixture: ComponentFixture<TestimonialsComponent>;  // ← CAMBIAR ESTO

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TestimonialsComponent]  // ← CAMBIAR ESTO
    })
      .compileComponents();

    fixture = TestBed.createComponent(TestimonialsComponent);  // ← CAMBIAR ESTO
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
