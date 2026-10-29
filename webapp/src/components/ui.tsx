"use client";
import Link from "next/link";
import { ArrowRight, Shirt, LoaderCircle, RefreshCw } from "lucide-react";
import { imageUrl,money } from "@/lib/format";
import type { Product } from "@/lib/types";
export function Loading(){return <div className="loading" role="status"><LoaderCircle className="spin" size={24}/>Đang tải…</div>}
export function ErrorBox({message,retry}:{message:string;retry?:()=>void}){return <div className="error-box" role="alert">{message}{retry&&<button className="text-button" onClick={retry}><RefreshCw size={15}/>Thử lại</button>}</div>}
export function Empty({title,description,href="/san-pham",label="Khám phá sản phẩm"}:{title:string;description:string;href?:string;label?:string}){return <div className="empty"><ShoppingIcon/><h2>{title}</h2><p>{description}</p><Link href={href} className="button">{label}<ArrowRight size={17}/></Link></div>}
function ShoppingIcon(){return <Shirt size={42} strokeWidth={1}/>}
export function ProductCard({product}:{product:Product}) {
 const prices=product.variants.filter(v=>v.isActive).map(v=>v.price);const price=prices.length?Math.min(...prices):null;
 const photo=imageUrl((product.images.find(i=>i.isPrimary)||product.images[0])?.url);
 const available=product.variants.some(v=>v.isActive&&v.stockQuantity>0);
 const colors=[...new Set(product.variants.map(v=>v.color))];
 return <Link className="product-card" href={"/san-pham/"+product.id}>
  <div className="product-photo">{photo?<img src={photo} alt={product.name} loading="lazy"/>:<div className="product-placeholder"><Shirt strokeWidth={.65}/><span>{product.category}</span></div>}
   {!available&&<span className="product-tag">Tạm hết hàng</span>}<span className="quick-view">Xem sản phẩm <ArrowRight size={16}/></span>
  </div>
  <div className="product-meta"><span>{product.category}</span><span>{colors.length?colors.length+" màu":""}</span></div>
  <h3>{product.name}</h3><div className="price">{price===null?"Chưa có giá":money(price)}</div>
 </Link>;
}
