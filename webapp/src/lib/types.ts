export type Lookup = {id:string;name:string;slug:string;parentId?:string};
export type Variant = {id:string;sku:string;size:string;color:string;material?:string;price:number;compareAtPrice?:number;stockQuantity:number;isActive:boolean};
export type Product = {id:string;name:string;slug:string;sku:string;description?:string;brandId:string;brand:string;categoryId:string;category:string;isActive:boolean;variants:Variant[];images:{id:string;url:string;isPrimary:boolean;sortOrder:number}[]};
export type Page<T> = {items:T[];total:number;page:number;pageSize:number};
export type Cart = {items:{productVariantId:string;productName:string;sku:string;size:string;color:string;unitPrice:number;quantity:number;total:number;available:boolean}[];subtotal:number};
export type Order = {id:string;status:string;customerName:string;customerPhone:string;shippingAddress:string;subtotal:number;discount:number;shippingFee:number;total:number;createdAt:string;items:{productVariantId:string;productName:string;sku:string;unitPrice:number;quantity:number;total:number}[]};
export type User = {email:string;isAdmin:boolean};
