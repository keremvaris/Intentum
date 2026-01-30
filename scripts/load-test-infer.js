// k6 load test for Sample.Web POST /api/intent/infer
// Run: k6 run scripts/load-test-infer.js
// Base URL: set K6_BASE_URL or default http://localhost:5000
import http from 'k6/http';
import { check, sleep } from 'k6';

const baseUrl = __ENV.K6_BASE_URL || 'http://localhost:5000';

export const options = {
  vus: 50,
  duration: '30s',
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<2000'],
  },
};

const payload = JSON.stringify({
  events: [
    { actor: 'user', action: 'login' },
    { actor: 'user', action: 'submit' },
  ],
});

export default function () {
  const res = http.post(`${baseUrl}/api/intent/infer`, payload, {
    headers: { 'Content-Type': 'application/json' },
  });
  check(res, { 'status is 200': (r) => r.status === 200 });
  sleep(0.1);
}
