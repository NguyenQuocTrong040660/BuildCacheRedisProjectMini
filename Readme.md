# Hướng dẫn Khởi chạy Dự án Web API & Redis Cache với Docker

Tài liệu này hướng dẫn chi tiết cách cấu hình, khởi chạy dự án `.NET 8.0 Web API` tích hợp `Redis Cache` và giao diện quản lý `Portainer` thông qua Docker.

---

## 📌 Điều kiện tiên quyết (Prerequisites)

* Máy tính đã cài đặt **Docker Desktop**.
* Đảm bảo **Docker Desktop đã khởi động** (Trạng thái hiển thị màu xanh lá - *Engine running*).

---

## 📂 Thư mục chạy lệnh

Bạn phải mở Terminal (CMD, PowerShell, Git Bash hoặc Terminal trong VS Code) tại **thư mục gốc** của dự án:
```text
D:\1.RepoPrivate\8. RedisApp\BuildCacheRedisProjectMini
```

---

## 🚀 Các bước khởi chạy dự án

### Bước 1: Khởi động hệ thống Docker Compose
Tại thư mục gốc của dự án, chạy lệnh dưới đây để build Docker Image cho Web API và tải các container cần thiết về chạy ngầm:

```bash
docker-compose up --build -d
```

* **`--build`**: Tự động biên dịch mã nguồn của dự án Web API và tạo Docker Image mới nhất.
* **`-d`**: Chạy ngầm (Detached mode), giải phóng cửa sổ dòng lệnh cho bạn.

---

### Bước 2: Kiểm tra trạng thái hoạt động
Sau khi chạy xong lệnh trên, kiểm tra xem các container đã hoạt động bình thường chưa bằng lệnh:

```bash
docker ps
```

Bạn sẽ thấy 3 container đang chạy tương ứng với:
1. `web-api` (Cổng `8080`)
2. `redis-cache` (Cổng `6379`)
3. `portainer` (Cổng `9000` và `9443`)

---

### Bước 3: Truy cập các dịch vụ

Sau khi khởi chạy thành công, bạn mở trình duyệt và truy cập các đường dẫn sau:

* **Ứng dụng Web API (Swagger UI):**  
  👉 [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)  
  *Giao diện tương tác thử nghiệm các API như `setCacheKey`, `existCacheKey`, `removeCacheKey`...*

* **Health Check (Kiểm tra sức khỏe hệ thống & Redis):**  
  👉 [http://localhost:8080/health](http://localhost:8080/health)

* **API Lấy tất cả cache keys trên Redis:**  
  👉 [http://localhost:8080/weatherforecast/keys](http://localhost:8080/weatherforecast/keys)

* **Trình quản lý Docker (Portainer Admin UI):**  
  👉 [http://localhost:9000](http://localhost:9000) (hoặc [https://localhost:9443](https://localhost:9443))  
  *Thiết lập mật khẩu Admin trong lần đầu truy cập để giám sát toàn bộ tài nguyên Docker.*

* **Redis Server:**  
  Địa chỉ: `localhost` | Cổng: `6379` (không có mật khẩu).

---

## 🛑 Dừng hệ thống

Khi không muốn chạy dự án nữa, bạn chạy lệnh sau tại thư mục gốc để giải phóng tài nguyên máy tính:

```bash
docker-compose down
```
*Lưu ý: Dữ liệu cấu hình của Portainer vẫn sẽ được lưu trữ an toàn trong Docker Volume (`portainer_data`), không lo bị mất.*

---

## 🛠️ Xử lý lỗi thường gặp

1. **Lỗi: `error during connect: ... open //./pipe/dockerDesktopLinuxEngine`**
   * **Nguyên nhân:** Docker Desktop chưa được mở hoặc đang khởi động.
   * **Cách xử lý:** Hãy mở Docker Desktop trên máy Windows của bạn và đợi trạng thái chuyển sang màu xanh lá trước khi chạy lệnh docker.

2. **Lỗi: Trùng cổng (Port Conflict)**
   * **Nguyên nhân:** Cổng `8080`, `6379` hoặc `9000` đã có ứng dụng khác trên máy của bạn sử dụng.
   * **Cách xử lý:** Kiểm tra và tắt ứng dụng đang chiếm cổng đó (Ví dụ: Redis chạy ngầm trên Windows) rồi thử lại.
