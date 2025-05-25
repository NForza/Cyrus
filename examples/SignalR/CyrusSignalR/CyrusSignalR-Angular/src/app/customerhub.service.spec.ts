import { TestBed } from '@angular/core/testing';

import { CustomerService } from './customerhub.service';

describe('CustomerhubService', () => {
  let service: CustomerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CustomerhubService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
