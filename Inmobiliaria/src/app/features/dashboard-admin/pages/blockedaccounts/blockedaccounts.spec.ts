import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Blockedaccounts } from './blockedaccounts';

describe('Blockedaccounts', () => {
  let component: Blockedaccounts;
  let fixture: ComponentFixture<Blockedaccounts>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Blockedaccounts]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Blockedaccounts);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
