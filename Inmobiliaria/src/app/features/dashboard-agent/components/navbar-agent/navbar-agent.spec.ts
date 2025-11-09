import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NavbarAgent } from './navbar-agent';

describe('NavbarAgent', () => {
  let component: NavbarAgent;
  let fixture: ComponentFixture<NavbarAgent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [NavbarAgent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NavbarAgent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
