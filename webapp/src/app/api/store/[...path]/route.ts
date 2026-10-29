import { NextRequest, NextResponse } from "next/server";
import { sameOrigin, token, unavailable, upstream } from "@/lib/server";
export const dynamic="force-dynamic";
async function handle(request:NextRequest,{params}:{params:Promise<{path:string[]}>}) {
 const {path}=await params;
 if(!["products","categories","brands","cart","orders","admin","variants"].includes(path[0])) return new NextResponse(null,{status:404});
 if(!["GET","HEAD"].includes(request.method)&&!sameOrigin(request)) return NextResponse.json(null,{status:403});
 try {
  const access=await token();
  const headers=new Headers();
  if(access) headers.set("Authorization","Bearer "+access);
  const type=request.headers.get("Content-Type");
  if(type) headers.set("Content-Type",type);
  let body:ArrayBuffer|undefined;
  if(!["GET","HEAD"].includes(request.method)) {
   if(Number(request.headers.get("Content-Length")||0)>6_000_000) return new NextResponse(null,{status:413});
   const reader=request.body?.getReader();
   const chunks:Uint8Array[]=[];let size=0;
   if(reader) while(true) {
    const {done,value}=await reader.read();if(done)break;
    size+=value.length;
    if(size>6_000_000) {await reader.cancel();return new NextResponse(null,{status:413});}
    chunks.push(value);
   }
   const combined=new Uint8Array(size);let offset=0;
   for(const chunk of chunks){combined.set(chunk,offset);offset+=chunk.length;}
   body=combined.buffer;
  }
  const result=await upstream("/api/"+path.map(encodeURIComponent).join("/")+request.nextUrl.search,{method:request.method,headers,body});
  return new NextResponse(result.status===204?null:await result.arrayBuffer(),{status:result.status,headers:{"Content-Type":result.headers.get("Content-Type")||"application/json","Cache-Control":"no-store"}});
 } catch {return unavailable();}
}
export {handle as GET,handle as POST,handle as PUT,handle as DELETE};
