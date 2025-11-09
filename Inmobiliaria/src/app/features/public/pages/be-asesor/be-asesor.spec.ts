import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BeAsesor } from './be-asesor';

describe('BeAsesor', () => {
  let component: BeAsesor;
  let fixture: ComponentFixture<BeAsesor>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [BeAsesor]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BeAsesor);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
