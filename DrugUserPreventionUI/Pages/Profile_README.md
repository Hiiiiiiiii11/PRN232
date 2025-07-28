# Trang Hồ Sơ Cá Nhân (Profile Page)

## Tổng quan
Trang hồ sơ cá nhân cho phép người dùng xem và cập nhật thông tin cá nhân của mình.

## Tính năng chính

### 1. Thông tin cá nhân
- Xem và cập nhật họ tên, email, số điện thoại
- Cập nhật ngày sinh và giới tính
- Hiển thị vai trò và trạng thái tài khoản

### 2. Quản lý Avatar
- Upload ảnh đại diện
- Preview ảnh trước khi upload
- Validation file type và size

### 3. Bảo mật
- Đổi mật khẩu
- Xem lịch sử đăng nhập
- Quản lý phiên đăng nhập

### 4. Hoạt động
- Timeline các hoạt động gần đây
- Thống kê khóa học và cuộc hẹn

## Cách sử dụng

### Truy cập trang Profile
1. Đăng nhập hệ thống
2. Click vào avatar/tên người dùng ở góc phải
3. Chọn "Hồ sơ cá nhân"

### Cập nhật thông tin
1. Vào tab "Thông tin cá nhân"
2. Sửa các thông tin cần thiết
3. Click "Lưu thay đổi"

### Đổi mật khẩu
1. Vào tab "Bảo mật"
2. Nhập mật khẩu hiện tại
3. Nhập mật khẩu mới và xác nhận
4. Click "Đổi mật khẩu"

### Upload Avatar
1. Click vào icon camera trên ảnh đại diện
2. Chọn file ảnh (JPG, PNG, tối đa 2MB)
3. Preview và click "Tải lên"

## API Endpoints

### GET /api/User/{id}
Lấy thông tin người dùng

### PUT /api/User/{id}
Cập nhật thông tin cá nhân

### POST /api/User/{id}/change-password
Đổi mật khẩu

### PUT /api/User/{id}/avatar
Cập nhật ảnh đại diện

## Bảo mật
- Chỉ user mới có thể cập nhật profile của chính mình
- Password được hash bằng ASP.NET Core Identity
- File upload có validation type và size
- CSRF protection với Razor Pages

## Lỗi thường gặp

### "Không thể xác định người dùng hiện tại"
- Kiểm tra đã đăng nhập chưa
- Kiểm tra JWT token còn hạn không

### "Mật khẩu hiện tại không đúng"
- Đảm bảo nhập đúng mật khẩu cũ
- Kiểm tra caps lock

### "Chỉ chấp nhận file ảnh định dạng JPG, JPEG, PNG"
- Upload đúng định dạng ảnh
- Kiểm tra extension file

### "Kích thước file không được vượt quá 2MB"
- Nén ảnh trước khi upload
- Chọn ảnh có kích thước nhỏ hơn 