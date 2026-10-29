import type { Metadata } from "next";
import Link from "next/link";
import { SessionProvider } from "@/components/session";
import { Header } from "@/components/header";
import "./globals.css";
export const metadata:Metadata={title:{default:"ClothingStore — Mặc đơn giản. Sống trọn vẹn.",template:"%s | ClothingStore"},description:"Khám phá phong cách thường ngày cùng ClothingStore. Chọn trang phục, đặt hàng và thanh toán khi nhận hàng."};
export default function RootLayout({children}:{children:React.ReactNode}) {
 return <html lang="vi"><body><SessionProvider><Header/><main>{children}</main><footer className="footer"><div className="footer-top"><div><Link className="wordmark" href="/">clothing<span>store</span><i>®</i></Link><p>Ít cầu kỳ hơn.<br/>Nhiều chất riêng hơn.</p></div><div><h3>Khám phá</h3><Link href="/san-pham">Tất cả sản phẩm</Link><Link href="/san-pham?search=áo">Áo thường ngày</Link><Link href="/san-pham?search=quần">Quần & phong cách</Link></div><div><h3>Tài khoản</h3><Link href="/tai-khoan">Đăng nhập / Đăng ký</Link><Link href="/gio-hang">Giỏ hàng của bạn</Link><Link href="/don-hang">Theo dõi đơn hàng</Link></div><div className="footer-note"><span>ĐƠN GIẢN, TỪ LỰA CHỌN ĐẾN TRẢI NGHIỆM.</span><p>Giao hàng miễn phí.<br/>Thanh toán khi nhận hàng.</p></div></div><div className="footer-bottom"><span>© {new Date().getFullYear()} ClothingStore</span><span>Được chọn để đồng hành mỗi ngày.</span><span>Việt Nam · VND</span></div></footer></SessionProvider></body></html>;
}
