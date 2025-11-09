import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfigurationsClient } from './configurations-client';

describe('ConfigurationsClient', () => {
  let component: ConfigurationsClient;
  let fixture: ComponentFixture<ConfigurationsClient>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ConfigurationsClient]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfigurationsClient);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
