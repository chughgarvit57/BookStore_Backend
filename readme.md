# 📚 BookStore API

This is a multi-layered ASP.NET Core Web API project designed for managing an online bookstore. It follows a clean architecture pattern with separate layers for Models, Business Logic, and Repository. The API supports user registration, book management, cart operations, order processing, address management, and wishlist functionalities.

---

## 🧱 Project Structure

```
BookStore/
│
├── BackendStore/              # Main API entry point (Controllers, Program.cs, config)
├── BusinessLayer/            # Business logic (services, interfaces)
├── ModelLayer/              # Domain models and enums
└── RepositoryLayer/         # Data access, DTOs, context, interfaces, migrations
```

---

## ✨ Getting Started

### Prerequisites

* [.NET 6 SDK or later](https://dotnet.microsoft.com/download)
* SQL Server (or any other DB used)

### Setup Instructions

1. Clone the repository:

   ```bash
   git clone https://github.com/your-username/bookstore-api.git
   cd bookstore-api
   ```

2. Restore NuGet packages:

   ```bash
   dotnet restore
   ```

3. Update `appsettings.json` with your DB connection string.

4. Apply migrations (if using EF Core):

   ```bash
   dotnet ef database update --project RepositoryLayer
   ```

5. Run the API:

   ```bash
   dotnet run --project BackendStore
   ```

6. Swagger UI is available at:

   ```
   https://localhost:{port}/swagger
   ```

---

## 📚 API Endpoints

### 🔐 User

* `POST /api/User/Register` – Register a new user
* `POST /api/User/Login` – Login and get token
* `PATCH /api/User/ResetPassword` – Reset user password
* `POST /api/User/ForgotPassword` – Request password reset
* `DELETE /api/User/DeleteUser` – Delete user account
* `GET /api/User/GetAllUsers` – Get all registered users

---

### 📦 Book

* `POST /api/Book/AddBook` – Add a new book
* `GET /api/Book/GetAllBooks` – Retrieve all books
* `GET /api/Book/BookId` – Get details of a book by ID

---

### 🛒 Cart

* `POST /api/Cart/AddToCart` – Add book to cart
* `GET /api/Cart/GetCart` – View user's cart
* `PATCH /api/Cart/UpdateCart` – Update quantity in cart
* `DELETE /api/Cart/RemoveFromCart` – Remove a specific book
* `DELETE /api/Cart/ClearCart` – Clear entire cart

---

### 📬 Address

* `POST /api/Address/AddAddress` – Add user address
* `GET /api/Address/GetAllAddresses` – Get all addresses
* `PUT /api/Address/UpdateAddress` – Update an address
* `DELETE /api/Address/AddressId` – Delete address by ID

---

### 📜 Order

* `POST /api/Order/CreateOrder` – Create an order
* `GET /api/Order/GetAllOrders` – View all orders

---

### 💖 WishList

* `POST /api/WishList/AddBookToWishList` – Add book to wishlist
* `GET /api/WishList/GetAllBooksInWishList` – View wishlist
* `DELETE /api/WishList/RemoveBookFromWishList` – Remove book from wishlist
* `DELETE /api/WishList/ClearWishList` – Clear entire wishlist

---

## 🧰 Technologies Used

* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* Swagger (OpenAPI)
* NLog for logging

---

## 📦 Folder-Specific Notes

* `ConsumerLayer`: Contains RabbitMQ consumer logic. If you are not using background tasks, this can be removed.
* `RepositoryLayer`: Manages data access logic and database operations.
* `BusinessLayer`: Contains core business logic, services, and interfaces.
* `ModelLayer`: Holds all domain entities and enums.

---

## ✅ Authorization

Most endpoints are protected and require a valid JWT token for access. Ensure you log in via `/api/User/Login` to retrieve your token.

---

## 📄 License

MIT License – Feel free to use and modify.

---

## ✨ Author

Garvit Chugh – [LinkedIn](https://www.linkedin.com/in/chughgarvit09/)