import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SidebarAgent } from './sidebar-agent';

describe('SidebarAgent', () => {
  let component: SidebarAgent;
  let fixture: ComponentFixture<SidebarAgent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SidebarAgent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SidebarAgent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
