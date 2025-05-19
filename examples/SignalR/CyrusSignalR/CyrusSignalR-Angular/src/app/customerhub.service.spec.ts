import { TestBed } from '@angular/core/testing';

import { CustomerhubService } from './customerhub.service';

describe('CustomerhubService', () => {
  let service: CustomerhubService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CustomerhubService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
