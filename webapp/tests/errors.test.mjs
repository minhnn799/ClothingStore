import test from "node:test";
import assert from "node:assert/strict";
import {translateError} from "../src/lib/errors.mjs";
test("customer-facing backend errors are Vietnamese",()=>{
 assert.match(translateError({message:"Insufficient stock."},409),/không còn đủ hàng/);
 assert.match(translateError({message:"Cart is empty."},400),/Giỏ hàng đang trống/);
 assert.match(translateError({message:"Password needs uppercase"},400),/Mật khẩu/);
 assert.match(translateError({message:"Unexpected server error."},500),/Không thể kết nối/);
 assert.match(translateError(null,401),/đăng nhập/);
});
