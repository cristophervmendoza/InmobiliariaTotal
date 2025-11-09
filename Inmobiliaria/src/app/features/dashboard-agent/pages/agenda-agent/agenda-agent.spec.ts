import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AgendaAgent } from './agenda-agent';

describe('AgendaAgent', () => {
  let component: AgendaAgent;
  let fixture: ComponentFixture<AgendaAgent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AgendaAgent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AgendaAgent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
