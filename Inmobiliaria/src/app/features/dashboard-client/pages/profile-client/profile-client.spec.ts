import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfileClient } from './profile-client';

describe('ProfileClient', () => {
  let component: ProfileClient;
  let fixture: ComponentFixture<ProfileClient>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ProfileClient]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProfileClient);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
