"use client";
import { createContext, useContext, useEffect, useState, useCallback } from "react";
import { sessionRequest } from "@/lib/api";
import type { User } from "@/lib/types";
const Context=createContext<{user:User|null;loading:boolean;reload:()=>Promise<void>;logout:()=>Promise<void>}>({user:null,loading:true,reload:async()=>{},logout:async()=>{}});
export function SessionProvider({children}:{children:React.ReactNode}) {
 const [user,setUser]=useState<User|null>(null);const [loading,setLoading]=useState(true);
 const reload=useCallback(async()=>{
  try {
   let response=await fetch("/api/session/me",{cache:"no-store"});
   if(response.status===401) {
    const refresh=await fetch("/api/session/refresh",{method:"POST"});
    if(refresh.ok) response=await fetch("/api/session/me",{cache:"no-store"});
   }
   setUser(response.ok?await response.json():null);
  } catch {setUser(null);} finally {setLoading(false);}
 },[]);
 useEffect(()=>{void reload()},[reload]);
 async function logout(){await sessionRequest("logout");setUser(null);window.location.href="/";}
 return <Context.Provider value={{user,loading,reload,logout}}>{children}</Context.Provider>;
}
export const useSession=()=>useContext(Context);
