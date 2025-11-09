import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MessagesAgent } from './messages-agent';

describe('MessagesAgent', () => {
  let component: MessagesAgent;
  let fixture: ComponentFixture<MessagesAgent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MessagesAgent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MessagesAgent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
