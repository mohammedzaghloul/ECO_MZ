# ECO — Full-Stack E-Commerce Platform

A full-stack e-commerce platform built with **Angular 21** and **.NET 10**, covering the complete shopping journey — from browsing products to checkout, payments, order tracking, and a full admin dashboard — plus a built-in **landing-page builder** with WhatsApp ordering and analytics.

## Features

### Storefront (Angular)
- Product catalog with search, filtering, sorting, and product specifications
- Product details with image galleries and zoom
- Shopping basket with live stock validation
- Checkout with delivery methods and city-based shipping (Egypt governorates & cities catalog)
- Guest checkout and WhatsApp ordering via landing pages
- Order tracking and order details
- Wishlist and product reviews
- Multi-language support (Arabic / English) with i18n
- PWA support (service worker) and offline-friendly behavior

### Identity & Security
- Register / login / email activation / password reset
- JWT access tokens with refresh-token rotation
- Role-based authorization (Admin / Customer)
- Google sign-in readiness
- Centralized exception-handling middleware

### Payments & Discounts
- Stripe Payment Intents integration
- Coupons and discount system applied to orders

### Admin Dashboard
- Sales dashboard with KPIs and analytics
- Product, category, and inventory management (stock tracking, dimensions)
- Customer and order management
- Discounts, delivery methods, and city shipping settings
- Store settings with customizable SMTP email settings
- **Landing-page builder**: create public product landing pages with customization, visit analytics, events, and outgoing webhooks

### Notifications & Email
- In-app notifications
- Transactional emails (order confirmation, activation, password reset) with customizable email appearance/text/theme settings

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 21, TypeScript 5.9, RxJS, i18n, PWA |
| Backend | ASP.NET Core (.NET 10), REST API |
| Data Access | Entity Framework Core, Specification pattern |
| Database | SQL Server |
| Caching | Redis (basket, performance) |
| Payments | Stripe |
| Auth | ASP.NET Core Identity + JWT |

## Project Structure

```
├── Client/            # Angular storefront + admin dashboard
│   └── src/app/
│       ├── core/      # navbar, footer, interceptors, shared services
│       ├── shared/    # models, components, pipes, shared services
│       ├── shop/      # product listing & details
│       ├── basket/    # basket feature
│       ├── checkout/  # checkout flow
│       ├── identity/  # auth pages
│       ├── admin/     # admin dashboard
│       └── landing-*  # landing-page builder & public view
├── ECO.Api/           # ASP.NET Core API layer (controllers, middleware)
├── ECO.BLL/           # business logic (services, DTOs, mapping)
└── ECO.DAL/           # data access (EF Core, entities, repositories, migrations)
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 20+ and npm
- SQL Server (LocalDB or full)
- Redis

### Backend
```bash
# restore & build
dotnet build ECO.slnx

# update the connection string in ECO.Api/appsettings.json, then apply migrations
dotnet ef database update --project ECO.DAL --startup-project ECO.Api

# run the API
dotnet run --project ECO.Api
```

### Frontend
```bash
cd Client
npm install
npm start          # dev server on http://localhost:4200 (proxied to the API)
```

### Configuration
Secrets are **not** committed. Fill in your own values in `ECO.Api/appsettings.json` (or use user-secrets / environment variables):
- Database connection string
- JWT token secret
- Stripe keys
- SMTP credentials (see [docs/SMTP-Gmail-App-Password-Ar.md](docs/SMTP-Gmail-App-Password-Ar.md) for a Gmail app-password guide)

## Roadmap / Notes
- Automated tests (unit + integration) are planned next.
- CI/CD pipeline to be added.
