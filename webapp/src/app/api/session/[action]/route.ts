import { cookies } from "next/headers";
import { NextRequest, NextResponse } from "next/server";
import { cookieNames, clearTokens, sameOrigin, setTokens, token, unavailable, upstream } from "@/lib/server";
export const dynamic="force-dynamic";
export async function GET(request:NextRequest,{params}:{params:Promise<{action:string}>}) {
 if((await params).action!=="me") return new NextResponse(null,{status:404});
 const access=await token();
 if(!access) return NextResponse.json(null,{status:401});
 try {
  const result=await upstream("/api/auth/me",{headers:{Authorization:"Bearer "+access}});
  if(!result.ok) return NextResponse.json(null,{status:result.status});
  return NextResponse.json(await result.json(),{headers:{"Cache-Control":"no-store"}});
 } catch { return unavailable(); }
}
export async function POST(request:NextRequest,{params}:{params:Promise<{action:string}>}) {
 if(!sameOrigin(request)) return NextResponse.json({message:"Forbidden"},{status:403});
 const {action}=await params;
 if(!["login","register","refresh","logout"].includes(action)) return new NextResponse(null,{status:404});
 try {
  const jar=await cookies();
  if(action==="logout") {
   const refresh=jar.get(cookieNames.refresh)?.value;
   if(refresh) await upstream("/api/auth/logout",{method:"POST",headers:{"Content-Type":"application/json"},body:JSON.stringify({refreshToken:refresh})}).catch(()=>null);
   const result=NextResponse.json({ok:true}); clearTokens(result); return result;
  }
  const payload=action==="refresh" ? {refreshToken:jar.get(cookieNames.refresh)?.value} : await request.json();
  if(action==="refresh"&&!payload.refreshToken) return NextResponse.json(null,{status:401});
  const upstreamResponse=await upstream("/api/auth/"+action,{method:"POST",headers:{"Content-Type":"application/json"},body:JSON.stringify(payload)});
  const data=await upstreamResponse.json().catch(()=>null);
  if(!upstreamResponse.ok) {
   const result=NextResponse.json(data,{status:upstreamResponse.status});
   if(action==="refresh" && [400,401].includes(upstreamResponse.status)) clearTokens(result);
   return result;
  }
  const result=NextResponse.json({ok:true});
  if(action!=="register") setTokens(result,data.returnObject);
  return result;
 } catch { return unavailable(); }
}
