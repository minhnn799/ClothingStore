import {test,before,after} from "node:test";
import assert from "node:assert/strict";
import http from "node:http";
import net from "node:net";
import {spawn} from "node:child_process";
import {once} from "node:events";
let backend,app,base,log="",upstreamHeaders;
async function port(){const s=net.createServer();s.listen(0,"127.0.0.1");await once(s,"listening");const p=s.address().port;await new Promise(r=>s.close(r));return p;}
before(async()=>{
 backend=http.createServer(async(req,res)=>{
  let body="";for await(const chunk of req)body+=chunk;
  res.setHeader("Content-Type","application/json");
  if(req.url==="/api/auth/login"){
   const credentials=JSON.parse(body);
   if(credentials.password!=="Correct!123"){res.statusCode=401;res.end(JSON.stringify({message:"Invalid credentials"}));return;}
   res.end(JSON.stringify({returnObject:{accessToken:"private-access",refreshToken:"private-refresh"}}));
  } else if(req.url==="/api/auth/refresh") {
   assert.equal(JSON.parse(body).refreshToken,"private-refresh");
   res.end(JSON.stringify({returnObject:{accessToken:"renewed-access",refreshToken:"renewed-refresh"}}));
  } else if(req.url==="/api/auth/logout") res.end("{}");
  else if(req.url==="/api/auth/register")res.end("{}");
  else if(req.url==="/api/auth/me"){
   if(req.headers.authorization!=="Bearer private-access"){res.statusCode=401;res.end("{}");return;}
   res.end(JSON.stringify({email:"customer@example.test",isAdmin:false}));
  } else if(req.url.startsWith("/api/products")){
   upstreamHeaders=req.headers;
   res.end(JSON.stringify({items:[],total:0,page:1,pageSize:20}));
  } else {res.statusCode=404;res.end("{}");}
 });
 backend.listen(0,"127.0.0.1");await once(backend,"listening");
 const p=await port();base="http://127.0.0.1:"+p;
 app=spawn(process.execPath,["node_modules/next/dist/bin/next","start","--hostname","127.0.0.1","--port",String(p)],{env:{...process.env,BACKEND_URL:"http://127.0.0.1:"+backend.address().port,APP_ORIGIN:base,COOKIE_SECURE:"false",NEXT_TELEMETRY_DISABLED:"1"},stdio:["ignore","pipe","pipe"]});
 app.stdout.on("data",d=>log+=d);app.stderr.on("data",d=>log+=d);
 let ready=false;
 for(let i=0;i<120;i++){try{const r=await fetch(base);if(r.ok){ready=true;break;}}catch{}await new Promise(r=>setTimeout(r,250));}
 assert.ok(ready,"Next.js did not start: "+log);
}, {timeout:40000});
after(async()=>{if(app){app.kill();await Promise.race([once(app,"exit"),new Promise(r=>setTimeout(r,2000))]);}if(backend)await new Promise(r=>backend.close(r));});
async function login(){return fetch(base+"/api/session/login",{method:"POST",headers:{"Content-Type":"application/json",Origin:base},body:JSON.stringify({email:"customer@example.test",password:"Correct!123"})});}
test("Vietnamese document and accessible page routes render",async()=>{
 const response=await fetch(base);const html=await response.text();assert.match(html,/<html lang="vi"/);assert.match(html,/Sống trọn vẹn/);
 for(const route of ["/san-pham","/tai-khoan","/gio-hang","/thanh-toan","/don-hang","/quan-tri"])assert.equal((await fetch(base+route)).status,200);
});
test("cross-origin session and store writes are rejected",async()=>{
 const headers={"Content-Type":"application/json",Origin:"https://untrusted.example"};
 assert.equal((await fetch(base+"/api/session/login",{method:"POST",headers,body:"{}"})).status,403);
 assert.equal((await fetch(base+"/api/store/products",{method:"POST",headers,body:"{}"})).status,403);
});
test("tokens stay in HttpOnly cookies and never in the login body",async()=>{
 const response=await login();assert.equal(response.status,200);
 const text=await response.text();assert.equal(text.includes("private-access"),false);
 const cookies=response.headers.getSetCookie();assert.equal(cookies.length,2);
 for(const cookie of cookies){assert.match(cookie,/HttpOnly/i);assert.match(cookie,/SameSite=lax/i);assert.match(cookie,/Path=\//i);}
 const cookie=cookies.map(c=>c.split(";")[0]).join("; ");
 const me=await fetch(base+"/api/session/me",{headers:{Cookie:cookie}});assert.equal(me.status,200);assert.equal((await me.json()).email,"customer@example.test");
});
test("proxy forwards access token but never browser cookies",async()=>{
 const response=await fetch(base+"/api/store/products",{headers:{Cookie:"clothing_access=private-access; unrelated=secret"}});
 assert.equal(response.status,200);assert.equal(upstreamHeaders.authorization,"Bearer private-access");assert.equal(upstreamHeaders.cookie,undefined);
 assert.equal((await fetch(base+"/api/store/auth/login")).status,404);
});
test("refresh rotates cookies and logout expires them",async()=>{
 const response=await fetch(base+"/api/session/refresh",{method:"POST",headers:{Origin:base,Cookie:"clothing_refresh=private-refresh"}});
 assert.equal(response.status,200);assert.match(response.headers.get("set-cookie"),/renewed-access/);assert.deepEqual(await response.json(),{ok:true});
 const logout=await fetch(base+"/api/session/logout",{method:"POST",headers:{Origin:base,Cookie:"clothing_refresh=renewed-refresh"}});
 assert.equal(logout.status,200);assert.equal(logout.headers.getSetCookie().length,2);assert.match(logout.headers.get("set-cookie"),/Max-Age=0/);
});
test("failed login returns no session cookies",async()=>{
 const response=await fetch(base+"/api/session/login",{method:"POST",headers:{Origin:base,"Content-Type":"application/json"},body:JSON.stringify({email:"x",password:"wrong"})});
 assert.equal(response.status,401);assert.equal(response.headers.get("set-cookie"),null);
});
test("oversized requests and invalid media paths are rejected",async()=>{
 const response=await fetch(base+"/api/store/products",{method:"POST",headers:{Origin:base,"Content-Type":"application/octet-stream"},body:new Uint8Array(6_000_001)});
 assert.equal(response.status,413);
 assert.equal((await fetch(base+"/media/not-a-product.html")).status,404);
});
