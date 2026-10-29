import { translateError } from "./errors.mjs";
export class ApiError extends Error { constructor(message:string,public status:number) { super(message); } }
let refreshing:Promise<boolean>|null=null;
export async function api<T>(path:string, options:RequestInit = {}, retry=true):Promise<T> {
 const response=await fetch("/api/store/"+path, {...options, headers:{...(options.body instanceof FormData ? {} : {"Content-Type":"application/json"}), ...options.headers}});
 if(response.status===401 && retry && !path.startsWith("auth/")) {
  refreshing ??= fetch("/api/session/refresh",{method:"POST"}).then(r=>r.ok).finally(()=>{refreshing=null});
  if(await refreshing) return api<T>(path, options, false);
 }
 const data=response.status===204?null:await response.json().catch(()=>null);
 if(!response.ok) throw new ApiError(translateError(data,response.status),response.status);
 return data as T;
}
export async function sessionRequest(path:string,body?:unknown) {
 const response=await fetch("/api/session/"+path,{method:"POST",headers:{"Content-Type":"application/json"},body:body?JSON.stringify(body):undefined});
 const data=await response.json().catch(()=>null);
 if(!response.ok) throw new ApiError(translateError(data,response.status),response.status);
 return data;
}
