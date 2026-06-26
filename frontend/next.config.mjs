/** @type {import('next').NextConfig} */
const BACKEND_URL = process.env.BACKEND_URL || "http://localhost:8000";

const nextConfig = {
  reactStrictMode: true,
  // BFF proxy: the browser calls same-origin /api/*, Next forwards to FastAPI.
  // Backend httpOnly auth cookies pass through and become first-party to the
  // frontend origin, so SameSite=Lax cookies are sent on subsequent requests.
  async rewrites() {
    return [{ source: "/api/:path*", destination: `${BACKEND_URL}/api/:path*` }];
  },
};

export default nextConfig;
