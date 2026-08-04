import { NextRequest, NextResponse } from "next/server"

import { auth0 } from "@/lib/auth/auth0-server"

export async function POST(request: NextRequest) {
  if (!isSameOriginRequest(request)) {
    return NextResponse.json(
      { error: "invalid_origin" },
      {
        status: 403,
        headers: {
          "Cache-Control": "no-store",
        },
      },
    )
  }

  const response = new NextResponse(null, {
    status: 204,
    headers: {
      "Cache-Control": "no-store",
    },
  })

  try {
    await auth0.getAccessToken(request, response, { refresh: true })
    return response
  } catch {
    return NextResponse.json(
      { error: "authentication_required" },
      {
        status: 401,
        headers: {
          "Cache-Control": "no-store",
        },
      },
    )
  }
}

function isSameOriginRequest(request: NextRequest): boolean {
  const origin = request.headers.get("origin")
  return origin !== null && origin === request.nextUrl.origin
}
