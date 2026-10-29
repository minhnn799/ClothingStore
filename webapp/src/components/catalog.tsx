"use client";
import { useEffect,useState } from "react";
import { useSearchParams } from "next/navigation";
import { Search,SlidersHorizontal,ArrowLeft,ArrowRight } from "lucide-react";
import { api } from "@/lib/api";
import type { Product,Page,Lookup } from "@/lib/types";
import { Loading,ErrorBox,Empty,ProductCard } from "./ui";
export function Featured() {
 const [data,setData]=useState<Page<Product>|null>(null);const [error,setError]=useState("");
 async function load(){try{setError("");setData(await api<Page<Product>>("products?pageSize=4"))}catch(e){setError((e as Error).message)}}
 useEffect(()=>{void load()},[]);
 if(error)return <ErrorBox message={error} retry={()=>void load()}/>;
 if(!data)return <Loading/>;
 if(!data.items.length)return <div className="quiet-empty"><span>BỘ SƯU TẬP ĐANG ĐƯỢC CHUẨN BỊ</span><h3>Những điều mới mẻ sắp đến.</h3><p>Sản phẩm sẽ xuất hiện tại đây khi cửa hàng cập nhật danh mục.</p></div>;
 return <div className="product-grid">{data.items.map(p=><ProductCard key={p.id} product={p}/>)}</div>;
}
export function Catalog(){
 const params=useSearchParams();const initial=params.get("search")||"";
 const [search,setSearch]=useState(initial);const [query,setQuery]=useState(initial);const [category,setCategory]=useState("");const [brand,setBrand]=useState("");const [page,setPage]=useState(1);
 const [categories,setCategories]=useState<Lookup[]>([]);const [brands,setBrands]=useState<Lookup[]>([]);const [data,setData]=useState<Page<Product>|null>(null);const [error,setError]=useState("");const [busy,setBusy]=useState(true);const [revision,setRevision]=useState(0);
 useEffect(()=>{setSearch(initial);setQuery(initial);setPage(1)},[initial]);
 useEffect(()=>{Promise.all([api<Lookup[]>("categories"),api<Lookup[]>("brands")]).then(([c,b])=>{setCategories(c);setBrands(b)}).catch(()=>{})},[]);
 useEffect(()=>{let alive=true;setBusy(true);setError("");const q=new URLSearchParams({search:query,page:String(page),pageSize:"12"});if(category)q.set("categoryId",category);if(brand)q.set("brandId",brand);
 api<Page<Product>>("products?"+q).then(d=>{if(alive)setData(d)}).catch(e=>{if(alive)setError(e.message)}).finally(()=>{if(alive)setBusy(false)});return()=>{alive=false}},[query,category,brand,page,revision]);
 return <div className="section catalog"><div className="page-heading"><div className="eyebrow">TỦ ĐỒ THƯỜNG NGÀY</div><h1>Chọn điều hợp với bạn.</h1><p>Khám phá những lựa chọn cho phong cách của riêng bạn.</p></div>
 <div className="catalog-toolbar"><form className="search-field" onSubmit={e=>{e.preventDefault();setPage(1);setQuery(search)}}><Search size={18}/><input aria-label="Tìm sản phẩm" placeholder="Tìm tên sản phẩm…" value={search} onChange={e=>setSearch(e.target.value)}/><button type="submit">Tìm</button></form><div className="filter-fields"><SlidersHorizontal size={18}/><select aria-label="Danh mục" value={category} onChange={e=>{setCategory(e.target.value);setPage(1)}}><option value="">Tất cả danh mục</option>{categories.map(c=><option key={c.id} value={c.id}>{c.name}</option>)}</select><select aria-label="Thương hiệu" value={brand} onChange={e=>{setBrand(e.target.value);setPage(1)}}><option value="">Tất cả thương hiệu</option>{brands.map(b=><option key={b.id} value={b.id}>{b.name}</option>)}</select></div></div>
 <div className="results-bar"><span>{data?.total??0} sản phẩm</span>{(query||category||brand)&&<button className="text-button" onClick={()=>{setQuery("");setSearch("");setCategory("");setBrand("");setPage(1)}}>Xóa bộ lọc</button>}</div>
 {busy?<Loading/>:error?<ErrorBox message={error} retry={()=>setRevision(r=>r+1)}/>:!data?.items.length?<Empty title="Chưa tìm thấy sản phẩm" description="Hãy thử một từ khóa khác hoặc quay lại khi cửa hàng cập nhật sản phẩm."/>:<div className="product-grid">{data.items.map(p=><ProductCard key={p.id} product={p}/>)}</div>}
 {data&&data.total>12&&<div className="pagination"><button className="button secondary" disabled={page===1||busy} onClick={()=>setPage(page-1)}><ArrowLeft size={16}/>Trước</button><span>Trang {page} / {Math.ceil(data.total/12)}</span><button className="button secondary" disabled={page*12>=data.total||busy} onClick={()=>setPage(page+1)}>Tiếp<ArrowRight size={16}/></button></div>}
 </div>;
}
