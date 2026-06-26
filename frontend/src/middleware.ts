import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

// Protect the authenticated portal: no access_token cookie → bounce to /login.
// (The cookie is httpOnly; middleware only checks presence, not contents.)
export function middleware(req: NextRequest) {
  const token = req.cookies.get("access_token")?.value;
  if (!token) {
    const url = req.nextUrl.clone();
    url.pathname = "/login";
    return NextResponse.redirect(url);
  }
  return NextResponse.next();
}

export const config = {
  matcher: ["/portal/:path*"],
};
