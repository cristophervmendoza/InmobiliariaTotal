import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardAgent } from './dashboard-agent';

describe('DashboardAgent', () => {
  let component: DashboardAgent;
  let fixture: ComponentFixture<DashboardAgent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DashboardAgent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DashboardAgent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
