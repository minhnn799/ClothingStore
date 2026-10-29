export function translateError(data,status) {
 const text=String(data?.message??data?.title??"");
 /** @type {Array<[RegExp,string]>} */
 const known=[
 [/Invalid email|Invalid credentials|Unauthorized|expired session/i,"Email hoặc mật khẩu không đúng, hoặc phiên đăng nhập đã hết hạn."],
 [/already taken|already exists/i,"Thông tin này đã được sử dụng. Vui lòng kiểm tra lại."],
 [/Insufficient stock|insufficient stock|unavailable/i,"Sản phẩm không còn đủ hàng. Vui lòng cập nhật giỏ hàng."],
 [/Cart is empty/i,"Giỏ hàng đang trống."],
 [/transition|Only pending/i,"Không thể thay đổi trạng thái của đơn hàng này."],
 [/hierarchy|cycle/i,"Danh mục cha không hợp lệ."],
 [/compare-at/i,"Giá gốc phải lớn hơn hoặc bằng giá bán."],
 [/Image|PNG|JPEG|WebP/i,"Vui lòng chọn ảnh PNG, JPEG hoặc WebP không quá 5 MB."],
 [/Password/i,"Mật khẩu cần ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt."],
 [/in use/i,"Dữ liệu đang được sử dụng nên chưa thể xóa."],
 ];
 for(const [pattern,message] of known) if(pattern.test(text)) return message;
 if(status===400) return "Vui lòng kiểm tra thông tin đã nhập.";
 if(status===401) return "Vui lòng đăng nhập để tiếp tục.";
 if(status===403) return "Bạn không có quyền thực hiện thao tác này.";
 if(status===404) return "Không tìm thấy thông tin yêu cầu.";
 if(status===409) return "Dữ liệu vừa thay đổi. Vui lòng tải lại và thử lại.";
 if(status===413) return "Tệp tải lên quá lớn. Giới hạn là 5 MB.";
 return "Không thể kết nối lúc này. Vui lòng thử lại sau.";
}
