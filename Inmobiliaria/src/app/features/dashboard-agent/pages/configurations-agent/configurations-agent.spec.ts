import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfigurationsAgent } from './configurations-agent';

describe('ConfigurationsAgent', () => {
  let component: ConfigurationsAgent;
  let fixture: ComponentFixture<ConfigurationsAgent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ConfigurationsAgent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfigurationsAgent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
