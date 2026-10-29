import Link from "next/link";
import { ArrowUpRight, ArrowRight, Truck, PackageCheck, ShoppingBag } from "lucide-react";
import { Featured } from "@/components/catalog";
export default function Home() {
 return <>
 <section className="hero">
  <div className="hero-copy"><div className="eyebrow"><span className="tiny-line"/> PHONG CÁCH THƯỜNG NGÀY</div><h1>Mặc đơn giản.<br/><em>Sống trọn vẹn.</em></h1><p>Những lựa chọn dễ mặc, dễ yêu.<br/>Để bạn luôn thoải mái là chính mình.</p><Link href="/san-pham" className="button">Khám phá cửa hàng <ArrowUpRight size={20}/></Link><div className="hero-foot"><span>01 — TỦ ĐỒ CỦA BẠN</span><span>Ít hơn, nhưng tốt hơn.</span></div></div>
  <div className="hero-art"><img src="/illustrations/wardrobe.svg" alt="Minh họa áo khoác nâu và áo thun kem trong tủ đồ tối giản"/><div className="hero-stamp">TỦ ĐỒ<br/><b>MỖI NGÀY</b><span>Trang phục cho mỗi ngày</span></div><div className="art-caption"><span>TỐI GIẢN · TỰ NHIÊN · LÀ BẠN</span><ArrowUpRight size={20}/></div></div>
 </section>
 <div className="benefits"><div><Truck size={21}/><span>Miễn phí giao hàng</span></div><div><PackageCheck size={21}/><span>Thanh toán khi nhận hàng</span></div><div><ShoppingBag size={21}/><span>Theo dõi đơn hàng dễ dàng</span></div></div>
 <section className="section"><div className="section-heading"><div><div className="eyebrow">BẮT ĐẦU TỪ NHỮNG ĐIỀU CƠ BẢN</div><h2>Hôm nay, bạn muốn mặc gì?</h2></div><span className="muted small">Một chút cảm hứng cho tủ đồ của bạn.</span></div>
 <div className="collection-grid"><Link href="/san-pham?search=áo" className="collection sand"><img src="/illustrations/tee.svg" alt="Minh họa áo thun"/><div><span>THOẢI MÁI MỖI NGÀY</span><h3>Áo & chất riêng</h3></div><ArrowUpRight/></Link><Link href="/san-pham?search=quần" className="collection sage"><img src="/illustrations/trousers.svg" alt="Minh họa quần dài"/><div><span>DỄ DÀNG KẾT HỢP</span><h3>Quần & chuyển động</h3></div><ArrowUpRight/></Link></div></section>
 <section className="section featured-section"><div className="section-heading"><div><div className="eyebrow">TÌM THẤY ĐIỀU BẠN THÍCH</div><h2>Từ cửa hàng của chúng mình</h2></div><Link className="underlined" href="/san-pham">Xem tất cả <ArrowRight size={17}/></Link></div><Featured/></section>
 <section className="manifesto"><span className="eyebrow">CLOTHINGSTORE / TINH THẦN TỐI GIẢN</span><h2>Phong cách đẹp nhất<br/>là khi bạn thấy <em>thoải mái.</em></h2><p>Không cần một tủ đồ thật lớn. Chỉ cần những món đồ thật hợp với bạn.</p><Link href="/san-pham" className="underlined">Tìm phong cách của bạn <ArrowUpRight size={18}/></Link></section>
 </>;
}
