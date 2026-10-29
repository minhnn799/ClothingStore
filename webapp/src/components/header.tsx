"use client";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { ShoppingBag, UserRound, ArrowUpRight, Menu, X } from "lucide-react";
import { useState } from "react";
import { useSession } from "./session";
export function Header() {
 const {user,logout}=useSession();const path=usePathname();const [open,setOpen]=useState(false);
 return <>
  <div className="announcement">Giao hàng miễn phí cho mọi đơn hàng <span>·</span> Thanh toán khi nhận hàng <ArrowUpRight size={12}/></div>
  <header className="header">
   <Link className="wordmark" href="/" aria-label="ClothingStore — Trang chủ">clothing<span>store</span><i>®</i></Link>
   <nav className={open?"nav open":"nav"} aria-label="Điều hướng chính">
    <Link className={path==="/"?"active":""} href="/" onClick={()=>setOpen(false)}>Trang chủ</Link>
    <Link className={path.startsWith("/san-pham")?"active":""} href="/san-pham" onClick={()=>setOpen(false)}>Cửa hàng</Link>
    <Link href="/san-pham?search=áo" onClick={()=>setOpen(false)}>Áo</Link>
    <Link href="/san-pham?search=quần" onClick={()=>setOpen(false)}>Quần</Link>
    <Link href="/don-hang" onClick={()=>setOpen(false)}>Đơn hàng</Link>
    {user?.isAdmin&&<Link href="/quan-tri" onClick={()=>setOpen(false)}>Quản trị</Link>}
   </nav>
   <div className="header-actions">
    {user?<button className="text-button account-action" onClick={()=>void logout()} title={user.email}>Đăng xuất</button>:<Link href="/tai-khoan" className="icon-link" aria-label="Đăng nhập"><UserRound size={21}/></Link>}
    <Link href="/gio-hang" className="bag-link" aria-label="Giỏ hàng"><ShoppingBag size={21}/><span>Giỏ hàng</span></Link>
    <button className="menu-toggle icon-link" aria-label={open?"Đóng menu":"Mở menu"} onClick={()=>setOpen(!open)}>{open?<X/>:<Menu/>}</button>
   </div>
  </header>
 </>;
}
