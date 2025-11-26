import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
  vus: 20,
  duration: '30s',

  thresholds: {
    http_req_duration: [
      'p(50)<800', // 50% of requests must complete below 200ms
      'p(99)<900' // 99% of requests must complete below 1000ms
    ],
  },
};

export default function () {
  http.get('http://dotnet-app:8080/api/Products');
  sleep(1);
}
