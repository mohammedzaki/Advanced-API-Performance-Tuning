import http from 'k6/http';
import { sleep } from 'k6';

// Configuration from environment variables with defaults
const BASE_URL = __ENV.BASE_URL || 'http://localhost:8081';
const VUS = __ENV.VUS || 100;
const DURATION = __ENV.DURATION || '10s';
const SLEEP_DURATION = __ENV.SLEEP_DURATION || 0.2;

export const options = {
  vus: parseInt(VUS),
  duration: DURATION,

  thresholds: {
    http_req_duration: [
      'p(50)<150', // 50% of requests must complete below 150ms
      'p(99)<300'  // 99% of requests must complete below 300ms
    ],
  },
};

export default function () {
  // Option 1: Blocking SQL call (without threading)
  //http.get(`${BASE_URL}/api/blocking-sql/blocking-optimized`);
  
  // Option 2: Non-blocking SQL call (with threading optimization)
  http.get(`${BASE_URL}/api/blocking-sql/non-blocking-optimized`);
  
  sleep(parseFloat(SLEEP_DURATION));
}
