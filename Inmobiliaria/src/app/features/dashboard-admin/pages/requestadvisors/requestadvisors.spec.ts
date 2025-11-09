import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Requestadvisors } from './requestadvisors';

describe('Requestadvisors', () => {
  let component: Requestadvisors;
  let fixture: ComponentFixture<Requestadvisors>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Requestadvisors]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Requestadvisors);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
