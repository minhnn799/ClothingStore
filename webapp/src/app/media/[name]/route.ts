import { NextResponse } from "next/server";
import { upstream } from "@/lib/server";
export async function GET(_request:Request,{params}:{params:Promise<{name:string}>}) {
 const {name}=await params;
 if(!/^[a-f0-9]{32}\.(png|jpg|webp)$/.test(name)) return new NextResponse(null,{status:404});
 try {
  const result=await upstream("/uploads/"+name);
  if(!result.ok) return new NextResponse(null,{status:404});
  return new NextResponse(await result.arrayBuffer(),{headers:{"Content-Type":result.headers.get("Content-Type")||"image/jpeg","X-Content-Type-Options":"nosniff"}});
 } catch {return new NextResponse(null,{status:503});}
}
