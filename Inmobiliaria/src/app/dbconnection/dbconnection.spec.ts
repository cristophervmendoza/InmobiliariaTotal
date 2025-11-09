import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Dbconnection } from './dbconnection';

describe('Dbconnection', () => {
  let component: Dbconnection;
  let fixture: ComponentFixture<Dbconnection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Dbconnection]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Dbconnection);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
