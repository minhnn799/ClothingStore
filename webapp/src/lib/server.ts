import "server-only";
import { cookies } from "next/headers";
import { NextRequest, NextResponse } from "next/server";
export const backend = () => (process.env.BACKEND_URL || "http://localhost:5000").replace(/\/$/,"");
export const cookieNames = {access:"clothing_access",refresh:"clothing_refresh"};
export function sameOrigin(request:NextRequest) {
 const origin=request.headers.get("origin");
 const expected=process.env.APP_ORIGIN || request.nextUrl.origin;
 return origin === expected;
}
export async function upstream(path:string, init:RequestInit={}) {
 return fetch(backend()+path, {...init,cache:"no-store",signal:AbortSignal.timeout(15000)});
}
export async function token() { return (await cookies()).get(cookieNames.access)?.value; }
export function setTokens(response:NextResponse, tokens:{accessToken:string;refreshToken:string}) {
 const options={httpOnly:true,sameSite:"lax" as const,secure:process.env.COOKIE_SECURE==="true",path:"/"};
 response.cookies.set(cookieNames.access,tokens.accessToken,{...options,maxAge:86400});
 response.cookies.set(cookieNames.refresh,tokens.refreshToken,{...options,maxAge:604800});
}
export function clearTokens(response:NextResponse) {
 response.cookies.set(cookieNames.access,"",{httpOnly:true,path:"/",maxAge:0});
 response.cookies.set(cookieNames.refresh,"",{httpOnly:true,path:"/",maxAge:0});
}
export const unavailable = () => NextResponse.json({message:"Không thể kết nối máy chủ."},{status:503});
