# API Authentication Testing Guide

## 📝 Tổng quan các thay đổi

### 1. **Login Endpoint** - Đăng nhập linh hoạt
- Hỗ trợ đăng nhập bằng **username HOẶC email**
- Tự động cập nhật `LastLoginAt` khi đăng nhập thành công
- Trả về đầy đủ thông tin user (bao gồm FullName, AvatarUrl)

### 2. **Register Endpoint** - Đăng ký với validation
- **Username validation**:
  - Chỉ chấp nhận: chữ cái không dấu (a-z, A-Z), số (0-9), và dấu gạch dưới (_)
  - Không được có khoảng trắng
  - Không được có ký tự tiếng Việt có dấu
  - Độ dài: 3-50 ký tự
- **Email validation**:
  - Phải là email hợp lệ
  - Tự động chuyển về lowercase
  - Kiểm tra trùng lặp
- **Password validation**:
  - Tối thiểu 6 ký tự

---

## 🧪 Test Cases

### 1. Test Register - Đăng ký thành công

```http
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "userName": "john_doe123",
  "password": "password123",
  "email": "john@example.com",
  "role": "Staff"
}
```

**Expected Response (200 OK):**
```json
{
  "message": "Đăng ký thành công",
  "userId": "guid-here",
  "userName": "john_doe123",
  "email": "john@example.com",
  "role": "Staff"
}
```

---

### 2. Test Register - Username có dấu (FAIL ❌)

```http
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "userName": "nguyễn_văn_a",
  "password": "password123",
  "email": "nguyen@example.com",
  "role": "Staff"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "errors": {
    "UserName": [
      "Username chỉ được chứa chữ cái không dấu, số và dấu gạch dưới (_), không được có khoảng trắng"
    ]
  }
}
```

---

### 3. Test Register - Username có khoảng trắng (FAIL ❌)

```http
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "userName": "john doe",
  "password": "password123",
  "email": "john@example.com",
  "role": "Staff"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "errors": {
    "UserName": [
      "Username chỉ được chứa chữ cái không dấu, số và dấu gạch dưới (_), không được có khoảng trắng"
    ]
  }
}
```

---

### 4. Test Register - Email trùng lặp (FAIL ❌)

```http
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "userName": "newuser",
  "password": "password123",
  "email": "john@example.com",  // Email đã tồn tại
  "role": "Staff"
}
```

**Expected Response (409 Conflict):**
```json
{
  "message": "Email đã được sử dụng"
}
```

---

### 5. Test Login - Đăng nhập bằng Username

```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "loginIdentifier": "john_doe123",
  "password": "password123"
}
```

**Expected Response (200 OK):**
```json
{
  "message": "Đăng nhập thành công",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "random-uuid-here",
  "expiresIn": 60,
  "user": {
    "userId": "guid",
    "userName": "john_doe123",
    "email": "john@example.com",
    "fullName": null,
    "avatarUrl": null,
    "role": "Staff"
  }
}
```

---

### 6. Test Login - Đăng nhập bằng Email

```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "loginIdentifier": "john@example.com",
  "password": "password123"
}
```

**Expected Response (200 OK):**
```json
{
  "message": "Đăng nhập thành công",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "random-uuid-here",
  "expiresIn": 60,
  "user": {
    "userId": "guid",
    "userName": "john_doe123",
    "email": "john@example.com",
    "fullName": null,
    "avatarUrl": null,
    "role": "Staff"
  }
}
```

---

### 7. Test Login - Sai mật khẩu (FAIL ❌)

```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "loginIdentifier": "john_doe123",
  "password": "wrongpassword"
}
```

**Expected Response (401 Unauthorized):**
```json
{
  "message": "Tên đăng nhập/Email hoặc mật khẩu không đúng"
}
```

---

## 📋 Username Validation Rules

### ✅ Valid Usernames:
- `john_doe`
- `user123`
- `admin_2024`
- `staff_warehouse`
- `JohnDoe123`

### ❌ Invalid Usernames:
- `nguyễn_văn_a` (có dấu tiếng Việt)
- `john doe` (có khoảng trắng)
- `user@123` (có ký tự đặc biệt @)
- `a` (quá ngắn, < 3 ký tự)
- `this_is_a_very_long_username_that_exceeds_fifty_characters` (quá dài > 50 ký tự)

---

## 🔐 Password Rules

- Tối thiểu: **6 ký tự**
- Có thể chứa bất kỳ ký tự nào (chữ, số, ký tự đặc biệt)
- Được hash bằng BCrypt trước khi lưu vào database

---

## 📧 Email Rules

- Phải đúng format email (có @, domain, etc.)
- Tự động chuyển về **lowercase** khi lưu
- Kiểm tra trùng lặp (case-insensitive)

---

## 🎯 Testing với curl

### Register:
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "password": "test123",
    "email": "test@example.com",
    "role": "Staff"
  }'
```

### Login với Username:
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "loginIdentifier": "testuser",
    "password": "test123"
  }'
```

### Login với Email:
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "loginIdentifier": "test@example.com",
    "password": "test123"
  }'
```

---

## 📝 Notes

1. **LastLoginAt** được tự động cập nhật mỗi khi user đăng nhập thành công
2. Email được lưu ở dạng **lowercase** để tránh trùng lặp
3. Username validation sử dụng **Regex**: `^[a-zA-Z0-9_]+$`
4. Tất cả validation error messages đều bằng tiếng Việt
