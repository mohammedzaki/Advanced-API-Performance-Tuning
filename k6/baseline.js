import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
  vus: 100,
  duration: '10s',

  thresholds: {
    http_req_duration: [
      'p(50)<150', // 50% of requests must complete below 200ms
      'p(99)<300' // 99% of requests must complete below 1000ms
    ],
  },
};

export default function () {
  //http.get('http://localhost:8080/api/blocking-sql/blocking-optimized'); // -> 1 without Threading
  http.get('http://localhost:8080/api/blocking-sql/non-blocking-optimized'); // -> 2 -> with Threading 
  sleep(.2);
}
