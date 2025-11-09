import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReportsAgent } from './reports-agent';

describe('ReportsAgent', () => {
  let component: ReportsAgent;
  let fixture: ComponentFixture<ReportsAgent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ReportsAgent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ReportsAgent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
