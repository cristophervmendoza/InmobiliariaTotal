import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NavbarClient } from './navbar-client';

describe('NavbarClient', () => {
  let component: NavbarClient;
  let fixture: ComponentFixture<NavbarClient>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [NavbarClient]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NavbarClient);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
