import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Propertydetail } from './propertydetail';

describe('Propertydetail', () => {
  let component: Propertydetail;
  let fixture: ComponentFixture<Propertydetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Propertydetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Propertydetail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
