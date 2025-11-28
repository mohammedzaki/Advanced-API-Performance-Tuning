import http from 'k6/http';
import { sleep } from 'k6';
import { check } from 'k6';

// Configuration from environment variables with defaults
const BASE_URL = __ENV.BASE_URL || 'http://localhost:8081';
const VUS = __ENV.VUS || 100;
const DURATION = __ENV.DURATION || '60s';
const SLEEP_DURATION = __ENV.SLEEP_DURATION || 1;
const REPORT_DEFAULT_MONTHS = __ENV.REPORT_DEFAULT_MONTHS || 2;
const REPORT_DEFAULT_RECORDS = __ENV.REPORT_DEFAULT_RECORDS || 1000;
const REPORT_MAX_COMPLEXITY = __ENV.REPORT_MAX_COMPLEXITY || 12;

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
  // Quick sales report (configurable months)
  const quickMonths = parseInt(REPORT_DEFAULT_MONTHS);
  let response1 = http.get(`${BASE_URL}/api/report/sales-report?months=${quickMonths}`);
  check(response1, {
    'Quick report status is 200': (r) => r.status === 200,
  });

  // Large analytics dataset (configurable records)  
  const records = parseInt(REPORT_DEFAULT_RECORDS);
  let response2 = http.get(`${BASE_URL}/api/report/detailed-analytics?records=${records}`);
  check(response2, {
    'Analytics report status is 200': (r) => r.status === 200,
  });

  // Heavy processing simulation (configurable complexity)
  const complexMonths = parseInt(REPORT_MAX_COMPLEXITY);
  let response3 = http.get(`${BASE_URL}/api/report/sales-report?months=${complexMonths}&delay=1000`);
  check(response3, {
    'Heavy report status is 200': (r) => r.status === 200,
  });

  sleep(parseFloat(SLEEP_DURATION));
}
