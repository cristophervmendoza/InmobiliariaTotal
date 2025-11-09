import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MaintenanceAgent } from './maintenance-agent';

describe('MaintenanceAgent', () => {
  let component: MaintenanceAgent;
  let fixture: ComponentFixture<MaintenanceAgent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MaintenanceAgent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MaintenanceAgent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
