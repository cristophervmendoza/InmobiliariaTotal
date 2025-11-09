import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Offer } from './offer';

describe('Offer', () => {
  let component: Offer;
  let fixture: ComponentFixture<Offer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Offer]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Offer);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
