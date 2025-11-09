import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PropertiesAdmin } from './properties-admin';

describe('PropertiesAdmin', () => {
  let component: PropertiesAdmin;
  let fixture: ComponentFixture<PropertiesAdmin>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PropertiesAdmin]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PropertiesAdmin);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
