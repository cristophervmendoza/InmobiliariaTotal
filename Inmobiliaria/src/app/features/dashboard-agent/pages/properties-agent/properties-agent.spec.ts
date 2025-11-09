import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PropertiesAgent } from './properties-agent';

describe('PropertiesAgent', () => {
  let component: PropertiesAgent;
  let fixture: ComponentFixture<PropertiesAgent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PropertiesAgent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PropertiesAgent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
