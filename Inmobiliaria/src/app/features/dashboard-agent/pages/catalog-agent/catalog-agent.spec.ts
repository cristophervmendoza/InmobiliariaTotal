import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CatalogAgent } from './catalog-agent';

describe('CatalogAgent', () => {
  let component: CatalogAgent;
  let fixture: ComponentFixture<CatalogAgent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CatalogAgent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CatalogAgent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
