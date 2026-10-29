"use client";
import {useEffect,useState} from "react";import Link from "next/link";import {ArrowLeft,ShoppingBag,Truck,Check,Shirt} from "lucide-react";
import {api} from "@/lib/api";import {imageUrl,money} from "@/lib/format";import type {Product} from "@/lib/types";import {useSession} from "./session";import {Loading,ErrorBox} from "./ui";
export function ProductDetail({id}:{id:string}) {
 const {user}=useSession();const [product,setProduct]=useState<Product|null>(null);const [variantId,setVariantId]=useState("");const [quantity,setQuantity]=useState(1);const [photo,setPhoto]=useState(0);const [error,setError]=useState("");const [busy,setBusy]=useState(false);const [added,setAdded]=useState(false);
 useEffect(()=>{api<Product>("products/"+id).then(p=>{setProduct(p);setVariantId(p.variants.find(v=>v.stockQuantity>0)?.id||p.variants[0]?.id||"")}).catch(e=>setError(e.message))},[id]);
 const variant=product?.variants.find(v=>v.id===variantId);
 async function add(){if(!variant)return;setBusy(true);setError("");setAdded(false);try{await api("cart/items",{method:"PUT",body:JSON.stringify({productVariantId:variant.id,quantity})});setAdded(true)}catch(e){setError((e as Error).message)}finally{setBusy(false)}}
 if(!product)return error?<div className="section"><ErrorBox message={error}/></div>:<Loading/>;
 const images=[...product.images].sort((a,b)=>Number(b.isPrimary)-Number(a.isPrimary)||a.sortOrder-b.sortOrder);
 const src=imageUrl(images[photo]?.url);
 return <div className="section"><Link className="back-link" href="/san-pham"><ArrowLeft size={16}/>Trở lại cửa hàng</Link><div className="detail-grid"><div><div className="detail-photo">{src?<img src={src} alt={product.name}/>:<div className="product-placeholder"><Shirt strokeWidth={.6}/><span>Ảnh sản phẩm đang được cập nhật</span></div>}</div>{images.length>1&&<div className="thumbnails">{images.map((i,n)=><button key={i.id} className={photo===n?"selected":""} onClick={()=>setPhoto(n)} aria-label={"Xem ảnh "+(n+1)}><img src={imageUrl(i.url)} alt=""/></button>)}</div>}</div>
 <div className="detail-copy"><div className="eyebrow">{product.brand} / {product.category}</div><h1>{product.name}</h1><div className="detail-price">{variant?money(variant.price):"Chưa có giá"}{variant?.compareAtPrice&&variant.compareAtPrice>variant.price&&<del>{money(variant.compareAtPrice)}</del>}</div><p className="description">{product.description||"Thông tin sản phẩm đang được cập nhật."}</p>
 <fieldset className="variants"><legend>Chọn kích cỡ & màu sắc</legend>{product.variants.map(v=><button key={v.id} type="button" className={v.id===variantId?"variant selected":"variant"} aria-pressed={v.id===variantId} onClick={()=>{setVariantId(v.id);setAdded(false);setQuantity(1)}}>{v.size} · {v.color}{v.stockQuantity===0&&<small>Hết hàng</small>}</button>)}</fieldset>
 {variant?.material&&<p className="small muted">Chất liệu: {variant.material}</p>}
 <div className="stock-note">{variant&&variant.stockQuantity>0?<><span className="stock-dot"/>Còn {variant.stockQuantity} sản phẩm</>:"Tạm hết hàng"}</div>
 <label className="quantity-label">Số lượng <input type="number" min="1" max={Math.min(100,variant?.stockQuantity||1)} value={quantity} onChange={e=>{setQuantity(Number(e.target.value));setAdded(false)}}/></label>
 {error&&<ErrorBox message={error}/>}
 {user?<button className="button wide" disabled={busy||!variant||variant.stockQuantity===0||quantity<1||quantity>Math.min(100,variant.stockQuantity)||!Number.isInteger(quantity)} onClick={()=>void add()}>{added?<Check size={18}/>:<ShoppingBag size={18}/>} {busy?"Đang cập nhật…":added?"Đã cập nhật giỏ hàng":"Thêm vào giỏ hàng"}</button>:<Link href={"/tai-khoan?next="+encodeURIComponent("/san-pham/"+id)} className="button wide">Đăng nhập để mua hàng</Link>}
 {added&&<Link href="/gio-hang" className="underlined success-link">Xem giỏ hàng của bạn →</Link>}
 <div className="detail-service"><Truck size={22}/><div><b>Giao hàng miễn phí</b><span>Thanh toán khi nhận hàng. Theo dõi đơn trong tài khoản.</span></div></div>
 </div></div></div>;
}
