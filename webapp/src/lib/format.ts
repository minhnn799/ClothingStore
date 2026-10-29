export const money = (value:number) => new Intl.NumberFormat("vi-VN", {style:"currency",currency:"VND",maximumFractionDigits:0}).format(value);
export const date = (value:string) => new Intl.DateTimeFormat("vi-VN",{day:"2-digit",month:"2-digit",year:"numeric"}).format(new Date(value));
export const statuses:Record<string,string> = {Pending:"Chờ xác nhận",Processing:"Đang chuẩn bị",Shipped:"Đang giao",Delivered:"Đã giao",Cancelled:"Đã hủy"};
export function imageUrl(url?:string) {
 if (!url) return "";
 if (url.startsWith("/uploads/")) return "/media/" + url.slice("/uploads/".length);
 return /^https:\/\//.test(url) ? url : "";
}
