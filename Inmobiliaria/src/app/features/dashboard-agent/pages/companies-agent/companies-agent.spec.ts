import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CompaniesAgent } from './companies-agent';

describe('CompaniesAgent', () => {
  let component: CompaniesAgent;
  let fixture: ComponentFixture<CompaniesAgent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CompaniesAgent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CompaniesAgent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
