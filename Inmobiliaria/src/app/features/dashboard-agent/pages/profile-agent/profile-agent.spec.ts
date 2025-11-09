import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfileAgent } from './profile-agent';

describe('ProfileAgent', () => {
  let component: ProfileAgent;
  let fixture: ComponentFixture<ProfileAgent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ProfileAgent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProfileAgent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
