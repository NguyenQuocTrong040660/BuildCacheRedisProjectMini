# Ghi chú Khắc phục Sự cố Triển khai (Troubleshooting)

Tài liệu này ghi lại chi tiết vấn đề gặp phải trong quá trình giả lập triển khai thủ công tại thư mục `D:\9. BuildImageSourcceToDocker` và cách giải quyết.

---

## 1. Vấn đề gặp phải (The Issue)

Khi chạy lệnh `docker compose up -d` tại thư mục giả lập server `D:\9. BuildImageSourcceToDocker`, bạn gặp phải các lỗi sau:

1. **Lỗi 1 (Không tìm thấy Dockerfile):**
   ```text
   failed to solve: failed to read dockerfile: open Dockerfile: no such file or directory
   ```
2. **Lỗi 2 (Bị từ chối tải Image từ Docker Hub):**
   ```text
   web-api Error pull access denied for buildcacheredisprojectmini, repository does not exist or may require 'docker login'
   ```

---

## 2. Nguyên nhân gốc rễ (Root Cause)

Có 3 lý do đồng thời gây ra lỗi trên:

1. **Chưa nạp Image vào Docker:** Lúc chạy lệnh khởi động lần đầu, Image của API (`buildcacheredisprojectmini:latest`) chưa được nạp (load) vào Docker của hệ thống.
2. **Docker Compose cố gắng Build từ đầu:** Do lúc đó file `docker-compose.yml` vẫn cấu hình block `build:` (yêu cầu Docker tìm file `Dockerfile` ở thư mục hiện tại để build). Tuy nhiên, thư mục giả lập server `D:\9. BuildImageSourcceToDocker` là thư mục deploy sạch, chỉ có file `.tar` và `docker-compose.yml` chứ **không chứa mã nguồn hay file `Dockerfile`**.
3. **Cơ chế Fallback của Docker Compose:** Khi không tìm thấy `Dockerfile` để build tại chỗ, Docker Compose tự động chuyển hướng tìm kiếm và cố gắng tải (pull) image `buildcacheredisprojectmini` từ kho chứa public Docker Hub trên internet. Vì đây là image nội bộ chưa đẩy lên Docker Hub, hệ thống báo lỗi `pull access denied`.

---

## 3. Cách chúng ta đã giải quyết (The Solution)

Chúng ta đã khắc phục lỗi này bằng cách thiết lập lại đúng quy trình chạy độc lập không phụ thuộc mã nguồn:

### Bước 1: Nạp Image thủ công từ file `.tar`
Chạy lệnh nạp ảnh đã đóng gói từ local trước khi khởi động:
```cmd
docker load -i buildcacheredisprojectmini.tar
```
*Kết quả:* Docker thông báo `Loaded image: buildcacheredisprojectmini:latest`. Lúc này, hệ thống đã lưu trữ ảnh này ở bộ nhớ đệm local, không cần build hay pull trên mạng nữa.

### Bước 2: Loại bỏ hoàn toàn cơ chế Build trong file cấu hình
Đảm bảo file `docker-compose.yml` tại thư mục deploy (`D:\9. BuildImageSourcceToDocker`) **không chứa các dòng cấu hình build**:
```yaml
  web-api:
    image: buildcacheredisprojectmini:latest
    # ĐÃ XÓA/COMMENT PHẦN DƯỚI ĐÂY:
    # build:
    #   context: .
    #   dockerfile: Dockerfile
```

### Bước 3: Khởi chạy lại Docker Compose
Sau khi đã nạp image và dọn dẹp file cấu hình, chạy lại lệnh:
```cmd
docker compose up -d
```
*Kết quả:* Docker Compose tìm thấy ngay image `buildcacheredisprojectmini:latest` đã nạp ở Bước 1 ở dưới local, tự động khởi chạy 3 container (`web-api`, `redis-cache`, `portainer`) thành công mà không báo lỗi nữa.

### Bước 4: Kiểm tra kết quả thực tế
* Gọi HTTP GET tới `http://localhost:8080/health` trả về kết quả **Healthy**.
* Log của Container `web-api-1` ghi nhận đầy đủ luồng request thông qua Serilog:
  ```text
  [09:43:15 INF] HTTP GET /health responded 200 in 29.2509 ms
  ```

---

## 4. Bài học kinh nghiệm cho Production thật
Khi sang server Production thật:
1. Luôn chạy `docker load -i <tên_file>.tar` **trước** khi chạy `docker compose up -d`.
2. Đảm bảo file `docker-compose.yml` ở server **chỉ khai báo dòng `image: ...`** và tuyệt đối không khai báo dòng `build: ...`.
